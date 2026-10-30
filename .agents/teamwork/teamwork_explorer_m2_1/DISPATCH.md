# Dispatch — Explorer M2-1: Runtime-to-UI Decoupling Plan (T-101a)

## Mission
Analyze and specify the exact code changes to decouple `StarVanguard.Runtime` from `StarVanguard.UI`:
1. Target files:
   - `Star Vanguard/Assets/Scripts/UIManager.cs`
   - `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`
2. Requirements:
   - Eliminate `using StarVanguard.UI;` and all direct references to `UIPanelRouter` and `UIPanelId` from `UIManager.cs`.
   - Expose C# events on `UIManager` (`OnShowSettingsRequested`, `OnShowControlsRequested`, etc.).
   - Update `UIPanelRouter.cs` to subscribe to `GameManager.OnGameStateChanged` and `UIManager` navigation events in `OnEnable()` and unsubscribe in `OnDisable()`.
   - Verify that NO OTHER runtime script references `StarVanguard.UI`.
3. Provide exact before/after code blocks and line numbers.

## References
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_2\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`

## Output
Write report to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_1\handoff.md` and report back with `send_message`.

## 2026-10-30T08:36:28Z
You are Explorer M2-1 for Milestone 2 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_1
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_1\DISPATCH.md
Read the dispatch instructions, investigate the Runtime-to-UI decoupling plan (T-101a), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.

## 2026-10-30T08:47:06Z
**Context**: Milestone 2 Decoupling Plan Status Check
**Content**: Both proposed_UIManager.cs and proposed_UIPanelRouter.cs have been generated. Please report your final handoff and completion status.
**Action**: Conclude handoff.md and reply with completion report.
