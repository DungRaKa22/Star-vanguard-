# Dispatch — Explorer M1-2: EventSystem Input Actions Wiring (T-204b)

## Milestone 1: Scene Rescue & Input Fixes
Your task is to analyze and provide the exact implementation strategy for **T-204b**:
- Target: `MainMenuScene.unity` and `GameplayScene.unity`.
- Issue: `InputSystemUIInputModule` has dangling GUID reference (`ca9f5fa95ffab41fb9a615ab714db018`), causing unresponsive mouse clicks.
- Goal: Assign the valid ActionsAsset `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`).
- Constraints:
  - DO NOT EDIT SCENE YAML BY HAND.
  - Formulate an Editor script or automated method to load each scene via `EditorSceneManager`, find the `InputSystemUIInputModule`, assign the `InputActionAsset` loaded from `Assets/InputSystem_Actions.inputactions`, mark dirty, and save.
  - Verify that mouse input maps (Point, LeftClick, MiddleClick, RightClick, ScrollWheel) correctly bind.
  - Obey Single Scene Writer lock protocol.

## Inputs
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1\handoff.md`

## Output
Write your recommendations to `handoff.md` in your directory (`c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_2\handoff.md`).
Report back with `send_message`.


## 2026-10-30T07:41:06Z
You are Explorer M1-2 for Milestone 1 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_2
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_2\DISPATCH.md
Read the dispatch instructions, investigate the EventSystem Input Actions wiring plan (T-204b), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
