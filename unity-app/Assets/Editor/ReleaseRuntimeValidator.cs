using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor.Build;

namespace MouthOfTruth.Editor
{
    internal static class ReleaseRuntimeValidator
    {
        private static readonly RequiredReleaseFile[] REQUIRED_RELEASE_FILES =
        {
            new RequiredReleaseFile("python-engine/requirements.txt", string.Empty),
            new RequiredReleaseFile("python-engine/environment.yml", string.Empty),
            new RequiredReleaseFile("python-engine/src/mouth_of_truth/runners/bridge_analysis_runner.py", string.Empty),
            new RequiredReleaseFile("python-engine/src/mouth_of_truth/runners/bridge_analysis_worker.py", string.Empty),
            new RequiredReleaseFile("python-engine/scripts/run_bridge_analysis.sh", string.Empty),
            new RequiredReleaseFile("python-engine/scripts/run_bridge_analysis.bat", string.Empty),
            new RequiredReleaseFile("python-engine/scripts/run_bridge_analysis_worker.sh", string.Empty),
            new RequiredReleaseFile("python-engine/scripts/run_bridge_analysis_worker.bat", string.Empty),
            new RequiredReleaseFile("python-engine/models/face/yolo26x_rafdb_best.pt", "48e47f019b8214b4c6869af87a3ab8a23fa34a0e891a6d4caf7fd25f7492e35a"),
            new RequiredReleaseFile("python-engine/models/voice/best_wav2vec2_iemocap/config.json", "e80a86c0d4e859cd46cc852d4f5864f3de78be8e64f47c1f79b31b687099f5be"),
            new RequiredReleaseFile("python-engine/models/voice/best_wav2vec2_iemocap/model.safetensors", "699c55de39fddb538eee49a24afc1008a20bb78918b7a50429b63b59dc62f5c3"),
            new RequiredReleaseFile("python-engine/models/voice/best_wav2vec2_iemocap/preprocessor_config.json", "8cdfd65ff4115423185a1512bdae100e2e0cd744f5b322417429944aaafd0827"),
        };
        private static readonly string[] REQUIRED_PROJECT_STREAMING_ASSET_FILES =
        {
            "unity-app/Assets/StreamingAssets/questions/question_pool.json",
            "unity-app/Assets/StreamingAssets/tutorial/combined_sequence.json",
            "unity-app/Assets/StreamingAssets/art/backgrounds/title_background_stone_wall.jpeg",
            "unity-app/Assets/StreamingAssets/art/backgrounds/stage_card_selection_generated.png",
            "unity-app/Assets/StreamingAssets/art/backgrounds/stage_mouth_chamber_generated.png",
            "unity-app/Assets/StreamingAssets/art/cards/question_card_back.png",
            "unity-app/Assets/StreamingAssets/art/cards/question_card_front.png",
            "unity-app/Assets/StreamingAssets/art/effects/card_selection_glow.png",
            "unity-app/Assets/StreamingAssets/art/effects/card_selection_progress_fill.png",
            "unity-app/Assets/StreamingAssets/art/environment/floor_red_carpet_runner.png",
            "unity-app/Assets/StreamingAssets/art/input/hand_pointer_cursor.png",
            "unity-app/Assets/StreamingAssets/art/input/leap_motion_device.png",
            "unity-app/Assets/StreamingAssets/art/input/ritual_hand_insert.png",
            "unity-app/Assets/StreamingAssets/art/mouth/truth_mouth_face.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_end_game.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_exit_icon.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_frame_primary.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_start_game.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_try_again.png",
            "unity-app/Assets/StreamingAssets/art/ui/logo_title_main.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_question.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_result.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_status.png",
            "unity-app/Assets/StreamingAssets/art/ui/title_vignette.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_false.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_true.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_uncertain.png",
            "unity-app/Assets/StreamingAssets/audio/ambience/title_temple_ambience_loop.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_hover.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_reveal.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_select.wav",
            "unity-app/Assets/StreamingAssets/audio/interaction/hand_insert.wav",
            "unity-app/Assets/StreamingAssets/audio/interaction/hand_prompt.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0001.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0002.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0003.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0004.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0005.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0006.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0007.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0008.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0009.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0010.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0011.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0012.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_false.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_true.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_uncertain.wav",
            "unity-app/Assets/StreamingAssets/audio/ui/button_confirm.wav",
        };

        public static void ValidateProjectRuntimeAssets(string runtimeRootPath)
        {
            validateReleaseFiles(runtimeRootPath, "project runtime");
            validateProjectStreamingAssets(runtimeRootPath);
        }

        public static void ValidateDistributionRuntimeAssets(string distributionRootPath)
        {
            validateReleaseFiles(distributionRootPath, "distribution runtime");
        }

        private static void validateReleaseFiles(string releaseRootPath, string releaseRootDescription)
        {
            if (string.IsNullOrWhiteSpace(releaseRootPath))
            {
                throw new BuildFailedException("Release runtime root path is empty.");
            }

            assertDirectoryExists(Path.Combine(releaseRootPath, "python-engine", "src"), releaseRootDescription);
            assertDirectoryExists(Path.Combine(releaseRootPath, "python-engine", "scripts"), releaseRootDescription);
            assertDirectoryExists(Path.Combine(releaseRootPath, "python-engine", "models"), releaseRootDescription);

            foreach (RequiredReleaseFile requiredReleaseFile in REQUIRED_RELEASE_FILES)
            {
                validateRequiredFile(releaseRootPath, releaseRootDescription, requiredReleaseFile);
            }
        }

        private static void validateProjectStreamingAssets(string runtimeRootPath)
        {
            foreach (string requiredProjectStreamingAssetFile in REQUIRED_PROJECT_STREAMING_ASSET_FILES)
            {
                string filePath = Path.Combine(runtimeRootPath, requiredProjectStreamingAssetFile.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                {
                    continue;
                }

                throw new BuildFailedException("Required project StreamingAssets file is missing: " + filePath);
            }
        }

        private static void assertDirectoryExists(string directoryPath, string releaseRootDescription)
        {
            if (Directory.Exists(directoryPath))
            {
                return;
            }

            throw new BuildFailedException($"Required {releaseRootDescription} directory is missing: {directoryPath}");
        }

        private static void validateRequiredFile(string releaseRootPath, string releaseRootDescription, RequiredReleaseFile requiredReleaseFile)
        {
            string filePath = Path.Combine(releaseRootPath, requiredReleaseFile.RelativePath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath) == false)
            {
                throw new BuildFailedException($"Required {releaseRootDescription} file is missing: {filePath}");
            }

            if (requiredReleaseFile.HasSha256Hash == false)
            {
                return;
            }

            string actualSha256Hash = calculateSha256Hash(filePath);

            if (string.Equals(actualSha256Hash, requiredReleaseFile.Sha256Hash, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new BuildFailedException(
                $"Required {releaseRootDescription} file has an unexpected SHA-256 hash: {filePath}\n"
                + $"expected: {requiredReleaseFile.Sha256Hash}\n"
                + $"actual:   {actualSha256Hash}");
        }

        private static string calculateSha256Hash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(fileStream);
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length * 2);

                foreach (byte hashByte in hashBytes)
                {
                    hashBuilder.Append(hashByte.ToString("x2"));
                }

                return hashBuilder.ToString();
            }
        }

        private sealed class RequiredReleaseFile
        {
            public string RelativePath
            {
                get;
            }

            public string Sha256Hash
            {
                get;
            }

            public bool HasSha256Hash
            {
                get;
            }

            public RequiredReleaseFile(string relativePath, string sha256Hash)
            {
                RelativePath = relativePath;
                Sha256Hash = sha256Hash;
                HasSha256Hash = string.IsNullOrWhiteSpace(sha256Hash) == false;
            }
        }
    }
}
