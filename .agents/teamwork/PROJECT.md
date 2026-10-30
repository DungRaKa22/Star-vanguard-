# Project: Star Vanguard — Sprint 0 (Emergency Rescue & Safety Net)

## Architecture
Star Vanguard is a Unity 6000.5.7f1 2D space shooter built with URP 2D, uGUI, and Unity Input System.
Architecture follows persistent Bootstrap Scene management (`BootstrapScene` at build index 0) loading `MainMenuScene` and `GameplayScene` additively or single-mode, with strict one-way assembly boundaries:
- `StarVanguard.Runtime` (Core gameplay, state, audio, managers, pool)
- `StarVanguard.UI` (Panels, HUD, router) -> depends on `StarVanguard.Runtime`
- `StarVanguard.Editor` (Editor tooling, validators, cleanup) -> depends on `Runtime`, `UI`
- `StarVanguard.Tests` (PlayMode & EditMode NUnit tests) -> depends on `Runtime`, `UI`

Runtime must never reference UI directly; communication flows down via direct calls and up via C# events.

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | T-204a: Legacy Co-op UI Cleanup | Remove `CoopPanel`, `CoopCard`, `CoopButton` from `MainMenuScene.unity` using `StarVanguardCoopCleanup.cs` | M1 | Survey 1, ORIGINAL_REQUEST §R1 |
| 2 | T-204b: EventSystem Mouse Input Fix | Assign valid `InputSystem_Actions.inputactions` (GUID `65b761383e570eb4390d8402a8c60d39`) to `InputSystemUIInputModule` in `MainMenuScene` & `GameplayScene` | M1 | Survey 1, ORIGINAL_REQUEST §R2 |
| 3 | T-204c: Bootstrap Scene Duplicate Removal | Remove duplicate `Main Camera` (and AudioListener) and `Global Light 2D` from `BootstrapScene.unity` | M1 | Survey 1, ORIGINAL_REQUEST §R3 |
| 4 | T-101a: Runtime-to-UI Decoupling | Decouple `UIManager.cs` from `StarVanguard.UI` (`UIPanelRouter`/`UIPanelId`) via C# events | M2 | Survey 2, ORIGINAL_REQUEST §R4 |
| 5 | T-101b: 4 Assembly Definitions Creation | Create `StarVanguard.Runtime.asmdef`, `StarVanguard.UI.asmdef`, `StarVanguard.Editor.asmdef`, `StarVanguard.Tests.asmdef` with strict one-way references | M2 | Survey 2, ORIGINAL_REQUEST §R4 |
| 6 | T-101c: Validator Alignment | Fix `StarVanguardValidator.cs` index 0 assertion (BootstrapScene) and add `[MenuItem]` attribute for MCP/Editor execution | M2 | Survey 2, Survey 3 |
| 7 | Safety Net G1 Verification | Compile verification via dotnet build and Unity compilation log (0 errors, no new warnings) | M3 | Survey 3, ORIGINAL_REQUEST §G1 |
| 8 | Safety Net G2 Verification | Regression verification via `StarVanguardValidator.ValidateAll` (100% pass) and playmode smoke test | M3 | Survey 3, ORIGINAL_REQUEST §G2 |
| 9 | Safety Net G3 Verification | Static verification via `python3 Tools/check_project.py` (0 FAIL, warnings <= 8) | M3 | Survey 3, ORIGINAL_REQUEST §G3 |
| 10 | Functional Verification | Verify mouse responsiveness in MainMenuScene, no duplicate camera/light, no co-op UI, clean asmdef compilation | M3 | ORIGINAL_REQUEST §Functional |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Scene Rescue & Input Fixes | T-204a (Co-op UI cleanup), T-204b (EventSystem ActionsAsset wiring), T-204c (Bootstrap duplicates removal) | none | DONE |
| 2 | One-Way Asmdef & Architecture Decoupling | T-101a (UIManager C# event decoupling), T-101b (4 asmdefs), T-101c (StarVanguardValidator alignment) | M1 | DONE |
| 3 | Verification Gates & Safety Net | Gates G1, G2, G3 verification, PlayMode smoke test, functional acceptance | M1, M2 | DONE |

## Interface Contracts
### `UIManager` (Runtime) ↔ `UIPanelRouter` (UI)
- `UIManager` must NOT reference `StarVanguard.UI`.
- `UIManager` exposes C# navigation events:
  - `public static event Action OnShowSettingsRequested;`
  - `public static event Action OnShowControlsRequested;`
  - `public static event Action OnShowAudioSettingsRequested;`
  - `public static event Action OnHideSubPanelsRequested;`
- `UIPanelRouter` in `StarVanguard.UI` subscribes to `GameManager.OnGameStateChanged` for primary screen transitions (`Menu`, `Playing`, `Paused`, `GameOver`).
- `UIPanelRouter` subscribes to `UIManager`'s navigation events in `OnEnable()` and unsubscribes in `OnDisable()`.

## Code Layout
- `Star Vanguard/Assets/Scripts/`: `StarVanguard.Runtime.asmdef` and runtime gameplay scripts.
- `Star Vanguard/Assets/Scripts/UI/`: `StarVanguard.UI.asmdef` and UI panel scripts (`StarVanguard.UI` namespace).
- `Star Vanguard/Assets/Editor/`: `StarVanguard.Editor.asmdef` and editor scripts (`StarVanguard.Editor`, `StarVanguard.EditorTools`).
- `Star Vanguard/Assets/Tests/`: `StarVanguard.Tests.asmdef` and automated NUnit tests.
- `Star Vanguard/Assets/Scenes/`: `BootstrapScene.unity`, `MainMenuScene.unity`, `GameplayScene.unity`.
- `Star Vanguard/Assets/InputSystem_Actions.inputactions`: Canonical input actions asset (GUID `65b761383e570eb4390d8402a8c60d39`).
