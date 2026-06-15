using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MouthOfTruth.Editor
{
    public static class BuildMacReleaseEditor
    {
        private const string MAIN_SCENE_PATH = "Assets/Scenes/Main.unity";
        private const string DISTRIBUTION_ROOT_RELATIVE_PATH = "dist/macos/MouthOfTruth";
        private const string DISTRIBUTION_ARCHIVE_RELATIVE_PATH = "dist/macos/MouthOfTruth-macos.zip";
        private const string APPLICATION_NAME = "MouthOfTruth.app";
        private const string PYTHON_RUNTIME_ENVIRONMENT_VARIABLE_NAME = "MOUTH_OF_TRUTH_PYTHON_RUNTIME_ROOT";
        private const string PACKAGE_PYTHON_RUNTIME_SCRIPT_RELATIVE_PATH = "python-engine/scripts/package_python_runtime.sh";

        [MenuItem("Mouth Of Truth/Build Mac Release")]
        public static void Run()
        {
            ReleaseBuildPipeline.PrepareMainSceneForBuild(MAIN_SCENE_PATH);
            ReleaseBuildPipeline.GeneratePresentationBackgroundsIfGraphicsDeviceIsAvailable();

            ReleaseRuntimeRootPath runtimeRootPath = ReleaseBuildPipeline.GetValidatedProjectRuntimeRootPath();
            ReleaseRuntimeRootPath distributionRootPath = new ReleaseRuntimeRootPath(Path.Combine(runtimeRootPath.Value, DISTRIBUTION_ROOT_RELATIVE_PATH));
            string applicationPath = Path.Combine(distributionRootPath.Value, APPLICATION_NAME);

            ReleaseBuildPipeline.RecreateDirectory(distributionRootPath.Value);
            ReleaseBuildPipeline.BuildPlayerOrThrow(MAIN_SCENE_PATH, applicationPath, BuildTarget.StandaloneOSX, "Mac");

            string bundledPythonRuntimeRootPath = resolveBundledPythonRuntimeRootPath(runtimeRootPath.Value, Environment.GetEnvironmentVariable(PYTHON_RUNTIME_ENVIRONMENT_VARIABLE_NAME));
            ReleaseBuildPipeline.StageRuntimeSupport(runtimeRootPath.Value, distributionRootPath.Value, bundledPythonRuntimeRootPath);
            ReleaseRuntimeValidator.ValidateDistributionRuntimeAssets(distributionRootPath);
            ReleaseBuildPipeline.PruneDistributionArtifacts(distributionRootPath.Value);
            writeLauncherScript(distributionRootPath.Value);
            writeDistributionArchive(runtimeRootPath.Value, distributionRootPath.Value);
            AssetDatabase.Refresh();
        }

        private static string resolveBundledPythonRuntimeRootPath(string runtimeRootPath, string configuredPythonRuntimeRootPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPythonRuntimeRootPath) == false)
            {
                if (Directory.Exists(configuredPythonRuntimeRootPath) == false)
                {
                    throw new BuildFailedException($"Configured python runtime root does not exist: {configuredPythonRuntimeRootPath}");
                }

                return configuredPythonRuntimeRootPath;
            }

            string bundledPythonRuntimeRootPath = Path.Combine(runtimeRootPath, "python-runtime");

            if (Directory.Exists(bundledPythonRuntimeRootPath))
            {
                return bundledPythonRuntimeRootPath;
            }

            packageBundledPythonRuntime(runtimeRootPath);

            if (Directory.Exists(bundledPythonRuntimeRootPath) == false)
            {
                throw new BuildFailedException("Bundled python runtime could not be prepared for the release build.");
            }

            return bundledPythonRuntimeRootPath;
        }

        private static void packageBundledPythonRuntime(string runtimeRootPath)
        {
            string packageScriptPath = Path.Combine(runtimeRootPath, PACKAGE_PYTHON_RUNTIME_SCRIPT_RELATIVE_PATH);

            if (File.Exists(packageScriptPath) == false)
            {
                throw new BuildFailedException($"Python runtime packaging script is missing: {packageScriptPath}");
            }

            ReleaseBuildPipeline.RunProcess("/bin/zsh", $"\"{packageScriptPath}\"", runtimeRootPath);
        }

        private static void writeLauncherScript(string distributionRootPath)
        {
            string launcherScriptPath = Path.Combine(distributionRootPath, "Run Mouth of Truth.command");
            string launcherScriptContents =
                "#!/usr/bin/env zsh\n"
                + "set -euo pipefail\n"
                + "SCRIPT_DIRECTORY_PATH=\"$(cd \"$(dirname \"$0\")\" && pwd)\"\n"
                + "export MOUTH_OF_TRUTH_RUNTIME_ROOT=\"${SCRIPT_DIRECTORY_PATH}\"\n"
                + "open \"${SCRIPT_DIRECTORY_PATH}/MouthOfTruth.app\" --args -screen-fullscreen 1\n";

            File.WriteAllText(launcherScriptPath, launcherScriptContents);
            ReleaseBuildPipeline.RunProcess("/bin/chmod", $"+x \"{launcherScriptPath}\"", distributionRootPath);
        }

        private static void writeDistributionArchive(string runtimeRootPath, string distributionRootPath)
        {
            string archivePath = Path.Combine(runtimeRootPath, DISTRIBUTION_ARCHIVE_RELATIVE_PATH);

            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            string archiveDirectoryPath = Path.GetDirectoryName(archivePath);
            Directory.CreateDirectory(string.IsNullOrEmpty(archiveDirectoryPath) ? runtimeRootPath : archiveDirectoryPath);
            ReleaseBuildPipeline.RunProcess("/usr/bin/ditto", $"-c -k --norsrc --noextattr --noqtn --noacl --keepParent \"{distributionRootPath}\" \"{archivePath}\"", runtimeRootPath);
        }
    }
}
