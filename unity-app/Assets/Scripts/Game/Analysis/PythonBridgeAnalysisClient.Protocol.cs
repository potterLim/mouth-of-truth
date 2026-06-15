using System;
using System.Collections.Generic;
using System.IO;
using MouthOfTruth.Game.App;

namespace MouthOfTruth.Game.Analysis
{
    public partial class PythonBridgeAnalysisClient
    {
        private static EVerdictKind parseVerdictKind(string verdictText)
        {
            if (string.Equals(verdictText, "TRUE", StringComparison.OrdinalIgnoreCase))
            {
                return EVerdictKind.True;
            }

            if (string.Equals(verdictText, "FALSE", StringComparison.OrdinalIgnoreCase))
            {
                return EVerdictKind.False;
            }

            return EVerdictKind.Uncertain;
        }

        private static IReadOnlyList<AnalysisReasonCode> parseReasonCodes(string[] reasonCodeTexts)
        {
            if (reasonCodeTexts == null || reasonCodeTexts.Length == 0)
            {
                return Array.Empty<AnalysisReasonCode>();
            }

            List<AnalysisReasonCode> reasonCodes = new List<AnalysisReasonCode>();

            foreach (string reasonCodeText in reasonCodeTexts)
            {
                if (string.IsNullOrWhiteSpace(reasonCodeText))
                {
                    continue;
                }

                reasonCodes.Add(new AnalysisReasonCode(reasonCodeText));
            }

            return reasonCodes;
        }

        private static void deletePreviousResultIfPresent()
        {
            string resultFilePath = PythonAnalysisBridgePaths.GetResultFilePath();

            if (File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }
        }

        private static string buildRuntimeRelativePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
            {
                return string.Empty;
            }

            string normalizedPath = Path.GetFullPath(originalPath);
            string runtimeRootPath = Path.GetFullPath(MouthOfTruthRuntimePaths.GetRuntimeRootPath());
            string runtimeRootWithSeparator = runtimeRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (normalizedPath.StartsWith(runtimeRootWithSeparator, StringComparison.OrdinalIgnoreCase) || string.Equals(normalizedPath, runtimeRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetRelativePath(runtimeRootPath, normalizedPath)
                    .Replace(Path.DirectorySeparatorChar, '/');
            }

            return normalizedPath;
        }

        [Serializable]
        private sealed class BridgeWorkerCommandFileData
        {
            // JsonUtility maps fields by the Python worker protocol keys.
            public string Command = string.Empty;
            public string RequestFilePath = string.Empty;
            public string ResultFilePath = string.Empty;
        }

        [Serializable]
        private sealed class BridgeWorkerResponseFileData
        {
            // JsonUtility maps fields by the Python worker protocol keys.
            public string Status = string.Empty;
            public string ErrorMessage = string.Empty;
        }
    }
}
