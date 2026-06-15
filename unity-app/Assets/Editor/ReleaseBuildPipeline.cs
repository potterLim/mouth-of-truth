using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using MouthOfTruth.Game.App;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace MouthOfTruth.Editor
{
    internal static class ReleaseBuildPipeline
    {
        private const string BURST_DEBUG_INFORMATION_DIRECTORY_SUFFIX = "_BurstDebugInformation_DoNotShip";
        private static readonly string[] DISTRIBUTION_FILE_NAMES_TO_REMOVE =
        {
            ".DS_Store",
            ".gitignore",
            ".gitkeep",
        };

        private static readonly string[] DISTRIBUTION_DIRECTORY_NAMES_TO_REMOVE =
        {
            "__pycache__",
        };

        public static ReleaseRuntimeRootPath GetValidatedProjectRuntimeRootPath()
        {
            ReleaseRuntimeRootPath runtimeRootPath = new ReleaseRuntimeRootPath(MouthOfTruthRuntimePaths.GetRuntimeRootPath());
            ReleaseRuntimeValidator.ValidateProjectRuntimeAssets(runtimeRootPath);
            return runtimeRootPath;
        }

        public static void PrepareMainSceneForBuild(string mainScenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(mainScenePath) == null)
            {
                throw new BuildFailedException($"Main scene is missing: {mainScenePath}. Run Mouth Of Truth/Build Main Scene before creating a release.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(mainScenePath, true),
            };
        }

        public static void GeneratePresentationBackgroundsIfGraphicsDeviceIsAvailable()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
            {
                GeneratePresentationBackgroundsEditor.Run();
            }
        }

        public static void RecreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                FileUtil.DeleteFileOrDirectory(directoryPath);
            }

            Directory.CreateDirectory(directoryPath);
        }

        public static void BuildPlayerOrThrow(string mainScenePath, string applicationPath, BuildTarget buildTarget, string platformName)
        {
            BuildReport buildReport = BuildPipeline.BuildPlayer(
                new[]
                {
                    mainScenePath,
                },
                applicationPath,
                buildTarget,
                BuildOptions.None);

            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{platformName} release build failed with result {buildReport.summary.result}.");
            }
        }

        public static void StageRuntimeSupport(string runtimeRootPath, string distributionRootPath, string bundledPythonRuntimeRootPath)
        {
            string distributionPythonEngineRootPath = Path.Combine(distributionRootPath, "python-engine");
            Directory.CreateDirectory(distributionPythonEngineRootPath);
            CopyPath(Path.Combine(runtimeRootPath, "python-engine", "src"), Path.Combine(distributionPythonEngineRootPath, "src"));
            CopyPath(Path.Combine(runtimeRootPath, "python-engine", "scripts"), Path.Combine(distributionPythonEngineRootPath, "scripts"));
            CopyPath(Path.Combine(runtimeRootPath, "python-engine", "models"), Path.Combine(distributionPythonEngineRootPath, "models"));
            CopyPath(Path.Combine(runtimeRootPath, "python-engine", "requirements.txt"), Path.Combine(distributionPythonEngineRootPath, "requirements.txt"));
            CopyPath(Path.Combine(runtimeRootPath, "python-engine", "environment.yml"), Path.Combine(distributionPythonEngineRootPath, "environment.yml"));
            ensureSessionWorkspaceDirectory(Path.Combine(distributionPythonEngineRootPath, "data", "session-workspace"));
            ensureBridgeDirectory(Path.Combine(distributionRootPath, "bridge"));

            if (Directory.Exists(bundledPythonRuntimeRootPath))
            {
                CopyPath(bundledPythonRuntimeRootPath, Path.Combine(distributionRootPath, "python-runtime"));
            }
        }

        public static void CopyPath(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                string destinationDirectoryPath = Path.GetDirectoryName(destinationPath);
                Directory.CreateDirectory(string.IsNullOrEmpty(destinationDirectoryPath) ? destinationPath : destinationDirectoryPath);
                FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
                return;
            }

            if (Directory.Exists(sourcePath) == false)
            {
                throw new BuildFailedException($"Release source path is missing: {sourcePath}");
            }

            if (Directory.Exists(destinationPath))
            {
                FileUtil.DeleteFileOrDirectory(destinationPath);
            }

            FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
        }

        public static void PruneDistributionArtifacts(string distributionRootPath)
        {
            foreach (string directoryPath in Directory.GetDirectories(distributionRootPath, "*", SearchOption.AllDirectories))
            {
                string directoryName = Path.GetFileName(directoryPath);

                if (shouldRemoveDistributionDirectory(directoryName))
                {
                    FileUtil.DeleteFileOrDirectory(directoryPath);
                }
            }

            foreach (string filePath in Directory.GetFiles(distributionRootPath, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(filePath);

                if (shouldRemoveDistributionFile(fileName) || fileName.EndsWith(".pyc", StringComparison.OrdinalIgnoreCase))
                {
                    FileUtil.DeleteFileOrDirectory(filePath);
                }
            }
        }

        public static void WriteZipArchive(string archivePath, string archiveRootName, string sourceRootPath)
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            string archiveDirectoryPath = Path.GetDirectoryName(archivePath);
            Directory.CreateDirectory(string.IsNullOrEmpty(archiveDirectoryPath) ? sourceRootPath : archiveDirectoryPath);

            using (FileStream archiveStream = File.Create(archivePath))
            using (ZipArchive archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (string filePath in Directory.GetFiles(sourceRootPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(sourceRootPath, filePath).Replace(Path.DirectorySeparatorChar, '/');
                    archive.CreateEntryFromFile(filePath, $"{archiveRootName}/{relativePath}", System.IO.Compression.CompressionLevel.Optimal);
                }
            }
        }

        public static void RunProcess(string fileName, string arguments, string workingDirectory)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                if (process.Start() == false)
                {
                    throw new BuildFailedException($"Failed to start process: {fileName}");
                }

                Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                string standardOutput = standardOutputTask.GetAwaiter().GetResult();
                string standardError = standardErrorTask.GetAwaiter().GetResult();

                if (process.ExitCode != 0)
                {
                    throw new BuildFailedException(
                        $"{fileName} failed with exit code {process.ExitCode}.\n"
                        + $"stdout:\n{standardOutput}\n"
                        + $"stderr:\n{standardError}");
                }
            }
        }

        private static void ensureBridgeDirectory(string bridgeDirectoryPath)
        {
            Directory.CreateDirectory(bridgeDirectoryPath);
            ensureGitKeepFile(bridgeDirectoryPath);
        }

        private static void ensureSessionWorkspaceDirectory(string sessionWorkspaceDirectoryPath)
        {
            Directory.CreateDirectory(sessionWorkspaceDirectoryPath);
            ensureGitKeepFile(sessionWorkspaceDirectoryPath);
        }

        private static void ensureGitKeepFile(string directoryPath)
        {
            string gitKeepFilePath = Path.Combine(directoryPath, ".gitkeep");

            if (File.Exists(gitKeepFilePath) == false)
            {
                File.WriteAllText(gitKeepFilePath, string.Empty);
            }
        }

        private static bool shouldRemoveDistributionDirectory(string directoryName)
        {
            if (directoryName.EndsWith(BURST_DEBUG_INFORMATION_DIRECTORY_SUFFIX, StringComparison.Ordinal))
            {
                return true;
            }

            foreach (string candidateName in DISTRIBUTION_DIRECTORY_NAMES_TO_REMOVE)
            {
                if (string.Equals(directoryName, candidateName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool shouldRemoveDistributionFile(string fileName)
        {
            foreach (string candidateName in DISTRIBUTION_FILE_NAMES_TO_REMOVE)
            {
                if (string.Equals(fileName, candidateName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
