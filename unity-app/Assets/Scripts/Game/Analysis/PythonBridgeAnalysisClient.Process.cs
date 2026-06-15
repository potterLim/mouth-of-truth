using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.App;
using UnityEngine;

namespace MouthOfTruth.Game.Analysis
{
    public partial class PythonBridgeAnalysisClient
    {
        private const string RUNTIME_ROOT_ENVIRONMENT_VARIABLE_NAME = "MOUTH_OF_TRUTH_RUNTIME_ROOT";

        private enum EPythonProcessLaunchMode
        {
            Direct,
            WindowsCommandShell,
        }

        private async Task runPythonBridgeProcessAsync(CancellationToken cancellationToken)
        {
            string pythonInterpreterPath = PythonAnalysisBridgePaths.GetPythonInterpreterPath();
            string bridgeLauncherScriptPath = PythonAnalysisBridgePaths.GetBridgeLauncherScriptPath();
            string requestFilePath = PythonAnalysisBridgePaths.GetRequestFilePath();
            string resultFilePath = PythonAnalysisBridgePaths.GetResultFilePath();

            if (string.IsNullOrWhiteSpace(pythonInterpreterPath) == false && File.Exists(pythonInterpreterPath) == false)
            {
                throw new FileNotFoundException("The configured Python interpreter was not found.", pythonInterpreterPath);
            }

            if (File.Exists(bridgeLauncherScriptPath) == false)
            {
                throw new FileNotFoundException("The Python bridge launcher script was not found.", bridgeLauncherScriptPath);
            }

            using (Process process = buildPythonProcess(bridgeLauncherScriptPath, buildBridgeLauncherArguments(requestFilePath, resultFilePath)))
            {
                if (process.Start() == false)
                {
                    throw new InvalidOperationException("Failed to start the Python analysis process.");
                }

                Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

                try
                {
                    await waitForProcessExitAsync(process, DEFAULT_TIMEOUT_MILLISECONDS, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    killProcessIfRunning(process);
                    throw;
                }

                string standardOutput = await standardOutputTask.ConfigureAwait(false);
                string standardError = await standardErrorTask.ConfigureAwait(false);

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("Python analysis failed.\n" + $"stdout:\n{standardOutput}\n" + $"stderr:\n{standardError}");
                }
            }
        }

        private static async Task waitForProcessExitAsync(Process process, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            Stopwatch timeoutStopwatch = Stopwatch.StartNew();

            while (process.HasExited == false)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (timeoutStopwatch.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    throw new TimeoutException("Timed out while waiting for the Python analysis process.");
                }

                await Task.Delay(50, cancellationToken).ConfigureAwait(false);
            }
        }

        private static void killProcessIfRunning(Process process)
        {
            try
            {
                if (process != null && process.HasExited == false)
                {
                    process.Kill();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private Process buildPythonProcess(string launcherScriptPath, string launcherArguments)
        {
            string pythonInterpreterPath = PythonAnalysisBridgePaths.GetPythonInterpreterPath();
            EPythonProcessLaunchMode launchMode = getPythonProcessLaunchMode();
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = launchMode == EPythonProcessLaunchMode.WindowsCommandShell ? "cmd.exe" : launcherScriptPath,
                    Arguments = buildCommandShellArguments(launchMode, launcherScriptPath, launcherArguments),
                    WorkingDirectory = PythonAnalysisBridgePaths.GetProjectRootPath(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            process.StartInfo.Environment["PYTHONPATH"] = PythonAnalysisBridgePaths.GetPythonModuleRootPath();
            process.StartInfo.Environment[RUNTIME_ROOT_ENVIRONMENT_VARIABLE_NAME] = MouthOfTruthRuntimePaths.GetRuntimeRootPath();

            if (string.IsNullOrWhiteSpace(pythonInterpreterPath) == false)
            {
                process.StartInfo.Environment["MOUTH_OF_TRUTH_PYTHON"] = pythonInterpreterPath;
            }

            return process;
        }

        private static EPythonProcessLaunchMode getPythonProcessLaunchMode()
        {
            return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer
                ? EPythonProcessLaunchMode.WindowsCommandShell
                : EPythonProcessLaunchMode.Direct;
        }

        private static string buildBridgeLauncherArguments(string requestFilePath, string resultFilePath)
        {
            return $"\"{requestFilePath}\" \"{resultFilePath}\"";
        }

        private static string buildCommandShellArguments(EPythonProcessLaunchMode launchMode, string launcherScriptPath, string launcherArguments)
        {
            if (launchMode == EPythonProcessLaunchMode.WindowsCommandShell)
            {
                return string.IsNullOrWhiteSpace(launcherArguments)
                    ? $"/c \"\"{launcherScriptPath}\"\""
                    : $"/c \"\"{launcherScriptPath}\" {launcherArguments}\"";
            }

            return launcherArguments;
        }
    }
}
