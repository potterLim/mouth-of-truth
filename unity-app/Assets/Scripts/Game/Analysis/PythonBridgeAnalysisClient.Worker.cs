using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Diagnostics;

namespace MouthOfTruth.Game.Analysis
{
    public partial class PythonBridgeAnalysisClient
    {
        private async Task runPythonAnalysisAsync(CancellationToken cancellationToken)
        {
            if (isWorkerAvailable())
            {
                try
                {
                    await runPythonWorkerAnalysisAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("Persistent Python analysis worker failed. Falling back to one-shot analysis.\n" + exception);
                    stopWorkerProcess();
                    deletePreviousResultIfPresent();
                }
            }

            await runPythonBridgeProcessAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task runPythonWorkerAnalysisAsync(CancellationToken cancellationToken)
        {
            await ensureWorkerReadyAsync(cancellationToken).ConfigureAwait(false);

            if (mIsWorkerReady == false)
            {
                throw new InvalidOperationException("Python analysis worker is not ready.");
            }

            BridgeWorkerCommandFileData bridgeWorkerCommandFileData = new BridgeWorkerCommandFileData
            {
                Command = "analyze",
                RequestFilePath = PythonAnalysisBridgePaths.GetRequestFilePath(),
                ResultFilePath = PythonAnalysisBridgePaths.GetResultFilePath(),
            };
            string workerCommandJson = UnityEngine.JsonUtility.ToJson(bridgeWorkerCommandFileData, false);
            await mWorkerProcess.StandardInput.WriteLineAsync(workerCommandJson).ConfigureAwait(false);
            await mWorkerProcess.StandardInput.FlushAsync().ConfigureAwait(false);

            BridgeWorkerResponseFileData response = await readWorkerResponseAsync(DEFAULT_TIMEOUT_MILLISECONDS, cancellationToken).ConfigureAwait(false);

            if (string.Equals(response.Status, "done", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new InvalidOperationException("Python analysis worker failed.\n" + response.ErrorMessage);
        }

        private Task ensureWorkerReadyAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (mIsWorkerReady || isWorkerAvailable() == false)
            {
                return Task.CompletedTask;
            }

            lock (mWorkerReadyLock)
            {
                if (mWorkerReadyTask == null || mWorkerReadyTask.IsCanceled || mWorkerReadyTask.IsFaulted)
                {
                    mWorkerReadyTask = readWorkerReadyAsync(cancellationToken);
                }

                return mWorkerReadyTask;
            }
        }

        private async Task readWorkerReadyAsync(CancellationToken cancellationToken)
        {
            BridgeWorkerResponseFileData readyResponse = await readWorkerResponseAsync(WORKER_STARTUP_TIMEOUT_MILLISECONDS, cancellationToken).ConfigureAwait(false);

            if (string.Equals(readyResponse.Status, "ready", StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new InvalidOperationException("Python analysis worker returned an unexpected startup status: " + readyResponse.Status);
            }

            mIsWorkerReady = true;
        }

        private async Task<BridgeWorkerResponseFileData> readWorkerResponseAsync(int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            if (isWorkerAvailable() == false)
            {
                throw new InvalidOperationException("Python analysis worker is not running.");
            }

            Task<string> readLineTask = mWorkerProcess.StandardOutput.ReadLineAsync();
            Task timeoutTask = Task.Delay(timeoutMilliseconds, cancellationToken);
            Task completedTask = await Task.WhenAny(readLineTask, timeoutTask).ConfigureAwait(false);

            if (completedTask != readLineTask)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException("Timed out while waiting for the Python analysis worker.");
            }

            string responseJson = await readLineTask.ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new EndOfStreamException("Python analysis worker stopped without returning a response.");
            }

            BridgeWorkerResponseFileData response = UnityEngine.JsonUtility.FromJson<BridgeWorkerResponseFileData>(responseJson);

            if (response == null || string.IsNullOrWhiteSpace(response.Status))
            {
                throw new InvalidDataException("Python analysis worker returned malformed JSON: " + responseJson);
            }

            return response;
        }

        private void tryStartWorkerProcess()
        {
            try
            {
                string bridgeWorkerLauncherScriptPath = PythonAnalysisBridgePaths.GetBridgeWorkerLauncherScriptPath();

                if (File.Exists(bridgeWorkerLauncherScriptPath) == false)
                {
                    return;
                }

                mWorkerProcess = buildPythonProcess(bridgeWorkerLauncherScriptPath, string.Empty);

                if (mWorkerProcess.Start() == false)
                {
                    mWorkerProcess = null;
                    return;
                }

                mWorkerProcess.ErrorDataReceived += (_, eventArguments) =>
                {
                    if (string.IsNullOrWhiteSpace(eventArguments.Data) == false)
                    {
                        MouthOfTruthLog.LogInfo(eventArguments.Data);
                    }
                };
                mWorkerProcess.BeginErrorReadLine();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Could not start the persistent Python analysis worker. One-shot analysis will be used.\n" + exception);
                stopWorkerProcess();
            }
        }

        private bool isWorkerAvailable()
        {
            return mWorkerProcess != null && mWorkerProcess.HasExited == false;
        }

        private void stopWorkerProcess()
        {
            if (mWorkerProcess == null)
            {
                return;
            }

            try
            {
                if (mWorkerProcess.HasExited == false)
                {
                    BridgeWorkerCommandFileData shutdownCommandFileData = new BridgeWorkerCommandFileData
                    {
                        Command = "shutdown",
                    };
                    string shutdownCommandJson = UnityEngine.JsonUtility.ToJson(shutdownCommandFileData, false);
                    mWorkerProcess.StandardInput.WriteLine(shutdownCommandJson);
                    mWorkerProcess.StandardInput.Flush();

                    if (mWorkerProcess.WaitForExit(WORKER_SHUTDOWN_TIMEOUT_MILLISECONDS) == false)
                    {
                        mWorkerProcess.Kill();
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    if (mWorkerProcess.HasExited == false)
                    {
                        mWorkerProcess.Kill();
                    }
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                mWorkerProcess.Dispose();
                mWorkerProcess = null;
                mWorkerReadyTask = null;
                mIsWorkerReady = false;
            }
        }
    }
}
