using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Analysis
{
    public partial class PythonBridgeAnalysisClient : IAnswerAnalysisClient, IDisposable
    {
        private static readonly TimeSpan DEFAULT_ANALYSIS_TIMEOUT = TimeSpan.FromSeconds(15.0d);
        private static readonly TimeSpan WORKER_STARTUP_TIMEOUT = TimeSpan.FromSeconds(30.0d);
        private static readonly TimeSpan WORKER_SHUTDOWN_TIMEOUT = TimeSpan.FromSeconds(1.0d);
        private static readonly TimeSpan PROCESS_KILL_WAIT_TIMEOUT = TimeSpan.FromSeconds(1.0d);
        private static readonly TimeSpan PROCESS_OUTPUT_DRAIN_TIMEOUT = TimeSpan.FromSeconds(1.0d);

        private readonly object mWorkerReadyLock = new object();
        private readonly SemaphoreSlim mAnalysisSemaphore = new SemaphoreSlim(1, 1);
        private Process mWorkerProcess;
        private Task mWorkerReadyTask;
        private bool mIsWorkerReady;

        public PythonBridgeAnalysisClient()
        {
            tryStartWorkerProcess();
        }

        public Task WarmUpAsync(CancellationToken cancellationToken)
        {
            return ensureWorkerReadyAsync(cancellationToken);
        }

        public async Task<AnswerAnalysisResult> AnalyzeAsync(AnswerAnalysisRequest answerAnalysisRequest, CancellationToken cancellationToken)
        {
            if (answerAnalysisRequest == null)
            {
                throw new ArgumentNullException(nameof(answerAnalysisRequest));
            }

            await mAnalysisSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                Directory.CreateDirectory(PythonAnalysisBridgePaths.GetBridgeDirectoryPath());

                string requestId = Guid.NewGuid().ToString("N");
                BridgeAnalysisRequestFileData bridgeAnalysisRequestFileData =
                    new BridgeAnalysisRequestFileData
                    {
                        RequestID = requestId,
                        QuestionID = answerAnalysisRequest.QuestionDefinition.Id.Value,
                        QuestionText = answerAnalysisRequest.QuestionDefinition.Text.Value,
                        AnswerTranscript = answerAnalysisRequest.AnswerTranscript.Value,
                        AnswerAudioFilePath = buildRuntimeRelativePath(answerAnalysisRequest.AnswerAudioFilePath.Value),
                        FaceFramesDirectoryPath = buildRuntimeRelativePath(answerAnalysisRequest.FaceFramesDirectoryPath.Value),
                        FaceFrameCount = answerAnalysisRequest.FaceFrameCount.Value,
                        VoiceSegmentCount = answerAnalysisRequest.VoiceSegmentCount.Value,
                        RequestedAtUtc = DateTime.UtcNow.ToString("O"),
                    };

                string requestJson = UnityEngine.JsonUtility.ToJson(bridgeAnalysisRequestFileData, true);
                File.WriteAllText(PythonAnalysisBridgePaths.GetRequestFilePath(), requestJson);
                deletePreviousResultIfPresent();

                await runPythonAnalysisAsync(cancellationToken).ConfigureAwait(false);

                if (File.Exists(PythonAnalysisBridgePaths.GetResultFilePath()) == false)
                {
                    throw new FileNotFoundException("Python analysis finished without producing a result file.", PythonAnalysisBridgePaths.GetResultFilePath());
                }

                string resultJson = File.ReadAllText(PythonAnalysisBridgePaths.GetResultFilePath());
                BridgeAnalysisResultFileData bridgeAnalysisResultFileData = UnityEngine.JsonUtility.FromJson<BridgeAnalysisResultFileData>(resultJson);

                if (bridgeAnalysisResultFileData == null || bridgeAnalysisResultFileData.RequestID != requestId)
                {
                    throw new InvalidDataException("Python analysis returned an unexpected request identifier.");
                }

                IReadOnlyList<AnalysisReasonCode> reasonCodes = parseReasonCodes(bridgeAnalysisResultFileData.ReasonCodes);
                return new AnswerAnalysisResult(
                    parseVerdictKind(bridgeAnalysisResultFileData.Verdict),
                    new AnswerTranscript(bridgeAnalysisResultFileData.AnswerTranscript),
                    reasonCodes);
            }
            finally
            {
                mAnalysisSemaphore.Release();
            }
        }

        public void Dispose()
        {
            stopWorkerProcess();
            mAnalysisSemaphore.Dispose();
        }
    }
}
