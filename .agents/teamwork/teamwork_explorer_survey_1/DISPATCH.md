# Dispatch — Survey Explorer 1: Scene Hierarchy & Cleanup Survey

## Mission
Survey the scene structures and editor cleanup tooling for Sprint 0:
1. `MainMenuScene.unity`:
   - Inspect `StarVanguardCoopCleanup.cs` (`Tools > Star Vanguard > 1b. Remove Legacy Co-op UI`). What exact objects does it look for/remove? Are there `CoopPanel`, `CoopCard`, `CoopButton` objects?
   - Inspect EventSystem and `InputSystemUIInputModule` in `MainMenuScene.unity`. What is its current ActionsAsset assignment?
2. `GameplayScene.unity`:
   - Inspect EventSystem and `InputSystemUIInputModule`. What is its current ActionsAsset assignment?
3. `BootstrapScene.unity`:
   - Inspect objects in `BootstrapScene.unity`. Are `Main Camera` and `Global Light 2D` present? What components are on them?
4. Review `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`).
5. Review `AGENTS.md` rules regarding scene manipulation (no manual YAML edits; must use MCP or Editor scripts).

## Output
Write your comprehensive findings to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1\handoff.md`.
Report back when finished.

## 2026-10-30T07:30:17Z
You are Survey Explorer 1 for Sprint 0 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1\DISPATCH.md
Read the dispatch instructions, investigate the scene files and editor tools according to your mission, write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
