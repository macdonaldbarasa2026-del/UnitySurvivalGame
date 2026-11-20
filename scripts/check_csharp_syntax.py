#!/usr/bin/env python3
"""Parses every C# file with tree-sitter and fails on syntax errors.

Requires: pip install tree-sitter tree-sitter-c-sharp
This is a syntax check only; a Unity compile is still required for semantic errors.
"""
import sys
from pathlib import Path

try:
    import tree_sitter_c_sharp as csharp
    from tree_sitter import Language, Parser
except ImportError as exc:
    print("tree-sitter C# parser is not installed: " + str(exc))
    sys.exit(2)

ROOT = Path(__file__).resolve().parent.parent
parser = Parser(Language(csharp.language()))
failures = []
files = sorted((ROOT / "Assets" / "Scripts").rglob("*.cs"))

for path in files:
    source = path.read_bytes()
    tree = parser.parse(source)
    stack = [tree.root_node]
    bad = []
    while stack:
        node = stack.pop()
        if node.type == "ERROR" or node.is_missing:
            line = node.start_point[0] + 1
            bad.append(f"line {line}: {node.type}")
        stack.extend(node.children)
    if bad:
        failures.append(f"{path.relative_to(ROOT).as_posix()}: " + ", ".join(bad[:6]))

print(f"Parsed {len(files)} C# files with tree-sitter.")
if failures:
    print("Syntax errors found:")
    for failure in failures:
        print("  - " + failure)
    sys.exit(1)
print("No C# syntax errors detected.")
