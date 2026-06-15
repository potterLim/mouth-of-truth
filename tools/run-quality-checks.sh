#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIRECTORY_PATH="$(cd "$(dirname "$0")" && pwd)"
REPOSITORY_ROOT_PATH="$(cd "${SCRIPT_DIRECTORY_PATH}/.." && pwd)"

cd "${REPOSITORY_ROOT_PATH}"

python3 -m compileall -q python-engine/src
PYTHONPATH=python-engine/src python3 -m unittest discover -s python-engine/tests

dotnet build unity-app/MouthOfTruth.Game.csproj /m:1
dotnet build unity-app/Assembly-CSharp-Editor.csproj /m:1
dotnet build unity-app/MouthOfTruth.Editor.Tests.csproj /m:1
dotnet format unity-app/MouthOfTruth.Game.csproj --verify-no-changes --severity error --no-restore
dotnet format unity-app/MouthOfTruth.Editor.Tests.csproj --verify-no-changes --severity error --no-restore

git diff --check

longLineReport="$(
  find unity-app/Assets/Scripts unity-app/Assets/Editor/Tests python-engine/src python-engine/tests \
    -type f \( -name '*.cs' -o -name '*.py' \) -print0 \
    | xargs -0 awk 'length($0) > 180 { print FILENAME ":" FNR ":" length($0) }'
)"

if [[ -n "${longLineReport}" ]]; then
  echo "Lines longer than 180 characters were found:" >&2
  echo "${longLineReport}" >&2
  exit 1
fi

carriageReturnReport="$(
  find . \
    -path './.git' -prune -o \
    -path './dist' -prune -o \
    -path './python-runtime' -prune -o \
    -path './python-runtime-windows' -prune -o \
    -path './essay_work' -prune -o \
    -path './unity-app/Library' -prune -o \
    -path './unity-app/Temp' -prune -o \
    -path './unity-app/Logs' -prune -o \
    -path './unity-app/Obj' -prune -o \
    -path './unity-app/Assets/ThirdParty' -prune -o \
    -type f \( \
      -path './unity-app/Assets/*' -o \
      -path './python-engine/src/*' -o \
      -path './python-engine/tests/*' -o \
      -path './bridge/*' -o \
      -path './tools/*' -o \
      -path './.github/*' -o \
      -name '.editorconfig' -o \
      -name '*.md' \
    \) -print0 \
    | xargs -0 grep -Il $'\r' || true
)"

if [[ -n "${carriageReturnReport}" ]]; then
  echo "CRLF line endings were found in first-party files:" >&2
  echo "${carriageReturnReport}" >&2
  exit 1
fi

if [[ "${MOUTH_OF_TRUTH_RUN_UNITY_TESTS:-0}" == "1" ]]; then
  tools/run-unity-editmode-tests.sh
fi

echo "Quality checks passed."
