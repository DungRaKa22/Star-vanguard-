# Dispatch — Worker M2: One-Way Assembly Definitions & Architecture Decoupling (T-101)

## Mission
Implement all requirements of Milestone 2 (T-101a, T-101b, T-101c):

1. **T-101a (Runtime-to-UI Decoupling)**:
   - Update `Star Vanguard/Assets/Scripts/UIManager.cs`: remove `using StarVanguard.UI;`, remove direct calls to `UIPanelRouter` and `UIPanelId`, expose C# navigation events.
   - Update `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`: subscribe to `GameManager.OnGameStateChanged` and `UIManager` navigation events in `OnEnable()`, unsubscribe in `OnDisable()`.
   - You may apply `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_1\t101a_decoupling.patch` or use the proposed files in `teamwork_explorer_m2_1/`.

2. **T-101b (4 Assembly Definitions Creation)**:
   - Create `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`
   - Create `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`
   - Create `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef` (include `Unity.InputSystem`!)
   - Create `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`
   - Strictly follow the exact JSON schemas and referenced assemblies specified in `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_2\handoff.md`.

3. **T-101c (Validator Alignment & Tests Setup)**:
   - Update `Star Vanguard/Assets/Editor/StarVanguardValidator.cs`: update `CheckBuildSettings()` to verify canonical order (index 0 = `BootstrapScene.unity`), update `CheckGameplayScene()` to load `BootstrapScene` before `GameplayScene` additively, add `[MenuItem("Star Vanguard/Validate Project")]` to `ValidateAll()`, and guard `EditorApplication.Exit` with `if (Application.isBatchMode)`.
   - Create `Star Vanguard/Assets/Tests/AssemblySmokeTests.cs` adhering to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_3\handoff.md`.

## Critical Constraints & AGENTS.md Rules
- Follow naming conventions (`m_PascalCase`, `k_Constant`, etc.).
- Never reference `StarVanguard.UI` from `StarVanguard.Runtime`.
- Mandatory Integrity Warning:
> DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

## Verification Required
1. Run build verification (`dotnet build "Star Vanguard\Star Vanguard.sln"`). Must pass with 0 errors.
2. Run static verification (`python Tools/check_project.py`). Must report 0 FAIL, `[WARN] asmdef` eliminated, total warnings <= 6.
3. Run PlayMode smoke test (`StarVanguardPlayModeSmoke.Run`) or validator to verify zero regressions.
4. Verify git status and diff.

## References
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_1\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_2\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_3\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`

## Output
Write report to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m2\handoff.md` and notify orchestrator when complete.


## 2026-10-30T08:48:53Z
You are Worker M2 for Milestone 2 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m2
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_worker_m2\DISPATCH.md
Read the dispatch instructions, follow the explorer handoffs, implement all Milestone 2 changes (T-101a decoupling, T-101b 4 asmdefs, T-101c validator & tests), run verification builds and static checks, write your handoff.md, and send a completion message when finished.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.
