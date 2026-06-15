#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIRECTORY_PATH="$(cd "$(dirname "$0")" && pwd)"
REPOSITORY_ROOT_PATH="$(cd "${SCRIPT_DIRECTORY_PATH}/.." && pwd)"
PROJECT_PATH="${REPOSITORY_ROOT_PATH}/unity-app"
TEST_RESULTS_DIRECTORY_PATH="${REPOSITORY_ROOT_PATH}/artifacts/unity-tests"
TEST_RESULTS_PATH="${TEST_RESULTS_DIRECTORY_PATH}/editmode-results.xml"
UNITY_EDITOR_PATH="${UNITY_EDITOR_PATH:-}"

if [[ -z "${UNITY_EDITOR_PATH}" ]]; then
  DEFAULT_MACOS_UNITY_EDITOR_PATH="/Applications/Unity/Hub/Editor/6000.4.1f1/Unity.app/Contents/MacOS/Unity"

  if [[ -x "${DEFAULT_MACOS_UNITY_EDITOR_PATH}" ]]; then
    UNITY_EDITOR_PATH="${DEFAULT_MACOS_UNITY_EDITOR_PATH}"
  fi
fi

if [[ -z "${UNITY_EDITOR_PATH}" ]]; then
  echo "UNITY_EDITOR_PATH is not set." >&2
  echo "Set UNITY_EDITOR_PATH to the Unity 6000.4.1f1 executable and try again." >&2
  exit 1
fi

if [[ ! -x "${UNITY_EDITOR_PATH}" ]]; then
  echo "Unity editor was not found at: ${UNITY_EDITOR_PATH}" >&2
  exit 1
fi

mkdir -p "${TEST_RESULTS_DIRECTORY_PATH}"
rm -f "${TEST_RESULTS_PATH}"

"${UNITY_EDITOR_PATH}" \
  -batchmode \
  -nographics \
  -projectPath "${PROJECT_PATH}" \
  -runTests \
  -testPlatform EditMode \
  -testResults "${TEST_RESULTS_PATH}"

python3 - "${TEST_RESULTS_PATH}" <<'PY'
from __future__ import annotations

import sys
import xml.etree.ElementTree as ElementTree
from pathlib import Path

results_path = Path(sys.argv[1])

if not results_path.is_file():
    raise SystemExit(f"Unity test result file was not created: {results_path}")

root = ElementTree.parse(results_path).getroot()
total_count = int(root.attrib.get("total", "0"))
failed_count = int(root.attrib.get("failed", "0"))
inconclusive_count = int(root.attrib.get("inconclusive", "0"))

if total_count <= 0:
    raise SystemExit("Unity EditMode test run completed without executing any tests.")

if failed_count > 0 or inconclusive_count > 0:
    raise SystemExit(
        f"Unity EditMode tests were not clean: total={total_count}, "
        f"failed={failed_count}, inconclusive={inconclusive_count}."
    )

print(f"Unity EditMode tests passed: total={total_count}.")
PY
