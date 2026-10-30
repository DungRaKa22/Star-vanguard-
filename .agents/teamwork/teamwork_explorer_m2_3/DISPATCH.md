# Dispatch — Explorer M2-3: Validator Alignment & Testing Setup (T-101c)

## Mission
Analyze and specify the changes to align validator tooling and testing infrastructure:
1. `Star Vanguard/Assets/Editor/StarVanguardValidator.cs`:
   - Fix `CheckBuildSettings()` line 129 assertion: update expected index 0 scene from `MainMenuScene.unity` to `BootstrapScene.unity` (matching `EditorBuildSettings.asset` and AGENTS.md).
   - Add `[MenuItem("Star Vanguard/Validate Project")]` to `ValidateAll()` to allow execution from Unity Editor menus and MCP.
2. `Star Vanguard/Assets/Tests/`:
   - Specify folder structure and directory creation for `Assets/Tests/`.
   - Prepare initial test fixture or smoke check validating asmdef assembly loading.
3. Verify static check `check_project.py` rule for `asmdef`: confirm what rule checks for and how the 4 asmdefs will resolve the existing `[WARN] asmdef`.

## References
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_2\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`

## Output
Write report to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_3\handoff.md` and report back with `send_message`.


## 2026-10-30T08:36:49Z
From: e398e3de-abd2-415f-9284-f5920e4fe429
Content:
You are Explorer M2-3 for Milestone 2 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_3
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_3\DISPATCH.md
Read the dispatch instructions, investigate validator alignment and testing setup (T-101c), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
