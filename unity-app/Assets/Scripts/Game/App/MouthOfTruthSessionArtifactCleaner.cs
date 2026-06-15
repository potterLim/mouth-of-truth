using System;
using System.IO;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Voice;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    public static class MouthOfTruthSessionArtifactCleaner
    {
        public static void CleanAllSessionArtifacts()
        {
            cleanDirectoryContentsIfSafe(AnswerAudioWorkspacePaths.GetAudioDirectoryPath());
            cleanDirectoryContentsIfSafe(FaceFrameWorkspacePaths.GetFaceFramesDirectoryPath());
        }

        public static void CleanAnalysisArtifacts(AnswerAudioFilePath answerAudioFilePath, FaceFramesDirectoryPath faceFramesDirectoryPath)
        {
            deleteFileIfInsideDirectory(answerAudioFilePath.Value, AnswerAudioWorkspacePaths.GetAudioDirectoryPath());
            deleteDirectoryIfInsideDirectory(faceFramesDirectoryPath.Value, FaceFrameWorkspacePaths.GetFaceFramesDirectoryPath());
        }

        private static bool isPathInsideDirectory(string candidatePath, string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(candidatePath) || string.IsNullOrWhiteSpace(directoryPath))
            {
                return false;
            }

            string fullCandidatePath = Path.GetFullPath(candidatePath);
            string fullDirectoryPath = Path.GetFullPath(directoryPath);
            string fullDirectoryPathWithSeparator = fullDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            StringComparison pathComparison = getPathComparison();
            return fullCandidatePath.StartsWith(fullDirectoryPathWithSeparator, pathComparison);
        }

        private static void cleanDirectoryContentsIfSafe(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || Directory.Exists(directoryPath) == false)
            {
                return;
            }

            try
            {
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    deleteFileIfInsideDirectory(filePath, directoryPath);
                }

                foreach (string childDirectoryPath in Directory.GetDirectories(directoryPath))
                {
                    deleteDirectoryIfInsideDirectory(childDirectoryPath, directoryPath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Session artifact cleanup failed for directory '" + directoryPath + "'.\n" + exception);
            }
        }

        private static void deleteFileIfInsideDirectory(string filePath, string allowedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || File.Exists(filePath) == false)
            {
                return;
            }

            if (isPathInsideDirectory(filePath, allowedDirectoryPath) == false)
            {
                Debug.LogWarning("Skipped deleting session artifact outside the allowed directory: " + filePath);
                return;
            }

            try
            {
                File.Delete(filePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to delete session artifact file '" + filePath + "'.\n" + exception);
            }
        }

        private static void deleteDirectoryIfInsideDirectory(string directoryPath, string allowedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || Directory.Exists(directoryPath) == false)
            {
                return;
            }

            if (isPathInsideDirectory(directoryPath, allowedDirectoryPath) == false)
            {
                Debug.LogWarning("Skipped deleting session artifact directory outside the allowed directory: " + directoryPath);
                return;
            }

            try
            {
                Directory.Delete(directoryPath, recursive: true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to delete session artifact directory '" + directoryPath + "'.\n" + exception);
            }
        }

        private static StringComparison getPathComparison()
        {
            return Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        }
    }
}
