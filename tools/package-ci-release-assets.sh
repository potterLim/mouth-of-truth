#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIRECTORY_PATH="$(cd "$(dirname "$0")" && pwd)"
REPOSITORY_ROOT_PATH="$(cd "${SCRIPT_DIRECTORY_PATH}/.." && pwd)"
DISTRIBUTION_DIRECTORY_PATH="${REPOSITORY_ROOT_PATH}/dist/ci-assets"
ARCHIVE_FILE_PATH="${DISTRIBUTION_DIRECTORY_PATH}/mouth-of-truth-ci-assets.tar.gz"
CHECKSUM_FILE_PATH="${ARCHIVE_FILE_PATH}.sha256"

REQUIRED_ASSET_PATHS=(
  "python-engine/models/face/yolo26x_rafdb_best.pt"
  "python-engine/models/voice/best_wav2vec2_iemocap/config.json"
  "python-engine/models/voice/best_wav2vec2_iemocap/model.safetensors"
  "python-engine/models/voice/best_wav2vec2_iemocap/preprocessor_config.json"
  "unity-app/Assets/ThirdParty/Environment/DungeonModularPack"
  "unity-app/Assets/ThirdParty/Environment/DungeonModularPack.meta"
  "unity-app/Assets/ThirdParty/Environment/PersianCarpetUrp"
  "unity-app/Assets/ThirdParty/Environment/PersianCarpetUrp.meta"
)

OPTIONAL_ASSET_PATHS=(
  "python-engine/models/whisper/models--openai--whisper-tiny"
)

for relative_asset_path in "${REQUIRED_ASSET_PATHS[@]}"; do
  if [[ -e "${REPOSITORY_ROOT_PATH}/${relative_asset_path}" ]]; then
    continue
  fi

  echo "Required CI release asset is missing: ${relative_asset_path}" >&2
  exit 1
done

mkdir -p "${DISTRIBUTION_DIRECTORY_PATH}"
rm -f "${ARCHIVE_FILE_PATH}" "${CHECKSUM_FILE_PATH}"

archive_paths=("${REQUIRED_ASSET_PATHS[@]}")

for relative_asset_path in "${OPTIONAL_ASSET_PATHS[@]}"; do
  if [[ -e "${REPOSITORY_ROOT_PATH}/${relative_asset_path}" ]]; then
    archive_paths+=("${relative_asset_path}")
  fi
done

tar -czf "${ARCHIVE_FILE_PATH}" -C "${REPOSITORY_ROOT_PATH}" "${archive_paths[@]}"
shasum -a 256 "${ARCHIVE_FILE_PATH}" > "${CHECKSUM_FILE_PATH}"

echo "Packaged CI release assets:"
echo "${ARCHIVE_FILE_PATH}"
echo "${CHECKSUM_FILE_PATH}"
