import re
import os
import sys

def analyze_scene(scene_path):
    print(f"==================================================")
    print(f"Analyzing scene: {scene_path}")
    print(f"==================================================")
    
    with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # 1. Collect all defined fileIDs: --- !u!Tag &FileID
    defined_file_ids = set()
    doc_headers = re.findall(r'^---\s+!u!(\d+)\s+&(\d+)', content, re.MULTILINE)
    for tag, file_id in doc_headers:
        defined_file_ids.add(int(file_id))
    print(f"Total YAML objects defined: {len(defined_file_ids)}")

    # 2. Check for orphaned local fileIDs: {fileID: X} without guid, or guid: 00000000000000000000000000000000
    # Matches {fileID: 12345} or {fileID: 12345, type: 0} where guid is not present
    local_ref_pattern = re.compile(r'\{\s*fileID:\s*(-?\d+)(?:\s*,\s*type:\s*\d+)?\s*\}')
    
    # We also need to check references that include guid if guid is empty
    # Matches {fileID: 12345, guid: ..., type: ...}
    all_ref_pattern = re.compile(r'\{\s*fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-fA-F0-9]{32}))?(?:,\s*type:\s*\d+)?\s*\}')

    lines = content.splitlines()
    orphans = []
    dangling_guid_ca9f = []
    canonical_guid_65b7 = []

    for line_no, line in enumerate(lines, 1):
        if 'ca9f5fa95ffab41fb9a615ab714db018' in line:
            dangling_guid_ca9f.append((line_no, line.strip()))
        if '65b761383e570eb4390d8402a8c60d39' in line:
            canonical_guid_65b7.append((line_no, line.strip()))

        # Check local refs
        for m in local_ref_pattern.finditer(line):
            target_id = int(m.group(1))
            if target_id != 0 and target_id not in defined_file_ids:
                orphans.append((line_no, target_id, line.strip()))

    print(f"References to dangling GUID (ca9f...): {len(dangling_guid_ca9f)}")
    print(f"References to canonical GUID (65b7...): {len(canonical_guid_65b7)}")
    print(f"Orphaned local fileIDs found: {len(orphans)}")
    if orphans:
        print("SAMPLE ORPHANS (first 10):")
        for line_no, target_id, ltext in orphans[:10]:
            print(f"  Line {line_no}: target fileID {target_id} not found in scene: {ltext}")

    # 3. Check for specific GameObjects
    # Find all GameObject declarations
    # Tag 1 is GameObject
    go_pattern = re.compile(r'^---\s+!u!1\s+&(\d+)\nGameObject:\n(?:[^\n]*\n)*?\s+m_Name:\s*([^\n]+)', re.MULTILINE)
    game_objects = {}
    for m in go_pattern.finditer(content):
        g_id = int(m.group(1))
        g_name = m.group(2).strip().strip('"').strip("'")
        game_objects[g_id] = g_name

    print(f"Total GameObjects: {len(game_objects)}")
    for g_id, g_name in game_objects.items():
        if any(bad in g_name for bad in ["Coop", "Main Camera", "Global Light 2D"]):
            print(f"  Found notable object: [{g_name}] (fileID: {g_id})")

    # 4. Check UI Buttons and Navigation
    # Tag 114 is MonoBehaviour
    # Let's inspect Buttons: look for m_Script referencing UnityEngine.UI.Button
    # Button navigation and onClick
    button_blocks = []
    # split into documents
    raw_docs = content.split('--- !u!')
    for doc in raw_docs[1:]:
        header_line = doc.splitlines()[0]
        tag_match = re.match(r'(\d+)\s+&(\d+)', header_line)
        if not tag_match:
            continue
        tag = int(tag_match.group(1))
        fid = int(tag_match.group(2))
        if tag == 114: # MonoBehaviour
            if 'm_OnClick:' in doc and 'm_Navigation:' in doc:
                # It's a button or selectable
                # Extract GameObject reference
                go_ref = re.search(r'm_GameObject:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
                go_name = "Unknown"
                if go_ref:
                    go_id = int(go_ref.group(1))
                    go_name = game_objects.get(go_id, f"MissingGO_{go_id}")
                
                # Check navigation
                nav_match = re.search(r'm_Navigation:\s*\n\s+m_Mode:\s*(\d+)\n\s+m_WrapAround:\s*(\d+)\n\s+m_SelectOnUp:\s*\{\s*fileID:\s*(\d+)\s*\}\n\s+m_SelectOnDown:\s*\{\s*fileID:\s*(\d+)\s*\}\n\s+m_SelectOnLeft:\s*\{\s*fileID:\s*(\d+)\s*\}\n\s+m_SelectOnRight:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
                nav_info = "Nav parsed failed"
                if nav_match:
                    mode = int(nav_match.group(1))
                    up = int(nav_match.group(3))
                    down = int(nav_match.group(4))
                    left = int(nav_match.group(5))
                    right = int(nav_match.group(6))
                    nav_info = f"Mode={mode}, Up={up}, Down={down}, Left={left}, Right={right}"
                    for dir_name, target in [('Up', up), ('Down', down), ('Left', left), ('Right', right)]:
                        if target != 0 and target not in defined_file_ids:
                            print(f"  [ERROR] Button on '{go_name}' has invalid navigation {dir_name}={target} (target not defined!)")
                
                # Check target graphic
                tg_match = re.search(r'm_TargetGraphic:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
                tg_info = "None"
                if tg_match:
                    tg_id = int(tg_match.group(1))
                    tg_info = f"fileID {tg_id} (exists={tg_id in defined_file_ids or tg_id == 0})"
                    if tg_id != 0 and tg_id not in defined_file_ids:
                        print(f"  [ERROR] Button on '{go_name}' has invalid m_TargetGraphic {tg_id}")

                # Check onClick calls
                calls = re.findall(r'm_Target:\s*\{\s*fileID:\s*(\d+)(?:,\s*guid:\s*([a-fA-F0-9]{32}))?.*?m_MethodName:\s*([^\n]+)', doc, re.DOTALL)
                click_info = []
                for tgt_fid, tgt_guid, method in calls:
                    tgt_fid = int(tgt_fid)
                    method = method.strip()
                    valid = True
                    if not tgt_guid and tgt_fid != 0 and tgt_fid not in defined_file_ids:
                        valid = False
                        print(f"  [ERROR] Button on '{go_name}' onClick calls missing local fileID {tgt_fid}.{method}")
                    click_info.append(f"{tgt_fid}.{method} (valid={valid})")

                button_blocks.append({
                    'go_name': go_name,
                    'fid': fid,
                    'nav': nav_info,
                    'target_graphic': tg_info,
                    'calls': click_info
                })

    print(f"Selectable/Button components inspected: {len(button_blocks)}")
    for b in button_blocks:
        print(f"  Button on '{b['go_name']}': {b['nav']}, TargetGraphic: {b['target_graphic']}, OnClick: {b['calls']}")

    # 5. Check InputSystemUIInputModule
    input_modules = []
    for doc in raw_docs[1:]:
        if 'm_ActionsAsset:' in doc and 'm_PointAction:' in doc:
            go_ref = re.search(r'm_GameObject:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
            go_name = game_objects.get(int(go_ref.group(1)), "Unknown") if go_ref else "Unknown"
            actions_asset = re.search(r'm_ActionsAsset:\s*\{[^\}]+\}', doc)
            point_act = re.search(r'm_PointAction:\s*\{[^\}]+\}', doc)
            left_act = re.search(r'm_LeftClickAction:\s*\{[^\}]+\}', doc)
            input_modules.append({
                'go_name': go_name,
                'actions_asset': actions_asset.group(0) if actions_asset else "MISSING",
                'point': point_act.group(0) if point_act else "MISSING",
                'left_click': left_act.group(0) if left_act else "MISSING"
            })
    print(f"InputSystemUIInputModules found: {len(input_modules)}")
    for im in input_modules:
        print(f"  InputModule on '{im['go_name']}': asset={im['actions_asset']}")
        print(f"    Point: {im['point']}")
        print(f"    LeftClick: {im['left_click']}")

    return {
        'orphans': orphans,
        'dangling_ca9f': dangling_guid_ca9f,
        'canonical_65b7': canonical_guid_65b7,
        'game_objects': game_objects,
        'buttons': button_blocks,
        'input_modules': input_modules
    }

if __name__ == '__main__':
    scenes = [
        r"Star Vanguard/Assets/Scenes/BootstrapScene.unity",
        r"Star Vanguard/Assets/Scenes/MainMenuScene.unity",
        r"Star Vanguard/Assets/Scenes/GameplayScene.unity"
    ]
    for s in scenes:
        analyze_scene(s)
