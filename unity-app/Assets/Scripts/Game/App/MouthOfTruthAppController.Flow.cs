using System;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Diagnostics;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Presentation.Runtime;
using MouthOfTruth.Game.Session;
using MouthOfTruth.Game.Voice;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    public partial class MouthOfTruthAppController
    {
        private static readonly SecondsDuration MINIMUM_ANALYSIS_PRESENTATION_DURATION = new SecondsDuration(2.5f);

        private async Task startGameAsync()
        {
            mIsTransitionBusy = true;
            try
            {
                bool shouldShowFirstRunTutorial = mHasShownFirstRunTutorial == false;
                mHasShownFirstRunTutorial = true;
                mGameStateMachine.StartGame();
                resetInteractionSelectionState();

                if (shouldShowFirstRunTutorial)
                {
                    await mGameView.PlayFirstRunTutorialAsync();
                }

                await playCardSelectionPresentationAsync();
                mGameStateMachine.MarkCardPresentationCompleted();
            }
            finally
            {
                mIsTransitionBusy = false;
            }
        }

        private async Task restartGameAsync()
        {
            mIsTransitionBusy = true;
            try
            {
                mGameStateMachine.TryAgain();
                resetAnswerTracking();
                resetInteractionSelectionState();
                await playCardSelectionPresentationAsync();
                beginBottomCenterPointerSettle();
                mGameStateMachine.MarkCardPresentationCompleted();
            }
            finally
            {
                mIsTransitionBusy = false;
            }
        }

        private async Task playCardSelectionPresentationAsync()
        {
            await mGameView.PlayTempleApproachToCardSelectionAsync();
            mGameView.ShowCardSelection(mGameStateMachine.CreateSnapshot().CurrentRoundSelection);
            await mGameView.PlayCardSelectionEntranceAsync();
        }

        private async Task revealQuestionAsync(EQuestionCardSlot selectedQuestionCardSlot, QuestionDefinition selectedQuestionDefinition)
        {
            mIsTransitionBusy = true;
            try
            {
                mGameView.UpdatePointerVisual(EPointerVisualState.Hidden, null);
                mGameView.UpdateActionButtonHoverVisual(null, NormalizedProgress.Zero);
                await mGameView.PlayQuestionRevealAsync(selectedQuestionCardSlot, selectedQuestionDefinition, () => mQuestionNarrationService.SpeakQuestionAsync(selectedQuestionDefinition, mLifecycleCancellationTokenSource.Token));
                await mGameView.PrepareTempleGameplayBackdropAsync();
                mGameStateMachine.MarkQuestionRevealCompleted();
                mGameStateMachine.MarkQuestionNarrationCompleted();
                mGameView.ShowAwaitingHandInsertion();
                beginBottomCenterPointerSettle(mGameView.HandPromptPanelHoldDuration);
                mGameView.SetAnswerTranscriptInputMode(mAnswerCaptureInputAdapter.TranscriptInputMode);
                resetAnswerTracking();
            }
            finally
            {
                mIsTransitionBusy = false;
            }
        }

        private async Task insertHandAsync()
        {
            mIsTransitionBusy = true;
            try
            {
                resetPointerPresentationRebase();
                EAnswerCaptureStartMode answerCaptureStartMode = mGameStateMachine.CurrentState == EGameFlowState.AnswerPaused
                    ? EAnswerCaptureStartMode.Resume
                    : EAnswerCaptureStartMode.Begin;

                if (mGameStateMachine.CurrentState == EGameFlowState.AwaitingHandInsertion)
                {
                    mGameStateMachine.NotifyHandReachedFrontAnchor();
                }

                await mGameView.AnimateHandInsertionAsync();
                mGameStateMachine.NotifyHandReachedInnerAnchor();
                GameSessionSnapshot snapshot = mGameStateMachine.CreateSnapshot();
                try
                {
                    beginOrResumeAnswerCapture(answerCaptureStartMode);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Falling back to keyboard answer entry because microphone capture failed.\n" + exception);
                    mAnswerCaptureInputAdapter = new KeyboardTranscriptAnswerInputAdapter(mGameView);
                    mGameView.SetAnswerTranscriptPlaceholder(mAnswerCaptureInputAdapter.TranscriptPlaceholderText);
                    mAnswerCaptureInputAdapter.Reset();
                    beginOrResumeAnswerCapture(EAnswerCaptureStartMode.Begin);
                }
                beginOrResumeFaceCapture(snapshot.SelectedQuestionDefinitionOrNull == null ? QuestionId.Fallback : snapshot.SelectedQuestionDefinitionOrNull.Id, answerCaptureStartMode);

                mGameView.ShowAnswering();
                mGameView.SetAnswerTranscriptInputMode(mAnswerCaptureInputAdapter.TranscriptInputMode);
                mLastObservedHandAnchorState = EHandAnchorState.AtInnerAnchor;
            }
            finally
            {
                mIsTransitionBusy = false;
            }
        }

        private async Task analyzeAnswerAsync()
        {
            mIsTransitionBusy = true;
            AnswerCaptureResult answerCaptureResult = null;
            FaceCaptureResult faceCaptureResult = null;
            try
            {
                mGameView.ShowAnalyzing();
                float analysisPresentationStartedAtSeconds = Time.unscaledTime + mGameView.AnalysisFocusRampDuration.Value;
                GameSessionSnapshot snapshot = mGameStateMachine.CreateSnapshot();
                System.Diagnostics.Stopwatch captureStopwatch = System.Diagnostics.Stopwatch.StartNew();
                Task<AnswerCaptureResult> answerCaptureTask = mAnswerCaptureInputAdapter.CompleteCollectionAsync(snapshot.SelectedQuestionDefinitionOrNull == null ? QuestionId.Fallback : snapshot.SelectedQuestionDefinitionOrNull.Id, mLifecycleCancellationTokenSource.Token);
                Task<FaceCaptureResult> faceCaptureTask = mFaceCaptureInputAdapter.CompleteCollectionAsync(mLifecycleCancellationTokenSource.Token);
                await Task.WhenAll(answerCaptureTask, faceCaptureTask);
                captureStopwatch.Stop();
                MouthOfTruthLog.LogInfo($"Answer capture finalization completed in {captureStopwatch.ElapsedMilliseconds} ms.");
                answerCaptureResult = await answerCaptureTask;
                faceCaptureResult = await faceCaptureTask;

                if (answerCaptureResult.AnswerTranscript.IsEmpty == false)
                {
                    applyTranscriptUpdate(answerCaptureResult.AnswerTranscript);
                    snapshot = mGameStateMachine.CreateSnapshot();
                }

                AnswerAnalysisRequest answerAnalysisRequest = buildAnalysisRequest(snapshot, answerCaptureResult, faceCaptureResult);
                AnswerAnalysisResult answerAnalysisResult;

                try
                {
                    System.Diagnostics.Stopwatch analysisStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    answerAnalysisResult = await mAnswerAnalysisClient.AnalyzeAsync(answerAnalysisRequest, mLifecycleCancellationTokenSource.Token);
                    analysisStopwatch.Stop();
                    MouthOfTruthLog.LogInfo($"Answer analysis completed in {analysisStopwatch.ElapsedMilliseconds} ms.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Primary answer analysis failed. Falling back to deterministic analysis.\n" + exception);
                    answerAnalysisResult = await new DeterministicAnswerAnalysisClient().AnalyzeAsync(answerAnalysisRequest, mLifecycleCancellationTokenSource.Token);
                }

                SecondsDuration remainingAnalysisPresentationDuration = getRemainingAnalysisPresentationDuration(analysisPresentationStartedAtSeconds);
                await waitForRealtimeDurationAsync(remainingAnalysisPresentationDuration, mLifecycleCancellationTokenSource.Token);
                await mGameView.PlayAnalysisCompleteTransitionAsync();
                applyTranscriptUpdate(answerAnalysisResult.AnswerTranscript);
                snapshot = mGameStateMachine.CreateSnapshot();
                mGameStateMachine.CompleteAnalysis(answerAnalysisResult);
                mGameView.ShowResult(answerAnalysisResult.VerdictKind);
                await mGameView.PlayResultRevealAnimationAsync(answerAnalysisResult.VerdictKind);
            }
            finally
            {
                if (answerCaptureResult != null || faceCaptureResult != null)
                {
                    AnswerAudioFilePath answerAudioFilePath = answerCaptureResult == null ? AnswerAudioFilePath.Empty : answerCaptureResult.AudioFilePath;
                    FaceFramesDirectoryPath faceFramesDirectoryPath = faceCaptureResult == null ? FaceFramesDirectoryPath.Empty : faceCaptureResult.FaceFramesDirectoryPath;
                    MouthOfTruthSessionArtifactCleaner.CleanAnalysisArtifacts(answerAudioFilePath, faceFramesDirectoryPath);
                }

                mIsTransitionBusy = false;
            }
        }

        private static SecondsDuration getRemainingAnalysisPresentationDuration(float analysisPresentationStartedAtSeconds)
        {
            float elapsedAnalysisPresentationSeconds = Time.unscaledTime - analysisPresentationStartedAtSeconds;
            float remainingAnalysisPresentationSeconds = Mathf.Max(0.0f, MINIMUM_ANALYSIS_PRESENTATION_DURATION.Value - elapsedAnalysisPresentationSeconds);
            return new SecondsDuration(remainingAnalysisPresentationSeconds);
        }

        private static async Task waitForRealtimeDurationAsync(SecondsDuration duration, CancellationToken cancellationToken)
        {
            if (duration.Value <= 0.0f)
            {
                return;
            }

            float targetTimeSeconds = Time.unscaledTime + duration.Value;

            while (Time.unscaledTime < targetTimeSeconds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private AnswerAnalysisRequest buildAnalysisRequest(GameSessionSnapshot snapshot, AnswerCaptureResult answerCaptureResult, FaceCaptureResult faceCaptureResult)
        {
            AnswerTranscript answerTranscript = answerCaptureResult.AnswerTranscript.IsEmpty
                ? snapshot.CurrentAnswerTranscript.Trimmed()
                : answerCaptureResult.AnswerTranscript.Trimmed();

            if (snapshot.SelectedQuestionDefinitionOrNull == null)
            {
                throw new InvalidOperationException("Cannot build an analysis request without a selected question.");
            }

            return new AnswerAnalysisRequest(snapshot.SelectedQuestionDefinitionOrNull, answerTranscript, answerCaptureResult.AudioFilePath, faceCaptureResult.FaceFramesDirectoryPath, faceCaptureResult.CapturedFrameCount, answerCaptureResult.VoiceSegmentCount);
        }
    }
}
