# Dispatch — Worker M1: Scene Rescue & Input Fixes Implementation

## Mission
Implement all requirements of Milestone 1 (T-204a, T-204b, T-204c):
1. **T-204a (Co-op UI Cleanup)**:
   - Run `StarVanguardCoopCleanup.RemoveFromMainMenuSceneAndSave()` on `MainMenuScene.unity` to remove `CoopPanel`, `CoopCard`, and `CoopButton`.
2. **T-204b (EventSystem Input Actions Wiring)**:
   - Implement `Assets/Editor/StarVanguardInputWiring.cs` following the exact specification in `teamwork_explorer_m1_2/handoff.md`.
   - Wire `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) into `InputSystemUIInputModule` across both `MainMenuScene.unity` and `GameplayScene.unity`.
3. **T-204c (Bootstrap Scene Duplicate Removal)**:
   - Implement `Assets/Editor/StarVanguardBootstrapCleanup.cs` following the specification in `teamwork_explorer_m1_3/handoff.md`.
   - Remove `Main Camera` (and AudioListener) and `Global Light 2D` from `BootstrapScene.unity`.
   - Update `StarVanguardBootstrapSetup.cs` to include `Main Camera` and `Global Light 2D` in `k_NonBootstrapObjects`.

## Critical Constraints & AGENTS.md Rules
- **NEVER EDIT SCENE YAML BY HAND OR WITH REGEX.** Use Unity Editor scripts or batchmode execution to load, modify, and save scenes cleanly.
- Respect Single Scene Writer lock: load and save one scene at a time.
- Mandatory Integrity Warning:
> DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

## Verification Required
1. Run build verification (`dotnet build "Star Vanguard\Star Vanguard.sln"`).
2. Run static verification (`python3 Tools/check_project.py`) and verify `[WARN] legacy_coop` is eliminated with 0 FAIL.
3. Verify git diff / git status for the affected scenes and editor scripts.

## Reference Handoffs
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_1\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_2\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_3\handoff.md`

## Output
Write your full report to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m1\handoff.md` and notify the orchestrator when complete.


## 2026-10-30T07:51:04Z
[Message] timestamp=2026-10-30T07:51:04Z sender=e398e3de-abd2-415f-9284-f5920e4fe429 priority=MESSAGE_PRIORITY_HIGH content=You are Worker M1 for Milestone 1 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m1
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m1\DISPATCH.md
Read the dispatch instructions, follow the explorer handoffs, implement all Milestone 1 changes (T-204a, T-204b, T-204c), execute verification builds and static checks, write your handoff.md, and send a completion message when finished.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.
