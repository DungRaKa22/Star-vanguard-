# Handoff Report — Runtime-to-UI Decoupling Specification (T-101a)

**Executive Summary:**
A complete static and dynamic audit of `Star Vanguard` confirms that `Star Vanguard/Assets/Scripts/UIManager.cs` is the **only** runtime script referencing the `StarVanguard.UI` namespace and its types (`UIPanelRouter`, `UIPanelId`). All 48 other runtime scripts in `Star Vanguard/Assets/Scripts/` are completely free of UI dependencies. This report defines the exact event-driven decoupling plan between `UIManager` (Runtime) and `UIPanelRouter` (UI), providing exact before/after code snippets with line numbers, architectural contracts, lifecycle safeguards, and a verified machine-applicable patch (`t101a_decoupling.patch`) that satisfies `git apply --check` with zero errors.

---

## 1. Observation

### 1.1 Complete Audit of `StarVanguard.UI` References in Runtime
An exhaustive regex search across all 49 `.cs` files in `Star Vanguard/Assets/Scripts/` (excluding the `Assets/Scripts/UI/` subfolder) was executed using the following verification script:
```python
import os, re
scripts_dir = r'Star Vanguard\Assets\Scripts'
ui_dir = os.path.join(scripts_dir, 'UI')
types = ['StarVanguard.UI', 'ControlsScreen', 'InputMap', 'MainMenuScreen', 'SettingsScreen', 'UIPanelRouter', 'UIPanelId', 'UITheme']
results = {}
for root, dirs, files in os.walk(scripts_dir):
    if os.path.abspath(root).startswith(os.path.abspath(ui_dir)):
        continue
    for f in files:
        if f.endswith('.cs'):
            path = os.path.join(root, f)
            with open(path, 'r', encoding='utf-8', errors='ignore') as fp:
                content = fp.read()
                matches = [t for t in types if re.search(r'\b' + re.escape(t) + r'\b', content)]
                if matches:
                    results[f] = matches
print(results)
```
**Command Output:**
```
{'UIManager.cs': ['StarVanguard.UI', 'MainMenuScreen', 'UIPanelRouter', 'UIPanelId']}
```
*(Note: `MainMenuScreen` in `UIManager.cs` is solely an informational code comment on line 164).*

**Finding:** Exactly 48 of 49 runtime scripts have zero references to `StarVanguard.UI` or any of its types. `UIManager.cs` is the sole coupling point preventing one-way assembly boundary isolation.

### 1.2 Verbatim Couplings in `Star Vanguard/Assets/Scripts/UIManager.cs`
The exact lines coupling `UIManager.cs` to `StarVanguard.UI` were inspected:
1. **Line 1:**
   ```csharp
   using StarVanguard.UI;
   ```
2. **Lines 163–177 (`showState`):**
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
3. **Lines 180–186 (`hideAllPanels`):**
   ```csharp
   if (UIPanelRouter.Instance != null)
   {
       UIPanelRouter.Instance.HideAll();
       return;
   }
   ```
4. **Lines 196–210 (`ShowPanel` and `PushPanel` helper methods):**
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
5. **Lines 327–351 (Panel navigation methods):**
   ```csharp
   public void ShowControlsPanel()
   {
       PushPanel(UIPanelId.Controls);
   }

   public void HideControlsPanel()
   {
       if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Controls)
       {
           UIPanelRouter.Instance.Back();
       }
   }

   public void ShowSettingsPanel()
   {
       PushPanel(UIPanelId.Settings);
   }

   public void HideSettingsPanel()
   {
       if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Settings)
       {
           UIPanelRouter.Instance.Back();
       }
   }
   ```

### 1.3 Callers of `UIManager` Panel Navigation Methods
Cross-referencing calls to `ShowControlsPanel`, `HideControlsPanel`, `ShowSettingsPanel`, `HideSettingsPanel`:
- `Star Vanguard/Assets/Scripts/SettingsUI.cs`:
  - Line 127: `UIManager.Instance?.ShowSettingsPanel();`
  - Line 139: `UIManager.Instance?.HideSettingsPanel();`
- `Star Vanguard/Assets/Scripts/ControlsPanelController.cs`:
  - Line 7: `UIManager.Instance?.HideControlsPanel();`
- `Star Vanguard/Assets/Editor/StarVanguardSceneSplitter.cs`:
  - Line 331: `button.onClick.GetPersistentMethodName(i) == nameof(UIManager.HideSettingsPanel);`

**Finding:** The public method signatures `ShowControlsPanel()`, `HideControlsPanel()`, `ShowSettingsPanel()`, and `HideSettingsPanel()` on `UIManager` must remain intact so runtime callers (`SettingsUI`, `ControlsPanelController`) and Editor reflection continue to function seamlessly.

### 1.4 UnityEvent Serialization in Scene and Prefab YAML
A scan of all `.unity` and `.prefab` files for persistent `UnityEvent` listeners targeting `ShowControlsPanel`, `HideControlsPanel`, `ShowSettingsPanel`, `HideSettingsPanel`, `ShowPanel`, `PushPanel`, or `hideAllPanels` yielded **0 occurrences**:
```
Scene/Prefab UnityEvent hits: {}
```
**Finding:** Updating the internal implementation of `UIManager.cs` causes zero changes to serialized scene or prefab YAML data, conforming strictly to AGENTS.md Hard Rule #1.

### 1.5 Current State of `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`
Inspection of `UIPanelRouter.cs`:
- Class `UIPanelRouter : MonoBehaviour` defines `public static UIPanelRouter Instance { get; private set; }`.
- Defines `Awake()` (lines 57–67) and `OnDestroy()` (lines 69–75).
- Currently lacks `OnEnable()`, `OnDisable()`, and `Start()`.
- Scene panels are discovered via `DiscoverPanels()` and registered into `m_Panels`.
- Contains methods `Show(UIPanelId)`, `Push(UIPanelId)`, `Back()`, and `HideAll()`.

---

## 2. Logic Chain

1. **Assembly Boundary Requirement (R4 & T-101):** The project requires 4 one-way assembly definitions:
   - `StarVanguard.Runtime`
   - `StarVanguard.UI` (references `StarVanguard.Runtime`)
   - `StarVanguard.Editor` (references `StarVanguard.Runtime`, `StarVanguard.UI`)
   - `StarVanguard.Tests` (references `StarVanguard.Runtime`, `StarVanguard.UI`)
   Because compilation units cannot have cyclic dependencies, `StarVanguard.Runtime` must never reference types in `StarVanguard.UI`.
2. **Identification of the Violation:** `UIManager.cs` is located in `Assets/Scripts/` (part of `StarVanguard.Runtime`), but directly uses `using StarVanguard.UI;` and calls `UIPanelRouter` and `UIPanelId`. If `StarVanguard.Runtime.asmdef` is created now, it will fail to compile.
3. **Event-Driven Decoupling Pattern:**
   - In standard Unity decoupled architecture, high-level runtime components publish domain events, and UI presentation components subscribe to them.
   - `GameManager` already publishes `public static event Action<eGameState> OnGameStateChanged;`.
   - `UIPanelRouter` in `StarVanguard.UI` can directly observe `GameManager.OnGameStateChanged`. It can autonomously transition the top-level screens:
     - `eGameState.Menu` $\rightarrow$ `Show(UIPanelId.Home)`
     - `eGameState.Paused` $\rightarrow$ `Show(UIPanelId.Pause)`
     - `eGameState.GameOver` $\rightarrow$ `Show(UIPanelId.GameOver)`
     - `eGameState.Playing` $\rightarrow$ `HideAll()`
   - For sub-panel navigation (Settings, Controls) initiated by runtime scripts (`SettingsUI`, `ControlsPanelController`), `UIManager` can publish static C# events:
     - `public static event Action OnShowSettingsRequested;`
     - `public static event Action OnHideSettingsRequested;`
     - `public static event Action OnShowControlsRequested;`
     - `public static event Action OnHideControlsRequested;`
     - `public static event Action OnShowAudioSettingsRequested;`
     - `public static event Action OnHideSubPanelsRequested;`
     - `public static event Action OnHideAllPanelsRequested;`
4. **Subscription Lifecycle & Leak Prevention:**
   - Following AGENTS.md Rule #4, `UIPanelRouter` subscribes in `OnEnable()` and unsubscribes in `OnDisable()`.
   - `UIPanelRouter` must not invoke `Back()` indiscriminately; `HandleHideSettings()` verifies `if (Current == UIPanelId.Settings) Back();`, and `HandleHideControls()` verifies `if (Current == UIPanelId.Controls) Back();`, preventing double-pop bugs identified in `StarVanguardPlayModeSmoke.cs`.
5. **Separation of Concerns:**
   - `UIManager.showState()` retains responsibility for gameplay HUD visibility (`m_HUDContainer.SetActive(willBePlaying)`), `OnEnterPlayingState()`, and `OnLeavePlayingState()`.
   - `UIManager` no longer micromanages `UIPanelRouter`, eliminating race conditions between `showState` and `HandleGameStateChanged`.

---

## 3. Caveats

1. **Legacy Fallback GameObjects in `UIManager`:** `UIManager.cs` retains serialized fields for `m_HomeScreenPanel`, `m_PausePanel`, `m_GameOverPanel`, `m_ControlsPanel`, and `m_SettingsPanel`. These fields are retained so that any older scene setups continue to compile without missing field warnings; `SetPanelActive()` continues to act as a fallback if those fields are assigned.
2. **`SettingsUI.cs` Placement:** `SettingsUI.cs` remains in `Assets/Scripts/` (part of `StarVanguard.Runtime`) because it interacts with `SettingsManager`. It communicates with the UI layer via `UIManager.Instance.ShowSettingsPanel()` / `HideSettingsPanel()`, which dispatch the events.
3. **Execution Order on Startup:** When entering a scene, `UIPanelRouter.Start()` checks `if (GameManager.Instance != null && m_Stack.Count == 0)` and synchronizes to `GameManager.Instance.GameState`. If `MainMenuScreen.Awake()` has already called `Show(UIPanelId.Home)`, `m_Stack.Count` is 1, so `Start()` will not duplicate the operation.

---

## 4. Conclusion & Concrete Specification

### 4.1 Specification for `Star Vanguard/Assets/Scripts/UIManager.cs`

#### Change 1: Namespace Imports (Lines 1–4)
**Before:**
```csharp
using StarVanguard.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
```
**After:**
```csharp
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
```

#### Change 2: Expose UI Navigation C# Events (Lines 11–13)
**Before:**
```csharp
    protected override bool PersistAcrossScenes => false;

    [Header("HUD Elements")]
```
**After:**
```csharp
    protected override bool PersistAcrossScenes => false;

    #region UI Navigation Events
    public static event Action OnShowSettingsRequested;
    public static event Action OnHideSettingsRequested;
    public static event Action OnShowControlsRequested;
    public static event Action OnHideControlsRequested;
    public static event Action OnShowAudioSettingsRequested;
    public static event Action OnHideSubPanelsRequested;
    public static event Action OnHideAllPanelsRequested;
    #endregion

    [Header("HUD Elements")]
```

#### Change 3: Unconditional Unsubscribe in `OnDestroy()` (Lines 68–72)
**Before:**
```csharp
        if (GameManager.HasInstance)
        {
            GameManager.OnGameStateChanged -= showState;
        }
```
**After:**
```csharp
        GameManager.OnGameStateChanged -= showState;
```

#### Change 4: Decouple `showState()` from `UIPanelRouter` (Lines 160–178)
**Before:**
```csharp
        // Show appropriate panel for current state
        switch (i_GameState)
        {
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
            case eGameState.Playing:
                // No panels active during gameplay, but HUD is visible
                OnEnterPlayingState();
                break;
        }
```
**After:**
```csharp
        // Handle gameplay state transitions
        switch (i_GameState)
        {
            case eGameState.Menu:
                SetPanelActive(m_HomeScreenPanel, true);
                break;
            case eGameState.Paused:
                SetPanelActive(m_PausePanel, true);
                break;
            case eGameState.GameOver:
                SetPanelActive(m_GameOverPanel, true);
                break;
            case eGameState.Playing:
                // No panels active during gameplay, but HUD is visible
                OnEnterPlayingState();
                break;
        }
```

#### Change 5: Remove `UIPanelRouter` Calls and Helper Methods (Lines 180–210)
**Before:**
```csharp
    private void hideAllPanels()
    {
        if (UIPanelRouter.Instance != null)
        {
            UIPanelRouter.Instance.HideAll();
            return;
        }

        // Fallback for the very first frame, before the router has registered panels.
        SetPanelActive(m_GameOverPanel, false);
        SetPanelActive(m_PausePanel, false);
        SetPanelActive(m_HomeScreenPanel, false);
        SetPanelActive(m_ControlsPanel, false);
        SetPanelActive(m_SettingsPanel, false);
    }

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
**After:**
```csharp
    private void hideAllPanels()
    {
        // Fallback for serialized scene panels if assigned
        SetPanelActive(m_GameOverPanel, false);
        SetPanelActive(m_PausePanel, false);
        SetPanelActive(m_HomeScreenPanel, false);
        SetPanelActive(m_ControlsPanel, false);
        SetPanelActive(m_SettingsPanel, false);
    }
```

#### Change 6: Replace Panel Navigation Triggers with Event Invocations (Lines 327–351)
**Before:**
```csharp
    public void ShowControlsPanel()
    {
        PushPanel(UIPanelId.Controls);
    }

    public void HideControlsPanel()
    {
        if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Controls)
        {
            UIPanelRouter.Instance.Back();
        }
    }

    public void ShowSettingsPanel()
    {
        PushPanel(UIPanelId.Settings);
    }

    public void HideSettingsPanel()
    {
        if (UIPanelRouter.Instance != null && UIPanelRouter.Instance.Current == UIPanelId.Settings)
        {
            UIPanelRouter.Instance.Back();
        }
    }
```
**After:**
```csharp
    public void ShowControlsPanel()
    {
        OnShowControlsRequested?.Invoke();
    }

    public void HideControlsPanel()
    {
        OnHideControlsRequested?.Invoke();
        OnHideSubPanelsRequested?.Invoke();
    }

    public void ShowSettingsPanel()
    {
        OnShowSettingsRequested?.Invoke();
    }

    public void HideSettingsPanel()
    {
        OnHideSettingsRequested?.Invoke();
        OnHideSubPanelsRequested?.Invoke();
    }

    public void ShowAudioSettingsPanel()
    {
        OnShowAudioSettingsRequested?.Invoke();
    }

    public void HideSubPanels()
    {
        OnHideSubPanelsRequested?.Invoke();
    }

    public void HideAllPanels()
    {
        OnHideAllPanelsRequested?.Invoke();
        hideAllPanels();
    }
```

---

### 4.2 Specification for `Star Vanguard/Assets/Scripts/UI/UIPanelRouter.cs`

#### Change 1: Add `OnEnable()`, `OnDisable()`, and `Start()` (Lines 68–75)
**Before:**
```csharp
            Instance = this;
            DiscoverPanels();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
```
**After:**
```csharp
            Instance = this;
            DiscoverPanels();
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            UIManager.OnShowSettingsRequested += HandleShowSettings;
            UIManager.OnHideSettingsRequested += HandleHideSettings;
            UIManager.OnShowControlsRequested += HandleShowControls;
            UIManager.OnHideControlsRequested += HandleHideControls;
            UIManager.OnShowAudioSettingsRequested += HandleShowAudioSettings;
            UIManager.OnHideSubPanelsRequested += HandleHideSubPanels;
            UIManager.OnHideAllPanelsRequested += HandleHideAll;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            UIManager.OnShowSettingsRequested -= HandleShowSettings;
            UIManager.OnHideSettingsRequested -= HandleHideSettings;
            UIManager.OnShowControlsRequested -= HandleShowControls;
            UIManager.OnHideControlsRequested -= HandleHideControls;
            UIManager.OnShowAudioSettingsRequested -= HandleShowAudioSettings;
            UIManager.OnHideSubPanelsRequested -= HandleHideSubPanels;
            UIManager.OnHideAllPanelsRequested -= HandleHideAll;
        }

        private void Start()
        {
            // Sync with current game state on startup if no panel is currently active
            if (GameManager.Instance != null && m_Stack.Count == 0)
            {
                HandleGameStateChanged(GameManager.Instance.GameState);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
```

#### Change 2: Add Navigation Event Handlers (Lines 181–183)
**Before:**
```csharp
        public void HideAll()
        {
            if (s_DebugTrace)
            {
                Debug.LogWarning($"[UI-ROUTER] HideAll() depth={m_Stack.Count}\n{new System.Diagnostics.StackTrace(true)}");
            }

            m_Stack.Clear();
            ApplyVisibility(UIPanelId.None);
        }

        private void ApplyVisibility(UIPanelId i_Visible)
```
**After:**
```csharp
        public void HideAll()
        {
            if (s_DebugTrace)
            {
                Debug.LogWarning($"[UI-ROUTER] HideAll() depth={m_Stack.Count}\n{new System.Diagnostics.StackTrace(true)}");
            }

            m_Stack.Clear();
            ApplyVisibility(UIPanelId.None);
        }

        #region Navigation Event Handlers
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

        private void HandleHideSettings()
        {
            if (Current == UIPanelId.Settings)
            {
                Back();
            }
        }

        private void HandleShowControls() => Push(UIPanelId.Controls);

        private void HandleHideControls()
        {
            if (Current == UIPanelId.Controls)
            {
                Back();
            }
        }

        private void HandleShowAudioSettings() => Push(UIPanelId.Settings);

        private void HandleHideSubPanels()
        {
            if (Current == UIPanelId.Settings || Current == UIPanelId.Controls)
            {
                Back();
            }
        }

        private void HandleHideAll() => HideAll();
        #endregion

        private void ApplyVisibility(UIPanelId i_Visible)
```

---

### 4.3 Verified Artifacts Available in Explorer Directory
The following verified artifact files have been prepared in `.agents/teamwork/teamwork_explorer_m2_1/`:
1. `t101a_decoupling.patch`: Unified diff patch applicable via `git apply`.
2. `proposed_UIManager.cs`: Full proposed file content for `UIManager.cs`.
3. `proposed_UIPanelRouter.cs`: Full proposed file content for `UIPanelRouter.cs`.

---

## 5. Verification Method

### 5.1 Patch Dry-Run Verification
To verify the patch applies without conflict:
```powershell
git apply --check ".agents/teamwork/teamwork_explorer_m2_1/t101a_decoupling.patch"
```
*Expected Result:* Return code 0 with 0 errors or warnings. (Verified: PASSED).

### 5.2 Decoupling Verification (No UI References in Runtime)
To independently verify that zero runtime scripts reference `StarVanguard.UI`:
```powershell
python -c "import os, re; dir_p = r'Star Vanguard\Assets\Scripts'; print([f for f in os.listdir(dir_p) if f.endswith('.cs') and 'StarVanguard.UI' in open(os.path.join(dir_p, f), encoding='utf-8', errors='ignore').read()])"
```
*Expected Result:* Empty list `[]` after the patch is applied.

### 5.3 Gate Verification Commands (Upon Implementation)
Once the patch is applied and the 4 `.asmdef` files are created:
1. **Gate G1 (Compile):**
   Run `refresh_unity` followed by `read_console` (filter Error).
   *Success Condition:* 0 compilation errors.
2. **Gate G2 (Regression):**
   Execute `StarVanguard.EditorTools.StarVanguardValidator.ValidateAll`.
   *Success Condition:* 100% checks pass.
3. **Gate G3 (Static Project Audit):**
   ```powershell
   python Tools/check_project.py
   ```
   *Success Condition:* 0 FAIL, 0 asmdef warnings.

### 5.4 Invalidation Conditions
- Any new runtime script in `Assets/Scripts/` containing `using StarVanguard.UI;` or references to `UIPanelId`/`UIPanelRouter` will violate the assembly boundary and break compilation.
- Subscribing to `OnHideSubPanelsRequested` without verifying `Current == UIPanelId.Settings || Current == UIPanelId.Controls` would risk invalid stack pops.
