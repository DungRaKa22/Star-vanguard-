# BRIEFING — 2026-10-30T07:46:00Z

## Mission
Analyze and provide the exact implementation strategy for T-204a: Co-op UI cleanup in MainMenuScene.unity.

## 🔒 My Identity
- Archetype: explorer
- Roles: investigation, synthesis
- Working directory: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1
- Original parent: e398e3de-abd2-415f-9284-f5920e4fe429
- Milestone: Milestone 1: Scene Rescue & Input Fixes (T-204a)

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- DO NOT EDIT SCENE YAML BY HAND (Strict compliance with AGENTS.md)
- Obey Single Scene Writer lock protocol
- Deliver structured 5-component handoff report

## Current Parent
- Conversation ID: e398e3de-abd2-415f-9284-f5920e4fe429
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `Star Vanguard/Assets/Editor/StarVanguardCoopCleanup.cs` (lines 1–131)
  - `Star Vanguard/Assets/Scenes/MainMenuScene.unity` (YAML structure, blocks 1283642853, 1723830002, 1951282683, and all 67 related blocks)
  - `Star Vanguard/Assets/Scripts/UI/MainMenuScreen.cs` (lines 89, 147, 169–226, 254–262, 386–392)
  - `Tools/check_project.py` (lines 567–586, check_legacy_coop)
  - Repository-wide search for `CoopPanel`, `CoopCard`, `CoopButton`
  - Solution compilation via `dotnet build`
- **Key findings**:
  1. `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` is completely implemented and compiles with 0 errors.
  2. In `MainMenuScene.unity`, 67 total YAML blocks belong to the legacy co-op subtrees (`CoopButton` under `LeftColumn`, `CoopPanel` and `CoopCard` under `Canvas`).
  3. External references to these 67 blocks are strictly limited to exactly two parent `m_Children` lists: `LeftColumn` (`648767580`) and `Canvas` (`134044670`). No scripts, buttons, UnityEvents, or other components reference them.
  4. Destroying `CoopButton` and `CoopPanel` leaves zero dangling references and allows `VerticalLayoutGroup` on `LeftColumn` to cleanly collapse the gap.
  5. Can be executed directly via Unity batch mode: `Unity.exe -projectPath "..." -batchmode -quit -executeMethod StarVanguard.EditorTools.StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave -logFile -`.
  6. Must be executed under the Single Scene Writer lock sequentially with T-204b (which also modifies `MainMenuScene.unity`).
- **Unexplored areas**: None for T-204a scope.

## Key Decisions Made
- Confirmed direct execution of `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave` in Unity batch mode is the safest, AGENTS.md-compliant path.
- Established execution order and Single Scene Writer lock recommendations for Milestone 1 worker.

## Artifact Index
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1\handoff.md` — Detailed implementation plan and findings for T-204a.
