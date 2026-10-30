# Dispatch — Explorer M2-2: Assembly Definitions Specification (T-101b)

## Mission
Analyze and specify the exact configurations for the 4 assembly definition files:
1. Four `.asmdef` files:
   - `Star Vanguard/Assets/Scripts/StarVanguard.Runtime.asmdef`
   - `Star Vanguard/Assets/Scripts/UI/StarVanguard.UI.asmdef`
   - `Star Vanguard/Assets/Editor/StarVanguard.Editor.asmdef`
   - `Star Vanguard/Assets/Tests/StarVanguard.Tests.asmdef`
2. One-way dependency hierarchy:
   - `StarVanguard.Tests` -> `StarVanguard.UI`, `StarVanguard.Runtime`
   - `StarVanguard.Editor` -> `StarVanguard.Runtime`, `StarVanguard.UI`
   - `StarVanguard.UI` -> `StarVanguard.Runtime`
   - `StarVanguard.Runtime` -> zero internal dependencies (must NOT reference UI, Editor, or Tests).
3. Specify exact JSON content, referenced assemblies, and platform inclusions.
4. Verify against Unity package assemblies (`Unity.InputSystem`, `Unity.RenderPipelines.Universal.Runtime`, `Unity.TextMeshPro`, etc.).

## References
- `c:\Documents\UNITY 2026\.agents\teamwork\PROJECT.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_survey_2\handoff.md`
- `c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md`

## Output
Write report to `c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_2\handoff.md` and report back with `send_message`.


## 2026-10-30T08:36:41Z
Received dispatch from parent (e398e3de-abd2-415f-9284-f5920e4fe429):
You are Explorer M2-2 for Milestone 2 of Star Vanguard.
Your working directory is: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_2
Authoritative request: c:\Documents\UNITY 2026\.agents\teamwork\ORIGINAL_REQUEST.md
Your dispatch instructions are at: c:\Documents\UNITY 2026\.agents\teamwork\teamwork_explorer_m2_2\DISPATCH.md
Read the dispatch instructions, investigate the assembly definitions specification (T-101b), write your detailed report to handoff.md in your working directory, and notify me with send_message when finished.
