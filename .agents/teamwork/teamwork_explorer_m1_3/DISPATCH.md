# Dispatch — Explorer M1-3: Bootstrap Scene Cleanup Plan (T-204c)

## Milestone 1: Scene Rescue & Input Fixes
Your task is to analyze and provide the exact implementation strategy for **T-204c**:
- Target: `BootstrapScene.unity`.
- Issue: Duplicate `Main Camera` (with AudioListener) and `Global Light 2D` cause duplicate camera errors, audio listener conflicts, and blown-out 2D lighting when loaded additively.
- Goal: Remove `Main Camera` and `Global Light 2D` from `BootstrapScene.unity`.
- Constraints:
  - DO NOT EDIT SCENE YAML BY HAND.
  - Review how `BootstrapScene.unity` is loaded and how `StarVanguardBootstrapSetup.cs` interacts with it.
  - Formulate an Editor script or automated method to open `BootstrapScene.unity` via `EditorSceneManager`, find and destroy `Main Camera` and `Global Light 2D`, mark dirty, and save.
  - Ensure persistent managers (`GameManager`, `AudioManager`, `WaveDirector`, etc.) remain intact and unperturbed.

## Inputs
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_1\handoff.md`

## Output
Write your recommendations to `handoff.md` in your directory (`c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_3\handoff.md`).
Report back with `send_message`.

## 2026-10-30T07:41:11Z
You are Explorer M1-3 for Milestone 1 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_3
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_3\DISPATCH.md
Read the dispatch instructions, investigate the Bootstrap scene duplicate removal plan (T-204c), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
