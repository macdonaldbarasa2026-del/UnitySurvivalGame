#!/usr/bin/env bash
# Type-check all gameplay scripts against minimal Unity API stubs using Mono's C# compiler.
# This is not a substitute for a Unity build, but it catches real compile errors offline.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="${TMPDIR:-/tmp}/unitysurvival-compilecheck.dll"

if ! command -v mcs >/dev/null 2>&1; then
  echo "mcs (Mono C# compiler) is required for the compile check." >&2
  exit 2
fi

SOURCES=("$ROOT/tests/UnityStubs.cs")
while IFS= read -r -d '' file; do
  SOURCES+=("$file")
done < <(find "$ROOT/Assets/Scripts" -name '*.cs' -print0)

mcs -define:UNITY_EDITOR -target:library -langversion:7 -out:"$OUT" "${SOURCES[@]}" 2>"${OUT}.log"

echo "Compiled ${#SOURCES[@]} files successfully against Unity API stubs."
if [ -s "${OUT}.log" ]; then
  echo "Compiler warnings:"
  cat "${OUT}.log"
fi
