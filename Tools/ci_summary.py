#!/usr/bin/env python3
"""
Render audit-report.json (do Tools/check_project.py sinh ra) thanh Markdown cho
GitHub Actions Step Summary.

Dung:
    python3 Tools/check_project.py --json audit-report.json
    python3 Tools/ci_summary.py audit-report.json >> "$GITHUB_STEP_SUMMARY"
"""

from __future__ import annotations

import json
import sys

ORDER = {"FAIL": 0, "WARN": 1, "INFO": 2}


def main() -> int:
    path = sys.argv[1] if len(sys.argv) > 1 else "audit-report.json"
    try:
        with open(path, encoding="utf-8") as handle:
            rows = json.load(handle)
    except FileNotFoundError:
        print(f"_Không tìm thấy {path}_")
        return 0

    rows.sort(key=lambda r: ORDER.get(r["level"], 3))
    counts: dict[str, int] = {}
    for row in rows:
        counts[row["level"]] = counts.get(row["level"], 0) + 1

    status = "❌ có lỗi cần sửa" if counts.get("FAIL") else "✅ sạch"
    print(
        f"**{status}** · FAIL {counts.get('FAIL', 0)} · "
        f"WARN {counts.get('WARN', 0)} · OK {counts.get('INFO', 0)}"
    )
    print()

    problems = [r for r in rows if r["level"] in ("FAIL", "WARN")]
    if problems:
        print("| Mức | Kiểm tra | Kết quả |")
        print("| :-- | :-- | :-- |")
        for row in problems:
            label = f"**{row['level']}**" if row["level"] == "FAIL" else row["level"]
            message = row["message"].replace("|", "\\|")
            print(f"| {label} | `{row['check']}` | {message} |")
        print()

    print("<details><summary>Chi tiết đầy đủ</summary>")
    print()
    print("```")
    for row in rows:
        print(f"[{row['level']}] {row['check']}: {row['message']}")
        for detail in row["detail"][:10]:
            print(f"    - {detail}")
    print("```")
    print()
    print("</details>")
    return 0


if __name__ == "__main__":
    sys.exit(main())
