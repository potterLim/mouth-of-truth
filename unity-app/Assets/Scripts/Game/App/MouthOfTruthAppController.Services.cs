using System;
using System.IO;
using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Diagnostics;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Input.Keyboard;
using MouthOfTruth.Game.Input.Leap;
using MouthOfTruth.Game.Narration;
using MouthOfTruth.Game.Voice;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    public partial class MouthOfTruthAppController
    {
        private IQuestionNarrationService createNarrationService()
        {
            IQuestionNarrationService fallbackNarrationService = Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
                ? new MacOsQuestionNarrationService()
                : new SilentQuestionNarrationService();

            QuestionAudioDirectoryPath questionAudioDirectoryPath = new QuestionAudioDirectoryPath(Path.Combine(Application.streamingAssetsPath, "audio", "questions"));

            return new PrerecordedQuestionNarrationService(questionAudioDirectoryPath, fallbackNarrationService);
        }

        private IAnswerAnalysisClient createAnalysisClient()
        {
            if (mAnswerAnalysisRuntimeConfiguration == null)
            {
                mAnswerAnalysisRuntimeConfiguration = AnswerAnalysisRuntimeConfiguration.LoadFromEnvironment();
            }

            switch (mAnswerAnalysisRuntimeConfiguration.AnalysisMode)
            {
                case EAnswerAnalysisMode.Python:
                    if (isPythonBridgeAvailable() == false)
                    {
                        throw new InvalidDataException("Python analysis mode was requested, but the Python bridge runtime was not found.");
                    }

                    return new PythonBridgeAnalysisClient();

                case EAnswerAnalysisMode.Deterministic:
                    return new DeterministicAnswerAnalysisClient();

                default:
                    return isPythonBridgeAvailable()
                        ? new PythonBridgeAnalysisClient()
                        : new DeterministicAnswerAnalysisClient();
            }
        }

        private async Task warmUpAnalysisClientAsync()
        {
            try
            {
                System.Diagnostics.Stopwatch warmUpStopwatch = System.Diagnostics.Stopwatch.StartNew();
                await mAnswerAnalysisClient.WarmUpAsync(mLifecycleCancellationTokenSource.Token);
                warmUpStopwatch.Stop();
                MouthOfTruthLog.logInfo($"Answer analysis engine warmed up in {warmUpStopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Answer analysis engine warm-up did not finish before gameplay. The first verdict may wait for startup.\n" + exception);
            }
        }

        private IHandInteractionInputAdapter createHandInteractionInputAdapter()
        {
            return new CompositeHandInteractionInputAdapter(new LeapHandInputAdapter(), new KeyboardHandInputAdapter());
        }

        private IAnswerCaptureInputAdapter createAnswerCaptureInputAdapter()
        {
            MicrophoneAnswerInputAdapter microphoneAnswerInputAdapter = new MicrophoneAnswerInputAdapter();

            if (microphoneAnswerInputAdapter.HasAvailableDevice())
            {
                return microphoneAnswerInputAdapter;
            }

            return new KeyboardTranscriptAnswerInputAdapter(mGameView);
        }

        private void prepareAnswerAudioSession()
        {
            MicrophoneAnswerInputAdapter microphoneAnswerInputAdapter = mAnswerCaptureInputAdapter as MicrophoneAnswerInputAdapter;

            if (microphoneAnswerInputAdapter == null)
            {
                return;
            }

            try
            {
                microphoneAnswerInputAdapter.PrepareAudioSession();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Microphone audio session prewarm failed. Capture will retry when answering.\n" + exception);
            }
        }

        private IFaceCaptureInputAdapter createFaceCaptureInputAdapter()
        {
            return new WebcamFaceCaptureInputAdapter();
        }

        private static bool isPythonBridgeAvailable()
        {
            return File.Exists(PythonAnalysisBridgePaths.GetBridgeLauncherScriptPath())
                && Directory.Exists(PythonAnalysisBridgePaths.GetPythonModuleRootPath());
        }

        private void beginOrResumeAnswerCapture(EAnswerCaptureStartMode answerCaptureStartMode)
        {
            if (answerCaptureStartMode == EAnswerCaptureStartMode.Begin)
            {
                mAnswerCaptureInputAdapter.BeginCollection();
                return;
            }

            mAnswerCaptureInputAdapter.ResumeCollection();
        }

        private void beginOrResumeFaceCapture(QuestionId questionId, EAnswerCaptureStartMode answerCaptureStartMode)
        {
            if (answerCaptureStartMode == EAnswerCaptureStartMode.Begin)
            {
                mFaceCaptureInputAdapter.BeginCollection(questionId);
                return;
            }

            mFaceCaptureInputAdapter.ResumeCollection();
        }

        private void applyTranscriptUpdate(AnswerTranscript answerTranscript)
        {
            AnswerTranscript normalizedAnswerTranscript = answerTranscript;

            if (normalizedAnswerTranscript.Equals(mLastObservedTranscript))
            {
                return;
            }

            mLastObservedTranscript = normalizedAnswerTranscript;
            mGameStateMachine.UpdateAnswerTranscript(normalizedAnswerTranscript);
            mGameView.SetAnswerTranscriptText(normalizedAnswerTranscript);
        }

        private async Task requestCaptureAuthorizationsAsync()
        {
            await requestAuthorizationIfNeededAsync(UserAuthorization.Microphone);
            await requestAuthorizationIfNeededAsync(UserAuthorization.WebCam);
        }

        private async Task requestAuthorizationIfNeededAsync(UserAuthorization userAuthorization)
        {
            if (Application.HasUserAuthorization(userAuthorization))
            {
                return;
            }

            AsyncOperation requestOperation = Application.RequestUserAuthorization(userAuthorization);

            while (requestOperation.isDone == false)
            {
                await Task.Yield();
            }
        }
    }
}
