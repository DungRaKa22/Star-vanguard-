# Handoff Report — Assembly Definitions Specification (T-101b)

**Executive Summary:**
A complete investigation of the Star Vanguard codebase and Unity package ecosystem was conducted to specify the four assembly definition files required by Sprint 0 (T-101b). The architecture establishes a strict one-way dependency hierarchy (`StarVanguard.Tests` -> `StarVanguard.UI`, `StarVanguard.Runtime`; `StarVanguard.Editor` -> `StarVanguard.Runtime`, `StarVanguard.UI`; `StarVanguard.UI` -> `StarVanguard.Runtime`; `StarVanguard.Runtime` -> zero internal dependencies). A key finding not captured in previous surveys is that `StarVanguardInputWiring.cs` (in `Assets/Editor/`) requires `"Unity.InputSystem"` in `StarVanguard.Editor.asmdef`. Full JSON configurations, package references, and verification methods are detailed below.

---

## 1. Observation

### 1.1 Script Catalog and Assembly Boundaries
Static directory analysis of `Star Vanguard/Assets/` identified 67 C# scripts across three active directories (and one planned test directory):

| Target Assembly | Target Folder | Script Count | Namespaces Present |
|:---|:---|:---:|:---|
| `StarVanguard.Runtime` | `Star Vanguard/Assets/Scripts/` | 49 | `(global)` (48 files), `StarVanguard` (1 file: `BootstrapLoader.cs`) |
| `StarVanguard.UI` | `Star Vanguard/Assets/Scripts/UI/` | 6 | `StarVanguard.UI` (6 files) |
| `StarVanguard.Editor` | `Star Vanguard/Assets/Editor/` | 12 | `StarVanguard.Editor` (4 files), `StarVanguard.EditorTools` (8 files) |
| `StarVanguard.Tests` | `Star Vanguard/Assets/Tests/` | 0 (New) | `StarVanguard.Tests` (Target namespace) |

Unity compilation rule: When `StarVanguard.UI.asmdef` is placed in `Assets/Scripts/UI/`, Unity treats `Assets/Scripts/UI/` as a distinct assembly and automatically excludes it and all of its subdirectories from the parent assembly defined in `Assets/Scripts/StarVanguard.Runtime.asmdef`.

### 1.2 Package Assembly Verification (`PackageCache` & `Library/ScriptAssemblies/`)
Direct inspection of `Star Vanguard/Library/ScriptAssemblies/` and package manifests in `Library/PackageCache/` verified the exact assembly names registered by Unity:

1. **Unity Input System (`com.unity.inputsystem@1.20.0`):**
   - Asmdef name: `Unity.InputSystem` (`Library/PackageCache/com.unity.inputsystem@7a4e1a2a8194/InputSystem/Unity.InputSystem.asmdef`)
   - Compiled DLL: `Library/ScriptAssemblies/Unity.InputSystem.dll`
2. **Unity UI / uGUI (`com.unity.ugui@2.5.0`):**
   - Runtime Asmdef: `UnityEngine.UI` (`Library/PackageCache/com.unity.ugui@67707a67a4ab/Runtime/UGUI/UnityEngine.UI.asmdef`)
   - Editor Asmdef: `UnityEditor.UI` (`Library/PackageCache/com.unity.ugui@67707a67a4ab/Editor/UGUI/UnityEditor.UI.asmdef`)
   - Compiled DLLs: `Library/ScriptAssemblies/UnityEngine.UI.dll`, `UnityEditor.UI.dll`
3. **TextMeshPro (`com.unity.ugui@2.5.0`):**
   - Runtime Asmdef: `Unity.TextMeshPro` (`Library/PackageCache/com.unity.ugui@67707a67a4ab/Runtime/TMP/Unity.TextMeshPro.asmdef`)
   - Editor Asmdef: `Unity.TextMeshPro.Editor` (`Library/PackageCache/com.unity.ugui@67707a67a4ab/Editor/TMP/Unity.TextMeshPro.Editor.asmdef`)
   - Compiled DLLs: `Library/ScriptAssemblies/Unity.TextMeshPro.dll`, `Unity.TextMeshPro.Editor.dll`
4. **Universal Render Pipeline (`com.unity.render-pipelines.universal@17.6.0`):**
   - Runtime Asmdef: `Unity.RenderPipelines.Universal.Runtime`
   - Editor Asmdef: `Unity.RenderPipelines.Universal.Editor`
   - Compiled DLLs: `Library/ScriptAssemblies/Unity.RenderPipelines.Universal.Runtime.dll`, `Unity.RenderPipelines.Universal.Editor.dll`
5. **Unity Test Framework (`com.unity.test-framework@1.7.0`):**
   - Runtime Asmdef: `UnityEngine.TestRunner`
   - Editor Asmdef: `UnityEditor.TestRunner`
   - Precompiled Test Assembly: `nunit.framework.dll`

### 1.3 Missing Dependency in Prior Editor Specification
Static analysis of all 12 editor scripts in `Assets/Editor/` revealed that `StarVanguardInputWiring.cs` (created in Sprint 0 Milestone 1 for T-204b) explicitly imports:
- **Line 7:** `using UnityEngine.InputSystem;`
- **Line 8:** `using UnityEngine.InputSystem.UI;`

In the survey handoff (`teamwork_explorer_survey_2/handoff.md`), `StarVanguard.Editor.asmdef` omitted `"Unity.InputSystem"`. Without `"Unity.InputSystem"` in its references, `StarVanguard.Editor` will fail compilation with error:
`CS0246: The type or namespace name 'InputSystem' could not be found in namespace 'UnityEngine'`.
`"Unity.InputSystem"` must therefore be explicitly added to `StarVanguard.Editor.asmdef`.

### 1.4 Runtime Script Dependencies
Analysis of all 49 files in `Assets/Scripts/` showed:
- Using directives: `System`, `System.Collections`, `System.Collections.Generic`, `UnityEngine`, `UnityEngine.SceneManagement`, `UnityEngine.Serialization`, `UnityEngine.UI`, `TMPro`, `StarVanguard.UI` (only in `UIManager.cs`).
- Exactly 1 file (`UIManager.cs`) references `StarVanguard.UI`.
- 0 files in `Assets/Scripts/` reference `UnityEditor` or `StarVanguard.Editor`.
- Once `UIManager.cs` is decoupled via C# events (T-101a), `StarVanguard.Runtime` has zero references to any other project assembly.

### 1.5 Current Project Audit Baseline (`Tools/check_project.py`)
Running `python Tools/check_project.py`:
- Line 332: Checks for `.asmdef` files in `Assets/`.
- Currently reports: `[WARN] asmdef: Khong co Assembly Definition nao...`
- Adding the 4 specified `.asmdef` files turns this check into: `[ OK ] asmdef: 4 asmdef`.

---

## 2. Logic Chain

1. **Premise 1 (Unity Assembly Isolation):** An `.asmdef` file defines a compilation unit. Code inside an assembly cannot reference code outside unless that assembly is explicitly declared in its `"references"` array.
2. **Premise 2 (Folder Hierarchy and Overrides):** `Assets/Scripts/` is the root of `StarVanguard.Runtime`. Because `Assets/Scripts/UI/` contains `StarVanguard.UI.asmdef`, Unity isolates the `UI` folder and does not compile it into `StarVanguard.Runtime.dll`.
3. **Premise 3 (Strict One-Way Requirement R4):**
   - `StarVanguard.Runtime` must have zero project assembly dependencies. It provides domain logic, gameplay mechanics, singletons, state management, and pooling.
   - `StarVanguard.UI` references `StarVanguard.Runtime`. It requires `GameManager` (game state), `AudioManager` (audio triggers), `LeaderboardManager`, and `SettingsUI`.
   - `StarVanguard.Editor` references both `StarVanguard.Runtime` and `StarVanguard.UI` to inspect, bake, validate, and wire scenes.
   - `StarVanguard.Tests` references both `StarVanguard.Runtime` and `StarVanguard.UI` to execute unit and regression tests.
4. **Premise 4 (Dependency Completeness):**
   - `StarVanguard.Runtime` requires `UnityEngine.UI` and `Unity.TextMeshPro` (for HUD components), and `Unity.InputSystem` + `Unity.RenderPipelines.Universal.Runtime` (for input and rendering pipeline types).
   - `StarVanguard.UI` requires `StarVanguard.Runtime`, `UnityEngine.UI`, `Unity.TextMeshPro`, and `Unity.InputSystem`.
   - `StarVanguard.Editor` requires `StarVanguard.Runtime`, `StarVanguard.UI`, `Unity.InputSystem` (for `StarVanguardInputWiring.cs`), `UnityEngine.UI`, `UnityEditor.UI`, `Unity.TextMeshPro`, `Unity.TextMeshPro.Editor`, `Unity.RenderPipelines.Universal.Runtime`, and `Unity.RenderPipelines.Universal.Editor`.
   - `StarVanguard.Tests` requires `StarVanguard.Runtime`, `StarVanguard.UI`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `nunit.framework.dll`, `UnityEngine.UI`, `Unity.TextMeshPro`, and `Unity.InputSystem`.
5. **Premise 5 (No YAML or Meta Corruption):** Assembly definitions in Unity preserve asset GUIDs. Scripts in scenes and prefabs reference `.cs.meta` GUIDs, not assembly names. No YAML edits are made.

---

## 3. Caveats

1. **Decoupling Prerequisite (T-101a):** `StarVanguard.Runtime.asmdef` cannot compile until `UIManager.cs` is decoupled from `StarVanguard.UI`. If the asmdef is added before removing `using StarVanguard.UI;` and references to `UIPanelRouter`/`UIPanelId`, Unity will emit CS0246 compilation errors.
2. **Missing `Assets/Tests` Folder:** The folder `Star Vanguard/Assets/Tests/` does not currently exist. The implementer must create this directory when adding `StarVanguard.Tests.asmdef`.
3. **Validator Assertion Adjustment (T-101c):** `StarVanguardValidator.cs:129` asserts that build index 0 is `MainMenuScene.unity`. In `EditorBuildSettings.asset`, index 0 is `BootstrapScene.unity`. This check will fail Gate G2 until aligned in T-101c.
4. **Global Namespace Retention:** 48 runtime scripts reside in the global namespace. Creating `StarVanguard.Runtime.asmdef` does not require changing namespaces, avoiding high-churn edits in Sprint 0.

---

## 4. Conclusion & Concrete Specification

### 4.1 Dependency Architecture Diagram

```
+-------------------------------------------------------------+
|                     StarVanguard.Tests                      |
|                  (Assets/Tests/*.asmdef)                    |
+------------------------------+------------------------------+
                               |
            +------------------+------------------+
            |                                     |
            v                                     v
+-----------------------------+     +-----------------------------+
|     StarVanguard.Editor     |     |       StarVanguard.UI       |
|    (Assets/Editor/*.asmdef) |     |  (Assets/Scripts/UI/*.asmdef)
+--------------+--------------+     +--------------+--------------+
               |                                   |
               +-----------------+-----------------+
                                 |
                                 v
              +-------------------------------------+
              |         StarVanguard.Runtime        |
              |       (Assets/Scripts/*.asmdef)     |
              |       [Zero Internal References]    |
              +-------------------------------------+
```

---

### 4.2 Exact JSON Content for the 4 Assembly Definitions

#### File 1: `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`
- **Location:** `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`
- **Platform Inclusions:** All platforms (`"includePlatforms": []`, `"excludePlatforms": []`)
- **Auto-Referenced:** `true`
- **References:**
  - `Unity.TextMeshPro`
  - `UnityEngine.UI`
  - `Unity.InputSystem`
  - `Unity.RenderPipelines.Universal.Runtime`
- **Exact File Content:**
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

---

#### File 2: `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`
- **Location:** `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`
- **Platform Inclusions:** All platforms (`"includePlatforms": []`, `"excludePlatforms": []`)
- **Auto-Referenced:** `false` (enforces explicit opt-in; prevents runtime from accidentally linking UI)
- **References:**
  - `StarVanguard.Runtime`
  - `UnityEngine.UI`
  - `Unity.TextMeshPro`
  - `Unity.InputSystem`
- **Exact File Content:**
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

---

#### File 3: `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`
- **Location:** `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`
- **Platform Inclusions:** Editor only (`"includePlatforms": ["Editor"]`, `"excludePlatforms": []`)
- **Auto-Referenced:** `true`
- **References:**
  - `StarVanguard.Runtime`
  - `StarVanguard.UI`
  - `Unity.InputSystem`
  - `UnityEngine.UI`
  - `UnityEditor.UI`
  - `Unity.TextMeshPro`
  - `Unity.TextMeshPro.Editor`
  - `Unity.RenderPipelines.Universal.Runtime`
  - `Unity.RenderPipelines.Universal.Editor`
- **Exact File Content:**
```json
{
    "name": "StarVanguard.Editor",
    "rootNamespace": "",
    "references": [
        "StarVanguard.Runtime",
        "StarVanguard.UI",
        "Unity.InputSystem",
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

---

#### File 4: `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`
- **Location:** `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`
- **Platform Inclusions:** All platforms (`"includePlatforms": []`, `"excludePlatforms": []`)
- **Auto-Referenced:** `false`
- **Special Configurations:**
  - `"optionalUnityReferences": ["TestAssemblies"]` (integrates NUnit and Test Framework)
  - `"overrideReferences": true`
  - `"precompiledReferences": ["nunit.framework.dll"]`
  - `"defineConstraints": ["UNITY_INCLUDE_TESTS"]`
- **References:**
  - `StarVanguard.Runtime`
  - `StarVanguard.UI`
  - `UnityEngine.TestRunner`
  - `UnityEditor.TestRunner`
  - `UnityEngine.UI`
  - `Unity.TextMeshPro`
  - `Unity.InputSystem`
- **Exact File Content:**
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
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false,
    "optionalUnityReferences": [
        "TestAssemblies"
    ]
}
```

---

### 4.3 Recommended Execution Sequence for Implementer

1. **Step 1 (Decouple UIManager):** Apply C# event refactoring to `UIManager.cs` and `UIPanelRouter.cs`. Remove `using StarVanguard.UI;` and direct router calls from `UIManager.cs`.
2. **Step 2 (Create Runtime Asmdef):** Create `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`.
3. **Step 3 (Create UI Asmdef):** Create `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`.
4. **Step 4 (Create Editor Asmdef):** Create `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`.
5. **Step 5 (Create Tests Directory & Asmdef):** Create `Star Vanguard/Assets/Tests/` directory and `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`.
6. **Step 6 (Align Validator):** Update `Star Vanguard/Assets/Editor/StarVanguardValidator.cs:129` to check `BootstrapScene.unity` at index 0.

---

## 5. Verification Method

### 5.1 Verification Commands

1. **Gate G3 (Static Project Audit):**
   ```powershell
   python Tools/check_project.py
   ```
   *Success Condition:* Exit code 0, 0 FAIL. Check `asmdef` prints: `[ OK ] asmdef: 4 asmdef`.

2. **Gate G1 (Compilation Verification):**
   Run Unity compilation check via CLI/MCP:
   - Call `refresh_unity`
   - Read console logs with `read_console` (filter `Error`)
   *Success Condition:* 0 compilation errors. Assemblies `StarVanguard.Runtime.dll`, `StarVanguard.UI.dll`, `StarVanguard.Editor.dll`, and `StarVanguard.Tests.dll` are generated in `Library/ScriptAssemblies/`.

3. **Gate G2 (Regression / Validator):**
   ```powershell
   python -c "import os; print('Validating asmdefs exist: ' + str([os.path.exists(p) for p in [r'Star Vanguard\Assets\Scripts\StarVanguard.Runtime.asmdef', r'Star Vanguard\Assets\Scripts\UI\StarVanguard.UI.asmdef', r'Star Vanguard\Assets\Editor\StarVanguard.Editor.asmdef', r'Star Vanguard\Assets\Tests\StarVanguard.Tests.asmdef']]))"
   ```
   *Success Condition:* All four files return `True`.

4. **Circular Dependency Check:**
   Verify that `StarVanguard.Runtime.asmdef` contains zero references to `StarVanguard.UI`, `StarVanguard.Editor`, or `StarVanguard.Tests`:
   ```powershell
   python -c "import json; data = json.load(open(r'Star Vanguard\Assets\Scripts\StarVanguard.Runtime.asmdef')); print('Runtime references:', data.get('references')); assert not any('UI' in r or 'Editor' in r or 'Tests' in r for r in data.get('references', []))"
   ```

### 5.2 Invalidation Conditions
- If any script in `Assets/Scripts/` adds a `using StarVanguard.UI;` directive, compilation will immediately fail with `CS0246`.
- If `StarVanguard.Editor.asmdef` omits `"Unity.InputSystem"`, compilation will fail on `StarVanguardInputWiring.cs`.
- If `StarVanguard.UI.asmdef` omits `"StarVanguard.Runtime"`, compilation will fail on `MainMenuScreen.cs` and `SettingsScreen.cs`.
