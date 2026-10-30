import re

def inspect_eventsystem_and_canvas(scene_path):
    print(f"=== Inspecting {scene_path} ===")
    with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Find EventSystem GameObject and its components
    raw_docs = content.split('--- !u!')
    docs_by_id = {}
    for doc in raw_docs[1:]:
        lines = doc.splitlines()
        header = lines[0]
        m = re.match(r'(\d+)\s+&(\d+)', header)
        if m:
            tag, fid = int(m.group(1)), int(m.group(2))
            docs_by_id[fid] = (tag, doc)

    # Find GameObject named EventSystem
    for fid, (tag, doc) in docs_by_id.items():
        if tag == 1 and 'm_Name: EventSystem' in doc:
            print(f"Found EventSystem GameObject &{fid}")
            # print components
            comps = re.findall(r'component:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
            print(f"  EventSystem Components: {comps}")
            for cfid in comps:
                cfid = int(cfid)
                if cfid in docs_by_id:
                    ctag, cdoc = docs_by_id[cfid]
                    script_match = re.search(r'm_Script:\s*\{[^\}]+\}', cdoc)
                    script_info = script_match.group(0) if script_match else "built-in"
                    print(f"    Component &{cfid} (tag {ctag}): {script_info}")
                    if 'm_ActionsAsset:' in cdoc:
                        for line in cdoc.splitlines():
                            if any(k in line for k in ['m_ActionsAsset', 'Action', 'm_Point', 'm_LeftClick', 'm_RightClick', 'm_ScrollWheel']):
                                print(f"      {line.strip()}")

    # Find Canvas and GraphicRaycaster
    for fid, (tag, doc) in docs_by_id.items():
        if tag == 1 and 'm_Name: Canvas' in doc:
            print(f"\nFound Canvas GameObject &{fid}")
            comps = re.findall(r'component:\s*\{\s*fileID:\s*(\d+)\s*\}', doc)
            print(f"  Canvas Components: {comps}")
            for cfid in comps:
                cfid = int(cfid)
                if cfid in docs_by_id:
                    ctag, cdoc = docs_by_id[cfid]
                    script_match = re.search(r'm_Script:\s*\{[^\}]+\}', cdoc)
                    first_line = cdoc.splitlines()[1].strip() if len(cdoc.splitlines()) > 1 else ""
                    print(f"    Component &{cfid} (tag {ctag}): {first_line} {script_match.group(0) if script_match else ''}")

    # Check for GraphicRaycaster on any object in scene
    gr_found = []
    for fid, (tag, doc) in docs_by_id.items():
        if tag == 114 and ('GraphicRaycaster' in doc or 'guid: 163795da3629e4b449b497b78a9c3b88' in doc or 'guid: 44b1c2a1' in doc or 'm_BlockingObjects:' in doc):
            gr_found.append(fid)
    print(f"\nGraphicRaycasters found in scene: {gr_found}")

inspect_eventsystem_and_canvas("Star Vanguard/Assets/Scenes/MainMenuScene.unity")
inspect_eventsystem_and_canvas("Star Vanguard/Assets/Scenes/GameplayScene.unity")
