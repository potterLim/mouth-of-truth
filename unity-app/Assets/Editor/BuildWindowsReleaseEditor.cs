using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MouthOfTruth.Editor
{
    public static class BuildWindowsReleaseEditor
    {
        private const string MAIN_SCENE_PATH = "Assets/Scenes/Main.unity";
        private const string DISTRIBUTION_ROOT_RELATIVE_PATH = "dist/windows/MouthOfTruth";
        private const string DISTRIBUTION_ARCHIVE_RELATIVE_PATH = "dist/windows/MouthOfTruth-windows.zip";
        private const string APPLICATION_NAME = "MouthOfTruth.exe";
        private const string PYTHON_RUNTIME_ENVIRONMENT_VARIABLE_NAME = "MOUTH_OF_TRUTH_WINDOWS_PYTHON_RUNTIME_ROOT";
        private const string PACKAGE_PYTHON_RUNTIME_SCRIPT_RELATIVE_PATH = "python-engine/scripts/package_python_runtime.ps1";

        [MenuItem("Mouth Of Truth/Build Windows Release")]
        public static void Run()
        {
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64) == false)
            {
                throw new BuildFailedException("Windows Build Support is not installed for the current Unity editor.");
            }

            ReleaseBuildPipeline.PrepareMainSceneForBuild(MAIN_SCENE_PATH);
            ReleaseBuildPipeline.GeneratePresentationBackgroundsIfGraphicsDeviceIsAvailable();

            ReleaseRuntimeRootPath runtimeRootPath = ReleaseBuildPipeline.GetValidatedProjectRuntimeRootPath();
            ReleaseRuntimeRootPath distributionRootPath = new ReleaseRuntimeRootPath(Path.Combine(runtimeRootPath.Value, DISTRIBUTION_ROOT_RELATIVE_PATH));
            string applicationPath = Path.Combine(distributionRootPath.Value, APPLICATION_NAME);

            ReleaseBuildPipeline.RecreateDirectory(distributionRootPath.Value);
            ReleaseBuildPipeline.BuildPlayerOrThrow(MAIN_SCENE_PATH, applicationPath, BuildTarget.StandaloneWindows64, "Windows");

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
                    throw new BuildFailedException($"Configured Windows python runtime root does not exist: {configuredPythonRuntimeRootPath}");
                }

                return configuredPythonRuntimeRootPath;
            }

            string bundledPythonRuntimeRootPath = Path.Combine(runtimeRootPath, "python-runtime-windows");

            if (Directory.Exists(bundledPythonRuntimeRootPath))
            {
                return bundledPythonRuntimeRootPath;
            }

            packageBundledPythonRuntime(runtimeRootPath);

            if (Directory.Exists(bundledPythonRuntimeRootPath) == false)
            {
                throw new BuildFailedException("Bundled Windows python runtime could not be prepared for the release build.");
            }

            return bundledPythonRuntimeRootPath;
        }

        private static void packageBundledPythonRuntime(string runtimeRootPath)
        {
            string packageScriptPath = Path.Combine(runtimeRootPath, PACKAGE_PYTHON_RUNTIME_SCRIPT_RELATIVE_PATH);

            if (File.Exists(packageScriptPath) == false)
            {
                throw new BuildFailedException($"Windows python runtime packaging script is missing: {packageScriptPath}");
            }

            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                throw new BuildFailedException("Automatic Windows python runtime packaging must be run from a Windows Unity editor, " + "or you must set MOUTH_OF_TRUTH_WINDOWS_PYTHON_RUNTIME_ROOT to a prepared runtime folder.");
            }

            ReleaseBuildPipeline.RunProcess("powershell.exe", $"-ExecutionPolicy Bypass -File \"{packageScriptPath}\"", runtimeRootPath);
        }

        private static void writeLauncherScript(string distributionRootPath)
        {
            string launcherScriptPath = Path.Combine(distributionRootPath, "Run Mouth of Truth.bat");
            string launcherScriptContents =
                "@echo off\r\n"
                + "setlocal EnableExtensions\r\n"
                + "set \"SCRIPT_DIRECTORY_PATH=%~dp0\"\r\n"
                + "for %%I in (\"%SCRIPT_DIRECTORY_PATH%.\") do set \"MOUTH_OF_TRUTH_RUNTIME_ROOT=%%~fI\"\r\n"
                + "start \"Mouth of Truth\" \"%MOUTH_OF_TRUTH_RUNTIME_ROOT%\\MouthOfTruth.exe\" -screen-fullscreen 1\r\n";

            File.WriteAllText(launcherScriptPath, launcherScriptContents);
        }

        private static void writeDistributionArchive(string runtimeRootPath, string distributionRootPath)
        {
            string archivePath = Path.Combine(runtimeRootPath, DISTRIBUTION_ARCHIVE_RELATIVE_PATH);
            ReleaseBuildPipeline.WriteZipArchive(archivePath, "MouthOfTruth", distributionRootPath);
        }
    }
}
