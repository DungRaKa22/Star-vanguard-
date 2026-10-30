#!/usr/bin/env python3
"""
Star Vanguard — static project audit (no Unity Editor required).

Muc dich: phat hien som cac loi serialization / kien truc / cau hinh ma
Unity chi bao rat muon (hoac bao rat mo ho) khi mo project.

Chay:
    python3 Tools/check_project.py                 # mac dinh: ./Star Vanguard
    python3 Tools/check_project.py --project "..."  # duong dan khac
    python3 Tools/check_project.py --json report.json

Exit code:
    0 = khong co loi muc FAIL
    1 = co it nhat 1 FAIL (dung de lam CI gate / MCP agent gate)

Chi dung thu vien chuan Python 3.8+.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from collections import defaultdict

# ----------------------------------------------------------------------------
# Ket qua
# ----------------------------------------------------------------------------

FAIL = "FAIL"
WARN = "WARN"
INFO = "INFO"

results: list[dict] = []


def add(level: str, check: str, message: str, detail: list[str] | None = None) -> None:
    results.append(
        {
            "level": level,
            "check": check,
            "message": message,
            "detail": detail or [],
        }
    )
    if level == FAIL:
        print(f"[FAIL] {check}: {message}")
        for line in (detail or [])[:12]:
            print(f"       - {line}")
    elif level == WARN:
        print(f"[WARN] {check}: {message}")
        for line in (detail or [])[:8]:
            print(f"       - {line}")
    else:
        print(f"[ OK ] {check}: {message}")


# ----------------------------------------------------------------------------
# Helpers
# ----------------------------------------------------------------------------

GUID_RE = re.compile(r"^guid: ([0-9a-f]{32})", re.M)
ANY_GUID_RE = re.compile(r"guid: ([0-9a-f]{32})")
SCRIPT_REF_RE = re.compile(r"m_Script: \{fileID: -?\d+, guid: ([0-9a-f]{32}), type: 3\}")
CLASS_RE = re.compile(
    r"(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+|partial\s+)*"
    r"(class|struct|enum|interface)\s+(\w+)"
)
MONO_CLASS_RE = re.compile(
    r"(?:public|internal)\s+(?:sealed\s+|abstract\s+|partial\s+|static\s+)*class\s+(\w+)\s*:\s*([\w<>, \.]*)"
)
YAML_ASSET_EXT = (".unity", ".prefab", ".asset", ".controller", ".mat", ".anim")
# Chi scene/prefab moi co "Missing Script" theo nghia component gan tren GameObject.
# File .asset (VolumeProfile, Settings...) tham chieu type cua package -> khong phai loi.
GAMEOBJECT_ASSET_EXT = (".unity", ".prefab")

# GUID cua cac script nam trong package Unity (ugui / TextMeshPro / URP).
# Chung khong co .meta trong Assets/ nhung van resolve binh thuong qua PackageCache.
KNOWN_PACKAGE_GUIDS = {
    "f4688fdb7df04437aeb418b961361dc5",  # TextMeshProUGUI
    "fe87c0e1cc204ed48ad3b37840f39efc",  # UnityEngine.UI.Image
    "4e29b1a8efbd4b44bb3f3716e73f07ff",  # UnityEngine.UI.Button
    "306cc8c2b49d7114eaa3623786fc2126",  # TMP_InputField
    "59f8146938fff824cb5fd77236b75775",  # VerticalLayoutGroup
    "9085046f02f69544eb97fd06b6048fe2",  # UnityEngine.UI.Toggle
    "67db9e8f0e2ae9c40bc1e2b64352a6b4",  # UnityEngine.UI.Slider
    "5f7201a12d95ffc409449d95f23cf332",  # UnityEngine.UI.Text
    "0cd44c1031e13a943bb63640046fad76",  # CanvasScaler
    "a79441f348de89743a2939f4d699eac1",  # AudioListener / scene camera rig
    "073797afb82c5a1438f328866b10b3f0",  # URP camera data
    "2da0d512f12947e489f739169773d7ca",  # Light2D
    "3312d7739989d2b4e91e6319e9a96d76",  # URP renderer helper
    "dc42784cf147c0c48a680349fa168899",  # UnityEngine.UI.GraphicRaycaster
    "01614664b831546d2ae94a42149d80ac",  # InputSystemUIInputModule (com.unity.inputsystem)
    "76c392e42b5098c458856cdf6ecaaaa1",  # UnityEngine.EventSystems.EventSystem
    "30649d3a9faa99c48a7b1166b86bf2a0",  # HorizontalLayoutGroup
    "2da0c512f12947e489f739169773d7ca",  # TMPro.TMP_InputField
    # Sprite/asset dung san cua Unity (builtin extra / UI default):
    "a97c105638bdf8b4a8650670310a4cd3",
    "ca9f5fa95ffab41fb9a615ab714db018",
}


def read(path: str) -> str:
    with open(path, encoding="utf-8", errors="ignore") as handle:
        return handle.read()


def rel(project: str, path: str) -> str:
    return os.path.relpath(path, project).replace(os.sep, "/")


def iter_files(root: str, suffixes: tuple[str, ...]):
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if not d.startswith(".")]
        for name in filenames:
            if name.endswith(suffixes):
                yield os.path.join(dirpath, name)


# ----------------------------------------------------------------------------
# 1. Tinh toan ban do GUID <-> asset
# ----------------------------------------------------------------------------


def build_guid_map(project: str) -> tuple[dict[str, str], dict[str, list[str]]]:
    assets = os.path.join(project, "Assets")
    guid_to_asset: dict[str, str] = {}
    asset_to_guids: dict[str, list[str]] = defaultdict(list)

    for meta in iter_files(assets, (".meta",)):
        text = read(meta)
        match = GUID_RE.search(text)
        if not match:
            continue
        target = meta[: -len(".meta")]
        guid_to_asset[match.group(1)] = target
        asset_to_guids[target].append(match.group(1))
    return guid_to_asset, asset_to_guids


# ----------------------------------------------------------------------------
# 2. Cac check
# ----------------------------------------------------------------------------


def check_meta_integrity(project: str, guid_to_asset: dict[str, str]) -> None:
    assets = os.path.join(project, "Assets")

    missing_meta: list[str] = []
    orphan_meta: list[str] = []
    for dirpath, dirnames, filenames in os.walk(assets):
        dirnames[:] = [d for d in dirnames if not d.startswith(".")]
        for name in filenames:
            if name.endswith(".meta"):
                continue
            path = os.path.join(dirpath, name)
            if not os.path.exists(path + ".meta"):
                missing_meta.append(rel(project, path))
        for name in list(dirnames) + [f for f in filenames if f.endswith(".meta")]:
            if not name.endswith(".meta"):
                continue
            path = os.path.join(dirpath, name)
            if not os.path.exists(path[: -len(".meta")]):
                orphan_meta.append(rel(project, path))

    if missing_meta:
        add(FAIL, "meta_missing", f"{len(missing_meta)} asset thieu file .meta", missing_meta)
    else:
        add(INFO, "meta_missing", "Moi asset deu co .meta")

    if orphan_meta:
        add(FAIL, "meta_orphan", f"{len(orphan_meta)} file .meta mo coi (asset da bi xoa)", orphan_meta)
    else:
        add(INFO, "meta_orphan", "Khong co .meta mo coi")

    seen: dict[str, list[str]] = defaultdict(list)
    for asset, guids in {k: v for k, v in _asset_to_guids_cache.items()}.items():
        for guid in guids:
            seen[guid].append(rel(project, asset))
    duplicates = {guid: paths for guid, paths in seen.items() if len(paths) > 1}
    if duplicates:
        add(
            FAIL,
            "guid_duplicate",
            f"{len(duplicates)} GUID bi trung (Unity se doi fileID / mat reference)",
            [f"{guid} -> {paths}" for guid, paths in duplicates.items()],
        )
    else:
        add(INFO, "guid_duplicate", "Khong co GUID trung lap")


_asset_to_guids_cache: dict[str, list[str]] = {}


def check_references(project: str, guid_to_asset: dict[str, str]) -> dict[str, set[str]]:
    assets = os.path.join(project, "Assets")
    unresolved: dict[str, set[str]] = defaultdict(set)
    broken_scripts: dict[str, set[str]] = defaultdict(set)

    for path in iter_files(assets, YAML_ASSET_EXT):
        text = read(path)
        source = rel(project, path)
        is_gameobject_asset = path.endswith(GAMEOBJECT_ASSET_EXT)
        for guid in set(ANY_GUID_RE.findall(text)):
            if guid.startswith("0000000000000000"):
                continue
            if guid in guid_to_asset or guid in KNOWN_PACKAGE_GUIDS:
                continue
            if is_gameobject_asset:
                unresolved[guid].add(source)
        if not is_gameobject_asset:
            continue
        for match in SCRIPT_REF_RE.finditer(text):
            guid = match.group(1)
            if guid in guid_to_asset or guid in KNOWN_PACKAGE_GUIDS:
                continue
            broken_scripts[guid].add(source)

    if broken_scripts:
        add(
            FAIL,
            "missing_script",
            f"{len(broken_scripts)} GUID script khong ton tai (component se hien 'Missing Script')",
            [f"{guid} -> {sorted(paths)}" for guid, paths in broken_scripts.items()],
        )
    else:
        add(INFO, "missing_script", "Khong co component bi Missing Script")

    if unresolved:
        add(
            WARN,
            "unresolved_guid",
            f"{len(unresolved)} GUID khong tim thay trong Assets (co the la asset cua package)",
            [f"{guid} -> {sorted(paths)[:2]}" for guid, paths in list(unresolved.items())[:15]],
        )
    return unresolved


def check_class_file_names(project: str) -> None:
    assets = os.path.join(project, "Assets")
    mismatches: list[str] = []
    type_owner: dict[str, set[str]] = defaultdict(set)
    mono_count = 0

    for path in iter_files(assets, (".cs",)):
        text = read(path)
        stem = os.path.basename(path)[:-3]
        for match in MONO_CLASS_RE.finditer(text):
            name, bases = match.group(1), match.group(2)
            if re.search(r"MonoBehaviour|Singleton<|NetworkBehaviour|ScriptableObject", bases):
                mono_count += 1
                if name != stem:
                    mismatches.append(f"{rel(project, path)} -> class {name} (Unity khong add duoc component)")
        for match in CLASS_RE.finditer(text):
            type_owner[match.group(2)].add(rel(project, path))

    if mismatches:
        add(FAIL, "class_file_name", f"{len(mismatches)} class Mono/ScriptableObject khong trung ten file", mismatches)
    else:
        add(INFO, "class_file_name", f"{mono_count} class MonoBehaviour/ScriptableObject deu trung ten file")

    duplicates = {name: sorted(paths) for name, paths in type_owner.items() if len(paths) > 1}
    public_dupes = {k: v for k, v in duplicates.items() if k not in {"SubMesh"}}
    if public_dupes:
        add(WARN, "duplicate_type", f"{len(public_dupes)} ten type bi khai bao o nhieu file", [
            f"{name} -> {paths}" for name, paths in public_dupes.items()
        ])
    else:
        add(INFO, "duplicate_type", "Khong co type trung ten")


def check_networking(project: str, guid_to_asset: dict[str, str]) -> None:
    assets = os.path.join(project, "Assets")
    manifest_path = os.path.join(project, "Packages", "manifest.json")
    manifest = read(manifest_path) if os.path.exists(manifest_path) else ""

    mirror_package = "mirror" in manifest.lower() or "com.coplaydev" in manifest.lower()
    define_locations: list[str] = []
    for path in iter_files(assets, (".asmdef", ".rsp")):
        if "MIRROR" in read(path):
            define_locations.append(rel(project, path))

    guarded_files: list[str] = []
    for path in iter_files(assets, (".cs",)):
        text = read(path)
        if re.search(r"#if\s+MIRROR\b", text):
            guarded_files.append(rel(project, path))

    if guarded_files and not mirror_package:
        add(
            FAIL,
            "mirror_package",
            "Code mang bi loai khoi build: khong co goi Mirror trong Packages/manifest.json",
            guarded_files,
        )
    elif guarded_files and not define_locations:
        add(
            WARN,
            "mirror_define",
            "Co #if MIRROR nhung khong co asmdef/versionDefine nao dinh nghia MIRROR "
            "(Mirror khong tu define symbol nay -> code mang van khong duoc bien dich)",
            guarded_files,
        )

    # Cac script Networking co duoc gan vao scene/prefab nao khong?
    yaml_text = "\n".join(read(p) for p in iter_files(assets, YAML_ASSET_EXT))
    unused: list[str] = []
    for path in iter_files(assets, (".cs",)):
        if os.sep + "Networking" + os.sep not in path:
            continue
        meta = path + ".meta"
        if not os.path.exists(meta):
            continue
        match = GUID_RE.search(read(meta))
        if match and match.group(1) not in yaml_text:
            unused.append(rel(project, path))
    if unused:
        add(
            WARN,
            "net_unused",
            f"{len(unused)} script mang khong duoc gan vao scene/prefab nao (dead code)",
            unused,
        )


def check_asmdef_and_tests(project: str) -> None:
    assets = os.path.join(project, "Assets")
    asmdefs = [rel(project, p) for p in iter_files(assets, (".asmdef",))]
    if not asmdefs:
        add(
            WARN,
            "asmdef",
            "Khong co Assembly Definition nao: moi thay doi bien dich lai toan bo, "
            "Editor tools lan vao build assembly, khong test duoc doc lap",
        )
    else:
        add(INFO, "asmdef", f"{len(asmdefs)} asmdef")

    has_tests = any(
        "Test" in os.path.basename(p) or "/Tests/" in p.replace(os.sep, "/")
        for p in iter_files(assets, (".cs",))
        if "NUnit" in read(p) or "[Test]" in read(p)
    )
    if not has_tests:
        add(
            WARN,
            "unit_tests",
            "com.unity.test-framework co trong manifest nhung khong co test NUnit "
            "([Test]/[UnityTest]) -> khong co regression gate that su",
        )
    else:
        add(INFO, "unit_tests", "Co test NUnit")


def check_build_settings(project: str) -> None:
    path = os.path.join(project, "ProjectSettings", "EditorBuildSettings.asset")
    if not os.path.exists(path):
        add(FAIL, "build_scenes", "Thieu ProjectSettings/EditorBuildSettings.asset")
        return
    text = read(path)
    scenes = re.findall(r"path: (Assets/[^\n]+\.unity)", text)
    if not scenes:
        add(FAIL, "build_scenes", "Khong co scene nao trong Build Settings")
        return
    missing = [s for s in scenes if not os.path.exists(os.path.join(project, s))]
    if missing:
        add(FAIL, "build_scenes", "Scene trong Build Settings khong ton tai", missing)
    if "Bootstrap" not in os.path.basename(scenes[0]):
        add(FAIL, "build_scenes", f"Scene index 0 phai la BootstrapScene, dang la {scenes[0]}")
    else:
        add(INFO, "build_scenes", f"Build order dung: {' -> '.join(os.path.basename(s) for s in scenes)}")

    # Template scene cua URP nam trong Assets
    template = os.path.join(project, "Assets", "Settings", "Scenes", "URP2DSceneTemplate.unity")
    if os.path.exists(template):
        add(WARN, "template_scene", "Con sot scene template URP trong Assets/Settings/Scenes (nen xoa)")


def check_tags_layers(project: str) -> None:
    tag_path = os.path.join(project, "ProjectSettings", "TagManager.asset")
    if not os.path.exists(tag_path):
        return
    text = read(tag_path)
    tag_block = text.split("layers:")[0]
    tags = set(re.findall(r"^\s*-\s*(\w+)\s*$", tag_block, re.M))
    layer_block = text.split("layers:")[1].split("m_SortingLayers:")[0] if "layers:" in text else ""
    layers = {m for m in re.findall(r"^\s*-\s*(\w+)\s*$", layer_block, re.M)}

    used_tags: dict[str, set[str]] = defaultdict(set)
    used_layers: dict[str, set[str]] = defaultdict(set)
    for path in iter_files(os.path.join(project, "Assets"), (".cs",)):
        text = read(path)
        source = rel(project, path)
        for tag in re.findall(r'CompareTag\("([^"]+)"\)', text):
            used_tags[tag].add(source)
        for layer in re.findall(r'NameToLayer\("([^"]+)"\)', text):
            used_layers[layer].add(source)

    builtin_tags = {"Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController"}
    missing_tags = {t: s for t, s in used_tags.items() if t not in tags and t not in builtin_tags}
    missing_layers = {l: s for l, s in used_layers.items() if l not in layers}
    if missing_tags:
        add(FAIL, "tags", "CompareTag voi tag khong ton tai (Unity nem loi luc runtime)", [
            f"{t} -> {sorted(s)}" for t, s in missing_tags.items()
        ])
    else:
        add(INFO, "tags", f"Tag dung: {sorted(used_tags)}")
    if missing_layers:
        add(FAIL, "layers", "LayerMask.NameToLayer tra -1 (so sanh layer se luon sai)", [
            f"{l} -> {sorted(s)}" for l, s in missing_layers.items()
        ])
    else:
        add(INFO, "layers", f"Layer dung: {sorted(used_layers)}")


def check_input(project: str) -> None:
    settings = os.path.join(project, "ProjectSettings", "ProjectSettings.asset")
    if not os.path.exists(settings):
        return
    match = re.search(r"activeInputHandler: (\d)", read(settings))
    handler = {"0": "Input Manager (cu)", "1": "Input System (moi)", "2": "Both"}.get(
        match.group(1) if match else "", "khong ro"
    )

    legacy = 0
    new_input = 0
    for path in iter_files(os.path.join(project, "Assets"), (".cs",)):
        text = read(path)
        legacy += len(re.findall(r"\bInput\.(GetKey|GetKeyDown|GetKeyUp|GetAxis|GetAxisRaw|GetMouseButton)", text))
        new_input += len(re.findall(r"\bKeyboard\.current|\bGamepad\.current|InputAction", text))

    actions = os.path.join(project, "Assets", "InputSystem_Actions.inputactions")
    if legacy and new_input == 0:
        add(
            WARN,
            "input",
            f"activeInputHandler = {handler}; {legacy} cho goi Input legacy, 0 cho dung Input System"
            + ("; file .inputactions ton tai nhung khong duoc dung" if os.path.exists(actions) else ""),
        )
    else:
        add(INFO, "input", f"activeInputHandler = {handler}; legacy={legacy}, InputSystem={new_input}")


def check_hud_duplication(project: str) -> None:
    scripts = os.path.join(project, "Assets", "Scripts")
    ui_manager = os.path.join(scripts, "UIManager.cs")
    overheat = os.path.join(scripts, "OverheatGaugeUI.cs")
    if not (os.path.exists(ui_manager) and os.path.exists(overheat)):
        return
    ui_text = read(ui_manager)
    oh_text = read(overheat)
    auto_adds = re.findall(r"gameObject\.AddComponent<(\w+)>\(\)", ui_text)
    imgui_hud = "private void OnGUI()" in oh_text and "m_EnableProceduralHUD" in oh_text
    default_true = re.search(r"m_EnableProceduralHUD\s*=\s*true", oh_text) is not None
    if auto_adds and imgui_hud and default_true:
        add(
            WARN,
            "hud_duplication",
            "UIManager tu AddComponent " + ", ".join(sorted(set(auto_adds))) +
            " va OverheatGaugeUI ve HUD bang OnGUI voi m_EnableProceduralHUD = true "
            "-> co the ve trung HUD uGUI (score/lives/bombs) va cap phat GC moi frame",
        )


def check_hot_paths(project: str) -> None:
    assets = os.path.join(project, "Assets")
    hot: list[str] = []
    pattern = re.compile(
        r"\b(Update|FixedUpdate|LateUpdate|OnGUI)\s*\([^)]*\)\s*\{", re.S
    )
    for path in iter_files(assets, (".cs",)):
        text = read(path)
        for match in pattern.finditer(text):
            start = match.end() - 1
            depth = 0
            end = start
            while end < len(text):
                if text[end] == "{":
                    depth += 1
                elif text[end] == "}":
                    depth -= 1
                    if depth == 0:
                        break
                end += 1
            body = text[start:end]
            offenders = []
            if re.search(r"Find\w+<", body):
                offenders.append("Find*")
            if "Camera.main" in body:
                offenders.append("Camera.main")
            if re.search(r'new [A-Z]\w+\[', body):
                offenders.append("new[]")
            if re.search(r'\$"', body):
                offenders.append("string interpolation")
            if "GetComponent" in body:
                offenders.append("GetComponent")
            if offenders:
                hot.append(f"{rel(project, path)} :: {match.group(1)} -> {', '.join(offenders)}")
    if hot:
        add(WARN, "hot_path", f"{len(hot)} ham chay moi frame co thao tac ton kem", hot)
    else:
        add(INFO, "hot_path", "Khong thay Find/GC trong Update")


def check_camera_main_everywhere(project: str) -> None:
    count = 0
    files = set()
    for path in iter_files(os.path.join(project, "Assets", "Scripts"), (".cs",)):
        text = read(path)
        n = text.count("Camera.main")
        if n:
            count += n
            files.add(rel(project, path))
    if count:
        add(INFO, "camera_main", f"{count} lan goi Camera.main trong {len(files)} file (nen cache trong 1 service)")


def check_docs_drift(project: str) -> None:
    manifests = os.path.join(project, "Packages", "manifest.json")
    version_txt = os.path.join(project, "ProjectSettings", "ProjectVersion.txt")
    if not os.path.exists(version_txt):
        return
    real = re.search(r"m_EditorVersion: (\S+)", read(version_txt)).group(1)
    drift = []
    for name in os.listdir(project):
        if not name.endswith(".md"):
            continue
        text = read(os.path.join(project, name))
        for claimed in set(re.findall(r"6000\.\d+\.\d+f\d+", text)):
            if claimed != real:
                drift.append(f"{name}: ghi {claimed}, thuc te {real}")
    if drift:
        add(WARN, "docs_drift", "Tai lieu ghi sai phien ban Unity", drift)

    # IPoolable API drift
    poolable = os.path.join(project, "Assets", "Scripts", "IPoolable.cs")
    if os.path.exists(poolable):
        text = read(poolable)
        methods = re.findall(r"void (\w+)\(", text)
        docs = ""
        guard = os.path.join(project, "unity_6_ai_agent_rules_guardrails.md")
        if os.path.exists(guard):
            docs = read(guard)
        wrong = [m for m in ("OnSpawnFromPool", "OnReturnToPool") if m in docs and m not in methods]
        if wrong:
            add(
                WARN,
                "docs_drift",
                "Tai lieu guardrails mo ta sai API pooling: " + ", ".join(wrong) +
                f" (code thuc te: {', '.join(methods)})",
            )

    # Duplicate / messy docs
    names = [n for n in os.listdir(project) if n.endswith(".md")]
    dups = [n for n in names if re.search(r"\(\d+\)", n)]
    if dups:
        add(WARN, "docs_cleanup", "Tai lieu trung lap trong root", dups)

    if not os.path.exists(os.path.join(project, "README.md")):
        add(WARN, "docs_cleanup", "Khong co README.md o root (nguoi moi khong biet cach chay)")


def check_perf_settings(project: str) -> None:
    quality = os.path.join(project, "ProjectSettings", "QualitySettings.asset")
    if os.path.exists(quality):
        text = read(quality)
        vsync = re.findall(r"vSyncCount: (\d)", text)
        if vsync and "0" in vsync:
            add(
                WARN,
                "perf_settings",
                f"vSyncCount = {vsync} (co muc tat vsync) va khong thay Application.targetFrameRate trong code "
                "-> FPS khong gioi han, de nong may / frame time khong on dinh",
            )
    has_target = any(
        "targetFrameRate" in read(path)
        for path in iter_files(os.path.join(project, "Assets", "Scripts"), (".cs",))
    )
    if not has_target:
        add(INFO, "perf_settings", "Khong dat Application.targetFrameRate")


def check_scene_size(project: str) -> None:
    scenes = []
    for path in iter_files(os.path.join(project, "Assets", "Scenes"), (".unity",)):
        lines = sum(1 for _ in open(path, encoding="utf-8", errors="ignore"))
        scenes.append((rel(project, path), lines))
    big = [f"{name}: {lines} dong" for name, lines in scenes if lines > 3000]
    if big:
        add(WARN, "scene_size", "Scene qua lon (kho review diff, de conflict khi AI sua)", big)
    else:
        add(INFO, "scene_size", f"{len(scenes)} scene, kich thuoc binh thuong")


def check_dead_scripts(project: str, guid_to_asset: dict[str, str]) -> None:
    assets = os.path.join(project, "Assets")
    yaml_text = "\n".join(read(p) for p in iter_files(assets, YAML_ASSET_EXT))
    all_cs_text = {p: read(p) for p in iter_files(assets, (".cs",))}

    dead: list[str] = []
    for path, text in all_cs_text.items():
        if "Editor" + os.sep in path:
            continue
        meta = path + ".meta"
        if not os.path.exists(meta):
            continue
        match = GUID_RE.search(read(meta))
        if not match or match.group(1) in yaml_text:
            continue
        stem = os.path.basename(path)[:-3]
        referenced = any(
            re.search(rf"\b{re.escape(stem)}\b", other) for other_path, other in all_cs_text.items() if other_path != path
        )
        if not referenced:
            dead.append(rel(project, path))
    if dead:
        add(WARN, "dead_code", f"{len(dead)} script khong duoc dung o scene/prefab lan code", dead)
    else:
        add(INFO, "dead_code", "Khong co script chet ro rang")


def check_code_noise(project: str) -> None:
    logs = 0
    todos = 0
    for path in iter_files(os.path.join(project, "Assets", "Scripts"), (".cs",)):
        text = read(path)
        logs += len(re.findall(r"Debug\.Log(?!Warning|Error)", text))
        todos += len(re.findall(r"TODO|FIXME|HACK", text))
    add(INFO, "code_noise", f"{logs} Debug.Log, {todos} TODO/FIXME trong Assets/Scripts")


# ----------------------------------------------------------------------------
# main
# ----------------------------------------------------------------------------


def main() -> int:
    parser = argparse.ArgumentParser(description="Star Vanguard static audit")
    default_project = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Star Vanguard")
    parser.add_argument("--project", default=default_project, help="duong dan thu muc Unity project")
    parser.add_argument("--json", default="", help="ghi ket qua ra file JSON")
    parser.add_argument("--quiet", action="store_true", help="chi in FAIL")
    args = parser.parse_args()

    project = os.path.abspath(args.project)
    if not os.path.isdir(os.path.join(project, "Assets")):
        print(f"Khong tim thay Unity project tai: {project}", file=sys.stderr)
        return 2

    print(f"Star Vanguard audit -> {project}\n")

    global _asset_to_guids_cache
    guid_to_asset, _asset_to_guids_cache = build_guid_map(project)

    check_meta_integrity(project, guid_to_asset)
    check_references(project, guid_to_asset)
    check_class_file_names(project)
    check_networking(project, guid_to_asset)
    check_asmdef_and_tests(project)
    check_build_settings(project)
    check_tags_layers(project)
    check_input(project)
    check_hud_duplication(project)
    check_hot_paths(project)
    check_camera_main_everywhere(project)
    check_perf_settings(project)
    check_scene_size(project)
    check_dead_scripts(project, guid_to_asset)
    check_docs_drift(project)
    check_code_noise(project)

    fails = sum(1 for r in results if r["level"] == FAIL)
    warns = sum(1 for r in results if r["level"] == WARN)
    print(f"\nTong ket: {fails} FAIL, {warns} WARN, {len(results) - fails - warns} OK")

    if args.json:
        with open(args.json, "w", encoding="utf-8") as handle:
            json.dump(results, handle, ensure_ascii=False, indent=2)
        print(f"Da ghi bao cao JSON: {args.json}")

    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
