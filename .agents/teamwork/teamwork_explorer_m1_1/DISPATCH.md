# Dispatch — Explorer M1-1: Co-op Cleanup Implementation Plan (T-204a)

## Milestone 1: Scene Rescue & Input Fixes
Your task is to analyze and provide the exact implementation strategy for **T-204a**:
- Target: `MainMenuScene.unity`
- Objects to remove: `CoopPanel`, `CoopCard`, `CoopButton`.
- Tooling: `StarVanguardCoopCleanup.cs` (`Tools > Star Vanguard > 1b. Remove Legacy Co-op UI from MainMenuScene and Save`).
- Constraints:
  - DO NOT EDIT SCENE YAML BY HAND.
  - Review how `StarVanguardCoopCleanup.cs` operates. Can it be executed directly, or does an editor script/batch execution need to run it?
  - Confirm whether any other references to these destroyed objects exist in `MainMenuScene` (e.g., button listeners, layout groups).
  - Ensure Single Scene Writer lock protocol is respected.

## Inputs
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1\handoff.md`

## Output
Write your recommendations to `handoff.md` in your directory (`c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1\handoff.md`).
Report back with `send_message`.


## 2026-10-30T07:41:00Z
sender=e398e3de-abd2-415f-9284-f5920e4fe429
You are Explorer M1-1 for Milestone 1 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1\DISPATCH.md
Read the dispatch instructions, investigate the Co-op UI cleanup implementation plan (T-204a), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
