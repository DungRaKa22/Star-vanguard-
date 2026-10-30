# BRIEFING — 2026-10-30T07:47:00Z

## Mission
Investigate Bootstrap scene duplicate removal plan (T-204c) and design an automated, safe Editor script/method to remove Main Camera and Global Light 2D from BootstrapScene.unity while preserving persistent managers.

## 🔒 My Identity
- Archetype: explorer
- Roles: investigation, synthesis
- Working directory: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m1_3
- Original parent: e398e3de-abd2-415f-9284-f5920e4fe429
- Milestone: Milestone 1 (T-204c)

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Do NOT edit scene YAML by hand or regex
- Only write metadata/reports in your own directory .agents/teamwork/teamwork_explorer_m1_3/
- Never place source code, tests, or data files in .agents/teamwork/
- Never name a file AGENTS.md or GEMINI.md

## Current Parent
- Conversation ID: e398e3de-abd2-415f-9284-f5920e4fe429
- Updated: not yet

## Investigation State
- **Explored paths**: `BootstrapScene.unity`, `BootstrapLoader.cs`, `StarVanguardBootstrapSetup.cs`, `StarVanguardCoopCleanup.cs`, `StarVanguardPlayModeSmoke.cs`, `StarVanguardValidator.cs`, `Tools/check_project.py`, `PROJECT.md`.
- **Key findings**:
  1. `BootstrapScene.unity` has exactly 4 root GameObjects: `Main Camera` (ID 519420028), `Global Light 2D` (ID 619394800), `Managers` (ID 760890375), and `BootstrapController` (ID 582285520).
  2. Redundancy root cause: `StarVanguardBootstrapSetup.cs` cloned `MainMenuScene` and omitted `"Main Camera"` and `"Global Light 2D"` from `k_NonBootstrapObjects`.
  3. Runtime impact: In Editor Play Mode entered from `MainMenuScene` or `GameplayScene`, `BootstrapLoader.EnsureBootstrapInEditor()` loads `BootstrapScene` additively. Because the active scene is not `BootstrapScene`, `BootstrapLoader.Start()` does not unload `BootstrapScene`. Thus, two `Main Camera`s (dual AudioListener warning), and two `Global Light 2D`s (200% additive URP 2D light blowout) coexist indefinitely.
  4. Persistent managers isolation: `GameManager`, `AudioManager`, `PoolManager`, `LeaderboardManager`, `SettingsManager` reside under `Managers`, unparent on `Awake()` and call `DontDestroyOnLoad()`. None of them reference `Main Camera` or `Global Light 2D`.
  5. `WaveDirector` is scene-bound to `GameplayScene` (`PersistAcrossScenes => false`) and is not present in `BootstrapScene`.
  6. Proposed solution: Implement `StarVanguardBootstrapCleanup.cs` (`RemoveDuplicatesAndSave`) with `[MenuItem("Tools/Star Vanguard/1c. Remove Duplicate Camera and Light from BootstrapScene and Save", priority = 3)]` and update `k_NonBootstrapObjects` in `StarVanguardBootstrapSetup.cs`.
- **Unexplored areas**: None. Problem boundary, cause, solution, and verification are fully mapped.

## Key Decisions Made
- Formulated `StarVanguardBootstrapCleanup.cs` pattern mirroring `StarVanguardCoopCleanup.cs` for automated, batchmode-safe, idempotent scene cleaning without YAML manual edits.
- Added recommendation to update `k_NonBootstrapObjects` in `StarVanguardBootstrapSetup.cs` to prevent future regression.

## Artifact Index
- DISPATCH.md — Received instructions and tasks
- BRIEFING.md — Persistent working memory
- progress.md — Liveness heartbeat
- handoff.md — 5-component handoff report for T-204c
