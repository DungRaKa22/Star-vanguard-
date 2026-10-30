Execute Sprint 0 (Emergency Rescue & Safety Net) for Star Vanguard with the team (@Architect, @Gameplay-Dev, @UI-Audio-Dev, @QA-Engineer), focusing on fixing critical blockers and establishing safety gates before proceeding to subsequent sprints.

Working directory: C:\Documents\UNITY 2026
Integrity mode: development

Reference materials:
- `c:\Users\aovga\Downloads\antigravity_teamwork_plan.md`
- `C:\Users\aovga\.gemini\antigravity\brain\a66111d3-3a5f-4223-b277-381b9dc439a2\plan_teamwork_core_completion.md`
- `c:\Documents\UNITY 2026\AGENTS.md`

## Requirements

### R1. Cleanup Legacy Co-op UI (T-204a)
Execute legacy Co-op cleanup on `MainMenuScene.unity` using `StarVanguardCoopCleanup.cs` (`Tools > Star Vanguard > 1b. Remove Legacy Co-op UI`) to remove dead Co-op UI objects (`CoopPanel`, `CoopCard`, `CoopButton`).

### R2. Fix Mouse Input on EventSystem (T-204b)
Fix unresponsive mouse input on EventSystems in `MainMenuScene.unity` and `GameplayScene.unity` by assigning the valid ActionsAsset `Assets/InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) to `InputSystemUIInputModule`. Obey the Single Scene Writer lock and do not edit scene YAML directly by hand.

### R3. Remove Duplicate Camera & Light from BootstrapScene (T-204c)
Remove `Main Camera` and `Global Light 2D` from `BootstrapScene.unity` to prevent duplicate camera and URP 2D lighting when loaded additively.

### R4. Establish One-Way Assembly Definitions (T-101)
Create 4 `.asmdef` files with strict one-way dependency (`StarVanguard.Runtime`, `StarVanguard.UI`, `StarVanguard.Editor`, `StarVanguard.Tests` where `Tests -> UI -> Runtime` and `Editor -> Runtime, UI`). Ensure `StarVanguard.Runtime` never references `StarVanguard.UI` directly; UI updates must use C# events.

### R5. Execution Scope and Reporting
Focus strictly on completing and verifying Sprint 0 (T-204a, T-204b, T-204c, T-101). Pass all verification gates and report the results before continuing to subsequent roadmap sprints.

## Acceptance Criteria

### Gate Verification (AGENTS.md)
- [ ] G1 (Compile): Unity compiles cleanly with 0 errors and no new warnings after running `refresh_unity`.
- [ ] G2 (Regression): StarVanguardValidator or test suite passes 100%.
- [ ] G3 (Static check): `python3 Tools/check_project.py` reports 0 FAIL and warnings do not increase.

### Functional Verification
- [ ] In Play Mode starting from `BootstrapScene`, menu buttons in `MainMenuScene` respond immediately to mouse clicks.
- [ ] Additive loading does not produce duplicate Main Camera or duplicate URP 2D Global Light.
- [ ] Legacy Co-op UI elements are removed from `MainMenuScene`.
- [ ] The 4 assembly definition files compile without circular dependency errors and enforce one-way dependency.
