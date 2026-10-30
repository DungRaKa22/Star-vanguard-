# Handoff Report — Codebase Architecture & Assembly Definitions (T-101)

**Summary:** 
A full survey of 65 C# files in `Star Vanguard` reveals a clean separation between Editor (10 files), UI (6 files), and Runtime (49 files), with exactly one cross-boundary dependency violating the one-way rule: `UIManager.cs` directly references `StarVanguard.UI` (`UIPanelRouter`, `UIPanelId`). Decoupling `UIManager.cs` via C# events and creating 4 `.asmdef` files (`StarVanguard.Runtime`, `StarVanguard.UI`, `StarVanguard.Editor`, `StarVanguard.Tests`) enforces the required directional hierarchy (`Tests -> UI -> Runtime`, `Editor -> Runtime, UI`) without breaking any scenes or existing mechanics.

---

## 1. Observation

### 1.1 Script Inventory and Namespaces
Direct filesystem query (`Get-ChildItem -Path "Star Vanguard\Assets" -Filter "*.cs" -Recurse`) and static analysis yielded 65 `.cs` files across 3 directories:

| Directory | File Count | Declared Namespaces | Notes |
|:---|:---:|:---|:---|
| `Star Vanguard/Assets/Scripts/` | 49 | `(global)` (48 files)<br>`StarVanguard` (1 file: `BootstrapLoader.cs`) | Gameplay, persistence, audio, mechanics, pooling, and legacy/HUD UI. |
| `Star Vanguard/Assets/Scripts/UI/` | 6 | `StarVanguard.UI` (6 files) | Modern UI system: `ControlsScreen.cs`, `InputMap.cs`, `MainMenuScreen.cs`, `SettingsScreen.cs`, `UIPanelRouter.cs`, `UITheme.cs`. |
| `Star Vanguard/Assets/Editor/` | 10 | `StarVanguard.Editor` (4 files)<br>`StarVanguard.EditorTools` (6 files) | Editor utilities, smoke tests, scene splitter, and validator. |
| `Star Vanguard/Assets/Tests/` | 0 | None (directory does not exist yet) | Required for NUnit tests (T-102). |

#### Complete File Catalog by Folder:
1. **`Assets/Scripts/` (49 Runtime scripts):**
   - Persistence & Bootstrap: `BootstrapLoader.cs` (namespace `StarVanguard`), `Singleton.cs`, `SingletonRegistry.cs`, `GameManager.cs`, `SettingsManager.cs`, `LeaderboardManager.cs`.
   - Audio: `AudioManager.cs`, `eMusic.cs`, `eSFXId.cs`.
   - Player & Combat: `PlayerController.cs`, `PlayerShooting.cs`, `PlayerBombing.cs`, `PlayerRespawn.cs`, `PlayerHitBox.cs`, `PlayerStats.cs`, `WeaponHeatController.cs`, `Bullet.cs`, `Bomb.cs`, `IDamageable.cs`, `eWeaponType.cs`.
   - Enemies & Waves: `EnemyController.cs`, `EnemyMover.cs`, `EnemyShootingSimple.cs`, `EnemyBullet.cs`, `BossController.cs`, `BossMover.cs`, `BossShooting.cs`, `WaveDirector.cs`, `eEnemyMoveType.cs`.
   - Pickups & Economy: `PickupManager.cs`, `PowerUpPickup.cs`, `EnergyCrystal.cs`.
   - Pooling & Infrastructure: `PoolManager.cs`, `ObjectPool.cs`, `IPoolable.cs`.
   - Environment: `ParallaxManager.cs`, `ParallaxLayer.cs`, `FixedAspectCamera.cs`.
   - UI & HUD (Runtime-bound): `UIManager.cs`, `HUDIcon.cs`, `OverheatGaugeUI.cs`, `LeaderboardUI.cs`, `SettingsUI.cs`, `GameOverController.cs`, `PauseController.cs`, `InterWaveMessageManager.cs`, `CanvasAspectFitter.cs`, `ControlsPanelController.cs`, `eGameState.cs`.

2. **`Assets/Scripts/UI/` (6 UI scripts, namespace `StarVanguard.UI`):**
   - `ControlsScreen.cs`: Modern keyboard & mouse binding display.
   - `InputMap.cs`: Data table defining key mappings and action descriptions.
   - `MainMenuScreen.cs`: Main menu controller, difficulty selection (Cadet, Veteran, Ace), button routing.
   - `SettingsScreen.cs`: Audio sliders and game toggles, binds to `SettingsUI`.
   - `UIPanelRouter.cs`: Stack-based panel navigator (`UIPanelId`, `Show`, `Push`, `Back`, `HideAll`), screen fade transitions.
   - `UITheme.cs`: UI styling, fonts, color palettes, procedural sliced sprite generation.

3. **`Assets/Editor/` (10 Editor scripts):**
   - `StarVanguard.Editor`: `MainMenuScreenEditor.cs`, `StarVanguardMenuBaker.cs`, `UIPanelPreviewToolbar.cs`, `UIPanelRouterEditor.cs`.
   - `StarVanguard.EditorTools`: `StarVanguardBootstrapSetup.cs`, `StarVanguardCoopCleanup.cs`, `StarVanguardPlayModeSmoke.cs`, `StarVanguardSceneSplitter.cs`, `StarVanguardUIAudit.cs`, `StarVanguardValidator.cs`.

---

### 1.2 Runtime -> UI Direct Coupling in `UIManager.cs`
Analysis across all 49 runtime scripts revealed exactly **one** file referencing `StarVanguard.UI`: `Star Vanguard/Assets/Scripts/UIManager.cs`.

Verbatim references in `UIManager.cs`:
- **Line 1:** `using StarVanguard.UI;`
- **Line 164–172:**
  ```csharp
  case eGameState.Menu:
      // MainMenuScreen owns the menu; the router decides what is visible.
      ShowPanel(UIPanelId.Home);
      break;
  case eGameState.Paused:
      ShowPanel(UIPanelId.Pause);
      break;
  case eGameState.GameOver:
      ShowPanel(UIPanelId.GameOver);
      break;
  ```
- **Line 182–186:**
  ```csharp
  if (UIPanelRouter.Instance != null)
  {
      UIPanelRouter.Instance.HideAll();
      return;
  }
  ```
- **Line 196–210:**
  ```csharp
  private static void ShowPanel(UIPanelId i_Id)
  {
      if (UIPanelRouter.Instance != null)
      {
          UIPanelRouter.Instance.Show(i_Id);
      }
  }

  private static void PushPanel(UIPanelId i_Id)
  {
      if (UIPanelRouter.Instance != null)
      {
          UIPanelRouter.Instance.Push(i_Id);
      }
  }
  ```
- **Line 327–345:**
  ```csharp
  public void ShowControlsPanel()
  {
      PushPanel(UIPanelId.Controls);
  }

  public void HideControlsPanel()
  {
      UIPanelRouter.Instance?.Back();
  }

  public void ShowSettingsPanel()
  {
      PushPanel(UIPanelId.Settings);
  }

  public void HideSettingsPanel()
  {
      UIPanelRouter.Instance?.Back();
  }
  ```

Zero other runtime scripts reference `StarVanguard.UI` or any of its types (`ControlsScreen`, `InputMap`, `MainMenuScreen`, `SettingsScreen`, `UIPanelRouter`, `UITheme`, `UIPanelId`).

---

### 1.3 UI -> Runtime Dependencies
Analysis of `Assets/Scripts/UI/` confirmed that `StarVanguard.UI` already depends on `StarVanguard.Runtime`:
- `MainMenuScreen.cs:116, 475, 477, 485, 506, 514, 538`: references `AudioManager.Instance`, `GameManager.Instance`, `LeaderboardManager.Instance`, `eGameState`, `eSFXId`.
- `SettingsScreen.cs:105-107`: references `SettingsUI` (attaches and queries `SettingsUI` on the root GameObject).

This validates the required one-way dependency: `StarVanguard.UI` -> `StarVanguard.Runtime`.

---

### 1.4 Installed Packages & Assembly Environment
Inspecting `Star Vanguard/Packages/manifest.json` and `Library/ScriptAssemblies/`:
- `com.unity.ugui` (`2.5.0`): Assembly `UnityEngine.UI.dll` (Editor: `UnityEditor.UI.dll`).
- `com.unity.ugui` (TextMeshPro bundled): Assembly `Unity.TextMeshPro.dll` (Editor: `Unity.TextMeshPro.Editor.dll`).
- `com.unity.inputsystem` (`1.20.0`): Assembly `Unity.InputSystem.dll`.
- `com.unity.render-pipelines.universal` (`17.6.0`): Assembly `Unity.RenderPipelines.Universal.Runtime.dll` (Editor: `Unity.RenderPipelines.Universal.Editor.dll`).
- `com.unity.test-framework` (`1.7.0`): Assembly `UnityEngine.TestRunner.dll` (Editor: `UnityEditor.TestRunner.dll`).

---

### 1.5 Project Audit Status (`Tools/check_project.py`)
Executing `python Tools/check_project.py`:
- Exit Code: 0 (0 FAIL, 10 WARN, 13 OK).
- Explicit audit warnings concerning T-101:
  - `[WARN] asmdef: Khong co Assembly Definition nao: moi thay doi bien dich lai toan bo, Editor tools lan vao build assembly, khong test duoc doc lap`
  - `[WARN] unit_tests: com.unity.test-framework co trong manifest nhung khong co test NUnit ([Test]/[UnityTest]) -> khong co regression gate that su`

### 1.6 Additional Observation: `StarVanguardValidator.cs` Build Order Check
In `Star Vanguard/Assets/Editor/StarVanguardValidator.cs:129`:
`Assert(scenes[0].path.EndsWith("MainMenuScene.unity"), $"build index 0 is MainMenuScene ({scenes[0].path})");`
However, `ProjectSettings/EditorBuildSettings.asset` and AGENTS.md define:
- Index 0: `Assets/Scenes/BootstrapScene.unity`
- Index 1: `Assets/Scenes/MainMenuScene.unity`
- Index 2: `Assets/Scenes/GameplayScene.unity`
Running `StarVanguardValidator.ValidateAll()` will fail on `CheckBuildSettings` until line 129 is updated to match `BootstrapScene.unity`.

---

## 2. Logic Chain

1. **Premise 1 (Asmdef Boundaries):** Unity's compilation model dictates that any folder containing an `.asmdef` compiles into an isolated assembly. Subfolders containing their own `.asmdef` are excluded from parent assemblies.
2. **Premise 2 (One-Way Directionality):** Requirement R4 specifies `Tests -> UI -> Runtime` and `Editor -> Runtime, UI`. Therefore:
   - `StarVanguard.Runtime` must not reference `StarVanguard.UI`.
   - `StarVanguard.UI` references `StarVanguard.Runtime`.
   - `StarVanguard.Editor` references both `StarVanguard.Runtime` and `StarVanguard.UI`.
   - `StarVanguard.Tests` references both `StarVanguard.Runtime` and `StarVanguard.UI`.
3. **Premise 3 (Direct Blocker):** `UIManager.cs` is in `Assets/Scripts/` (the root of `StarVanguard.Runtime`). Because it has `using StarVanguard.UI;` and directly invokes `UIPanelRouter` and `UIPanelId`, creating `StarVanguard.Runtime.asmdef` without `StarVanguard.UI` in its references will result in immediate compilation errors (`CS0246: The type or namespace name 'StarVanguard.UI' could not be found`). Adding `StarVanguard.UI` to `StarVanguard.Runtime` would introduce a circular assembly dependency (`Runtime <-> UI`).
4. **Premise 4 (Decoupling Mechanism):**
   - In `StarVanguard.Runtime`, `GameManager` already defines `public static event Action<eGameState> OnGameStateChanged;`.
   - Since `StarVanguard.UI` references `StarVanguard.Runtime`, `UIPanelRouter` can subscribe directly to `GameManager.OnGameStateChanged` in `OnEnable`/`OnDisable`.
   - For UI panel requests originated from runtime (`ShowSettingsPanel()`, `ShowControlsPanel()`), `UIManager` can expose C# events (`public static event Action OnShowSettingsRequested;`, etc.) which `UIPanelRouter` listens to.
   - Alternatively, `StarVanguard.Runtime` can define an interface (`IUIPanelRouter`), which `UIPanelRouter` implements and registers.
   - C# event-driven subscription is the standard idiomatic Unity approach and fulfills Requirement R4 directly.
5. **Premise 5 (No YAML Impact):** Scanning all scene and prefab YAML files confirmed that 0 serialized `UnityEvent` entries call `ShowControlsPanel`, `HideControlsPanel`, `ShowSettingsPanel`, `HideSettingsPanel`, `ShowPanel`, or `PushPanel`. Therefore, changing `UIManager.cs` internal implementation does not break any serialized scene or prefab data.

---

## 3. Caveats

1. **Location of Legacy UI Scripts in Runtime:** Scripts such as `GameOverController.cs`, `HUDIcon.cs`, `InterWaveMessageManager.cs`, `LeaderboardUI.cs`, `OverheatGaugeUI.cs`, `PauseController.cs`, and `SettingsUI.cs` reside in `Assets/Scripts/` (global namespace) rather than `Assets/Scripts/UI/`. Moving them to `Assets/Scripts/UI/` is out of scope for Sprint 0 T-101 and would risk breaking GUIDs/meta references or creating reverse runtime dependencies (e.g. `GameManager` and `PlayerStats` call `UIManager`). They are safely compiled inside `StarVanguard.Runtime`.
2. **Global Namespace in Runtime Assembly:** 48 of the 49 runtime scripts do not specify a C# namespace. C# and Unity fully support global-namespace types inside assembly definitions. Subsequent refactoring sprints may enclose them in `namespace StarVanguard`, but for Sprint 0 T-101, keeping their namespaces untouched prevents widespread code churn.
3. **Test Assembly Structure:** `Assets/Tests/` currently does not exist. Creating `StarVanguard.Tests.asmdef` requires creating the `Assets/Tests/` directory with `"optionalUnityReferences": ["TestAssemblies"]`.

---

## 4. Conclusion & Concrete Design

### 4.1 Specification for the 4 Assembly Definitions (`.asmdef`)

#### 1. `StarVanguard.Runtime.asmdef`
- **Location:** `c:\Documents\UNITY 2026\Star Vanguard\Assets\Scripts\StarVanguard.Runtime.asmdef`
- **File Contents:**
```json
{
    "name": "StarVanguard.Runtime",
    "rootNamespace": "",
    "references": [
        "Unity.TextMeshPro",
        "UnityEngine.UI",
        "Unity.InputSystem",
        "Unity.RenderPipelines.Universal.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

#### 2. `StarVanguard.UI.asmdef`
- **Location:** `c:\Documents\UNITY 2026\Star Vanguard\Assets\Scripts\UI\StarVanguard.UI.asmdef`
- **File Contents:**
```json
{
    "name": "StarVanguard.UI",
    "rootNamespace": "StarVanguard.UI",
    "references": [
        "StarVanguard.Runtime",
        "UnityEngine.UI",
        "Unity.TextMeshPro",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

#### 3. `StarVanguard.Editor.asmdef`
- **Location:** `c:\Documents\UNITY 2026\Star Vanguard\Assets\Editor\StarVanguard.Editor.asmdef`
- **File Contents:**
```json
{
    "name": "StarVanguard.Editor",
    "rootNamespace": "",
    "references": [
        "StarVanguard.Runtime",
        "StarVanguard.UI",
        "UnityEngine.UI",
        "UnityEditor.UI",
        "Unity.TextMeshPro",
        "Unity.TextMeshPro.Editor",
        "Unity.RenderPipelines.Universal.Runtime",
        "Unity.RenderPipelines.Universal.Editor"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

#### 4. `StarVanguard.Tests.asmdef`
- **Location:** `c:\Documents\UNITY 2026\Star Vanguard\Assets\Tests\StarVanguard.Tests.asmdef`
- **File Contents:**
```json
{
    "name": "StarVanguard.Tests",
    "rootNamespace": "StarVanguard.Tests",
    "references": [
        "StarVanguard.Runtime",
        "StarVanguard.UI",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "UnityEngine.UI",
        "Unity.TextMeshPro",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false,
    "optionalUnityReferences": [
        "TestAssemblies"
    ]
}
```

---

### 4.2 Decoupling Code Changes: `UIManager.cs` and `UIPanelRouter.cs`

#### Proposed Change in `Star Vanguard/Assets/Scripts/UIManager.cs`:
1. Remove `using StarVanguard.UI;`.
2. Add static C# events for UI navigation:
   ```csharp
   public static event Action OnShowSettingsRequested;
   public static event Action OnHideSettingsRequested;
   public static event Action OnShowControlsRequested;
   public static event Action OnHideControlsRequested;
   public static event Action OnHideAllPanelsRequested;
   ```
3. Update `showState(eGameState i_GameState)`:
   Remove calls to `ShowPanel(UIPanelId...)`. `UIPanelRouter` will respond to `GameManager.OnGameStateChanged` directly.
4. Simplify `hideAllPanels()`:
   ```csharp
   private void hideAllPanels()
   {
       OnHideAllPanelsRequested?.Invoke();

       // Fallback for scene objects
       SetPanelActive(m_GameOverPanel, false);
       SetPanelActive(m_PausePanel, false);
       SetPanelActive(m_HomeScreenPanel, false);
       SetPanelActive(m_ControlsPanel, false);
       SetPanelActive(m_SettingsPanel, false);
   }
   ```
5. Remove `ShowPanel(UIPanelId)` and `PushPanel(UIPanelId)`.
6. Update navigation trigger methods:
   ```csharp
   public void ShowControlsPanel() => OnShowControlsRequested?.Invoke();
   public void HideControlsPanel() => OnHideControlsRequested?.Invoke();
   public void ShowSettingsPanel() => OnShowSettingsRequested?.Invoke();
   public void HideSettingsPanel() => OnHideSettingsRequested?.Invoke();
   ```

#### Proposed Change in `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`:
Subscribe to `GameManager.OnGameStateChanged` and `UIManager` events in `OnEnable`/`OnDisable`:
```csharp
private void OnEnable()
{
    GameManager.OnGameStateChanged += HandleGameStateChanged;
    UIManager.OnShowSettingsRequested += HandleShowSettings;
    UIManager.OnHideSettingsRequested += HandleBack;
    UIManager.OnShowControlsRequested += HandleShowControls;
    UIManager.OnHideControlsRequested += HandleBack;
    UIManager.OnHideAllPanelsRequested += HideAll;
}

private void OnDisable()
{
    GameManager.OnGameStateChanged -= HandleGameStateChanged;
    UIManager.OnShowSettingsRequested -= HandleShowSettings;
    UIManager.OnHideSettingsRequested -= HandleBack;
    UIManager.OnShowControlsRequested -= HandleShowControls;
    UIManager.OnHideControlsRequested -= HandleBack;
    UIManager.OnHideAllPanelsRequested -= HideAll;
}

private void HandleGameStateChanged(eGameState i_State)
{
    switch (i_State)
    {
        case eGameState.Menu:
            Show(UIPanelId.Home);
            break;
        case eGameState.Paused:
            Show(UIPanelId.Pause);
            break;
        case eGameState.GameOver:
            Show(UIPanelId.GameOver);
            break;
        case eGameState.Playing:
            HideAll();
            break;
    }
}

private void HandleShowSettings() => Push(UIPanelId.Settings);
private void HandleShowControls() => Push(UIPanelId.Controls);
private void HandleBack() => Back();
```

---

### 4.3 Recommended Step-by-Step Implementation Sequence for Implementer

1. **Step 1 (Decouple Runtime -> UI):**
   - Apply edits to `UIManager.cs` and `UIPanelRouter.cs`.
   - Remove `using StarVanguard.UI;` and references to `UIPanelId`/`UIPanelRouter` from `UIManager.cs`.
2. **Step 2 (Create Runtime Asmdef):**
   - Create `Assets/Scripts/StarVanguard.Runtime.asmdef`.
   - Refresh Unity and check console (`read_console`). Verify `StarVanguard.Runtime.dll` compiles with 0 errors.
3. **Step 3 (Create UI Asmdef):**
   - Create `Assets/Scripts/UI/StarVanguard.UI.asmdef`.
   - Refresh Unity and verify `StarVanguard.UI.dll` compiles with 0 errors.
4. **Step 4 (Create Editor Asmdef):**
   - Create `Assets/Editor/StarVanguard.Editor.asmdef`.
   - Refresh Unity and verify `StarVanguard.Editor.dll` compiles with 0 errors.
5. **Step 5 (Create Tests Asmdef):**
   - Create directory `Assets/Tests/` and file `Assets/Tests/StarVanguard.Tests.asmdef`.
   - Refresh Unity and verify compilation.
6. **Step 6 (Fix Validator Scene Index Check):**
   - In `StarVanguardValidator.cs:129`, update index 0 assertion from `MainMenuScene.unity` to `BootstrapScene.unity` so Gate G2 passes cleanly.

---

## 5. Verification Method

To independently verify these findings:

1. **Verify No Other Runtime -> UI Dependencies:**
   Run in PowerShell:
   ```powershell
   python -c "import os, re; dir_p = r'Star Vanguard\Assets\Scripts'; print([f for f in os.listdir(dir_p) if f.endswith('.cs') and 'StarVanguard.UI' in open(os.path.join(dir_p, f), encoding='utf-8', errors='ignore').read()])"
   ```
   *Expected result:* `['UIManager.cs']`.

2. **Verify Static Audit Gate G3:**
   ```powershell
   python Tools/check_project.py
   ```
   *Success Condition:* 0 FAIL. Once `.asmdef` files are created, the `[WARN] asmdef` check must turn to `[ OK ] asmdef: 4 asmdef`.

3. **Verify Gate G1 (Compilation):**
   After applying the decoupling patch and writing the 4 `.asmdef` files, run MCP `refresh_unity` and `read_console` (filter Error).
   *Success Condition:* 0 compiler errors.

4. **Invalidation Conditions:**
   - If any new script is added to `Assets/Scripts/` with `using StarVanguard.UI;`, `StarVanguard.Runtime` will fail compilation.
   - If `StarVanguard.UI.asmdef` omits `"StarVanguard.Runtime"` from its references, `MainMenuScreen.cs` and `SettingsScreen.cs` will fail compilation.
