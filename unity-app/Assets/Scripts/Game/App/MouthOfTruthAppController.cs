using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Narration;
using MouthOfTruth.Game.Presentation.Runtime;
using MouthOfTruth.Game.Session;
using MouthOfTruth.Game.Voice;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    [DisallowMultipleComponent]
    public partial class MouthOfTruthAppController : MonoBehaviour
    {
        private const float MINIMUM_ANALYSIS_PRESENTATION_SECONDS = 2.5f;
        private static readonly SecondsDuration CARD_SELECTION_DWELL_DURATION = new SecondsDuration(2.1f);
        private static readonly SecondsDuration UI_ACTION_DWELL_DURATION = new SecondsDuration(1.05f);
        private MouthOfTruthGameView mGameView;
        private MouthOfTruthGameStateMachine mGameStateMachine;
        private IQuestionNarrationService mQuestionNarrationService;
        private IAnswerAnalysisClient mAnswerAnalysisClient;
        private IHandInteractionInputAdapter mHandInteractionInputAdapter;
        private IAnswerCaptureInputAdapter mAnswerCaptureInputAdapter;
        private IFaceCaptureInputAdapter mFaceCaptureInputAdapter;
        private CancellationTokenSource mLifecycleCancellationTokenSource;
        private UiActionDwellSelectionTracker mUiActionDwellSelectionTracker;

        private bool mIsInitialized;
        private bool mIsTransitionBusy;
        private bool mHasShownFirstRunTutorial;
        private bool mHasCleanedSessionArtifactsOnExit;
        private AnswerTranscript mLastObservedTranscript = AnswerTranscript.Empty;

        private void Awake()
        {
            Application.runInBackground = true;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }

        private void Start()
        {
            _ = startAsync();
        }

        private async Task startAsync()
        {
            try
            {
                Debug.Log("MouthOfTruthAppController started.");
                mLifecycleCancellationTokenSource = new CancellationTokenSource();
                tryCleanAllSessionArtifacts("startup");
                mAnswerAnalysisClient = createAnalysisClient();
                _ = warmUpAnalysisClientAsync();
                mGameView = GetComponent<MouthOfTruthGameView>();
                if (mGameView == null)
                {
                    mGameView = gameObject.AddComponent<MouthOfTruthGameView>();
                }

                await mGameView.InitializeAsync();
                Debug.Log("MouthOfTruthGameView initialized.");
                applyRuntimeCursorPresentation(isFocused: true);

                await requestCaptureAuthorizationsAsync();

                initializeStateMachine();
                mIsInitialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void Update()
        {
            if (mIsInitialized == false)
            {
                return;
            }

            Vector2? pointerScreenPositionOrNull = tryGetPointerScreenPositionOrNull();
            Vector2? presentedPointerScreenPositionOrNull = getPresentedPointerScreenPositionOrNull(pointerScreenPositionOrNull);
            updatePointerPresentation(presentedPointerScreenPositionOrNull);

            if (mGameView.ConsumeExitRequested())
            {
                requestApplicationExit();
                return;
            }

            bool canAcceptPointerActivation = mPointerPresentationOverrideRemainingSeconds <= 0.0f
                && updatePointerActivationGuard(presentedPointerScreenPositionOrNull);
            Vector2? activatablePointerScreenPositionOrNull = canAcceptPointerActivation ? presentedPointerScreenPositionOrNull : null;

            if (mIsTransitionBusy)
            {
                if (mGameView.IsFirstRunTutorialVisible)
                {
                    updateUiActionSelection(activatablePointerScreenPositionOrNull);
                }

                return;
            }

            if (updateUiActionSelection(activatablePointerScreenPositionOrNull))
            {
                return;
            }

            if (mGameView.ConsumeStartRequested())
            {
                runObservedAsync(startGameAsync, "start game");
                return;
            }

            if (mGameView.ConsumeTryAgainRequested())
            {
                runObservedAsync(restartGameAsync, "restart game");
                return;
            }

            if (mGameView.ConsumeBackToTitleRequested() || (mGameStateMachine.CurrentState == EGameFlowState.ShowingResult && mHandInteractionInputAdapter.WasReturnToTitleTriggeredThisFrame()))
            {
                mGameStateMachine.ReturnToStart();
                mGameView.ShowStartScreen();
                resetAnswerTracking();
                resetInteractionSelectionState();
                return;
            }

            switch (mGameStateMachine.CurrentState)
            {
                case EGameFlowState.AwaitingCardSelection:
                    updateCardSelection(activatablePointerScreenPositionOrNull);
                    break;

                case EGameFlowState.AwaitingHandInsertion:
                    updateHandPromptDismissal(presentedPointerScreenPositionOrNull);
                    updateHandInsertion(activatablePointerScreenPositionOrNull);
                    break;

                case EGameFlowState.AnswerPaused:
                    updateHandInsertion(activatablePointerScreenPositionOrNull);
                    break;

                case EGameFlowState.Answering:
                    updateAnswering(pointerScreenPositionOrNull);
                    break;

                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            mLifecycleCancellationTokenSource?.Cancel();
            mAnswerCaptureInputAdapter?.CancelCollection();
            mFaceCaptureInputAdapter?.CancelCollection();
            (mAnswerAnalysisClient as IDisposable)?.Dispose();
            mLifecycleCancellationTokenSource?.Dispose();
            cleanSessionArtifactsOnExit();
            restoreSystemCursor();
        }

        private void OnApplicationQuit()
        {
            mLifecycleCancellationTokenSource?.Cancel();
            cleanSessionArtifactsOnExit();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                applyRuntimeCursorPresentation(isFocused: true);
                return;
            }

            restoreSystemCursor();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                restoreSystemCursor();
                return;
            }

            applyRuntimeCursorPresentation(isFocused: true);
        }

        private void initializeStateMachine()
        {
            QuestionPoolFilePath questionPoolFilePath = new QuestionPoolFilePath(System.IO.Path.Combine(Application.streamingAssetsPath, "questions", "question_pool.json"));
            IReadOnlyList<QuestionDefinition> questionDefinitions = QuestionPoolLoader.LoadQuestionDefinitions(questionPoolFilePath);
            QuestionDeckService questionDeckService = new QuestionDeckService(questionDefinitions);
            CardDwellSelectionTracker cardDwellSelectionTracker = new CardDwellSelectionTracker(CARD_SELECTION_DWELL_DURATION);
            AnswerCollectionPolicy answerCollectionPolicy = new AnswerCollectionPolicy();
            mUiActionDwellSelectionTracker = new UiActionDwellSelectionTracker(UI_ACTION_DWELL_DURATION);

            mGameStateMachine = new MouthOfTruthGameStateMachine(questionDeckService, cardDwellSelectionTracker, answerCollectionPolicy);
            mQuestionNarrationService = createNarrationService();
            if (mAnswerAnalysisClient == null)
            {
                mAnswerAnalysisClient = createAnalysisClient();
            }

            mHandInteractionInputAdapter = createHandInteractionInputAdapter();
            mAnswerCaptureInputAdapter = createAnswerCaptureInputAdapter();
            prepareAnswerAudioSession();
            mFaceCaptureInputAdapter = createFaceCaptureInputAdapter();
            mGameView.SetAnswerTranscriptPlaceholder(mAnswerCaptureInputAdapter.TranscriptPlaceholderText);
            mGameView.SetAnswerTranscriptInputMode(mAnswerCaptureInputAdapter.TranscriptInputMode);
            resetInteractionSelectionState();
            mGameStateMachine.OpenStartScreen();
        }

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

        private void updateCardSelection(Vector2? pointerScreenPositionOrNull)
        {
            EQuestionCardSlot? hoveredQuestionCardSlotOrNull = mGameView.GetHoveredQuestionCardSlotOrNull(pointerScreenPositionOrNull);
            EQuestionCardSlot? confirmedQuestionCardSlotOrNull = mGameStateMachine.UpdateCardSelectionOrNull(hoveredQuestionCardSlotOrNull, new SecondsDuration(Time.deltaTime));
            GameSessionSnapshot snapshot = mGameStateMachine.CreateSnapshot();

            NormalizedProgress hoverProgress = hoveredQuestionCardSlotOrNull == null
                || snapshot.CurrentState != EGameFlowState.AwaitingCardSelection
                ? NormalizedProgress.Zero
                : NormalizedProgress.FromUnclamped(snapshot.HoveredCardDwellDuration.Value / CARD_SELECTION_DWELL_DURATION.Value);

            mGameView.UpdateCardHoverVisual(hoveredQuestionCardSlotOrNull, hoverProgress);

            if (confirmedQuestionCardSlotOrNull.HasValue == false)
            {
                return;
            }

            EQuestionCardSlot confirmedQuestionCardSlot = confirmedQuestionCardSlotOrNull.Value;
            QuestionDefinition selectedQuestionDefinition = mGameStateMachine.CreateSnapshot().SelectedQuestionDefinitionOrNull;
            if (selectedQuestionDefinition == null)
            {
                throw new InvalidOperationException("Confirmed card selection did not produce a selected question.");
            }

            runObservedAsync(() => revealQuestionAsync(confirmedQuestionCardSlot, selectedQuestionDefinition), "reveal question");
        }

        private void updateHandInsertion(Vector2? pointerScreenPositionOrNull)
        {
            EHandAnchorState handAnchorState = mGameView.GetHandAnchorState(pointerScreenPositionOrNull);

            bool canStartInsertion = mLastObservedHandAnchorState == EHandAnchorState.OutsideMouth
                && handAnchorState != EHandAnchorState.OutsideMouth;

            if (canStartInsertion == false)
            {
                mLastObservedHandAnchorState = handAnchorState;
                return;
            }

            mLastObservedHandAnchorState = handAnchorState;
            runObservedAsync(insertHandAsync, "insert hand");
        }

        private void updateAnswering(Vector2? pointerScreenPositionOrNull)
        {
            EHandAnchorState handAnchorState = mGameView.GetHandAnchorState(pointerScreenPositionOrNull);
            mLastObservedHandAnchorState = handAnchorState == EHandAnchorState.OutsideMouth
                ? EHandAnchorState.AtFrontAnchor
                : handAnchorState;

            SecondsDuration frameDuration = new SecondsDuration(Time.deltaTime);
            AnswerCaptureFrameSnapshot frameSnapshot = mAnswerCaptureInputAdapter.Update(frameDuration);
            mFaceCaptureInputAdapter.Update(frameDuration);
            applyTranscriptUpdate(frameSnapshot.AnswerTranscript);
            EAnswerCollectionFinishReason answerCollectionFinishReason = mGameStateMachine.AdvanceAnswerCollection(frameDuration, frameSnapshot.SpeechDetectionState);
            GameSessionSnapshot snapshot = mGameStateMachine.CreateSnapshot();
            mGameView.UpdateAnswerMetrics(snapshot.ElapsedAnswerDuration, snapshot.ElapsedSilenceDuration);

            if (answerCollectionFinishReason != EAnswerCollectionFinishReason.None)
            {
                runObservedAsync(analyzeAnswerAsync, "analyze answer");
            }
        }

        private bool updateUiActionSelection(Vector2? pointerScreenPositionOrNull)
        {
            EUiActionTarget? hoveredUiActionTargetOrNull = mGameView.GetHoveredUiActionTargetOrNull(pointerScreenPositionOrNull);
            hoveredUiActionTargetOrNull = isUiActionAllowedForCurrentState(hoveredUiActionTargetOrNull) ? hoveredUiActionTargetOrNull : null;
            EUiActionTarget? confirmedUiActionTargetOrNull = mUiActionDwellSelectionTracker
                .UpdateHoveredTargetOrNull(hoveredUiActionTargetOrNull, new SecondsDuration(Time.deltaTime));
            NormalizedProgress hoverProgress = hoveredUiActionTargetOrNull == null
                ? NormalizedProgress.Zero
                : NormalizedProgress.FromUnclamped(mUiActionDwellSelectionTracker.HoveredDuration.Value / UI_ACTION_DWELL_DURATION.Value);

            mGameView.UpdateActionButtonHoverVisual(hoveredUiActionTargetOrNull, hoverProgress);

            if (confirmedUiActionTargetOrNull == null)
            {
                return false;
            }

            switch (confirmedUiActionTargetOrNull.Value)
            {
                case EUiActionTarget.StartGame:
                    runObservedAsync(startGameAsync, "start game");
                    return true;

                case EUiActionTarget.TryAgain:
                    runObservedAsync(restartGameAsync, "restart game");
                    return true;

                case EUiActionTarget.BackToTitle:
                    mGameStateMachine.ReturnToStart();
                    mGameView.ShowStartScreen();
                    resetAnswerTracking();
                    resetInteractionSelectionState();
                    return true;

                case EUiActionTarget.ExitGame:
                    requestApplicationExit();
                    return true;

                default:
                    return false;
            }
        }

        private bool isUiActionAllowedForCurrentState(EUiActionTarget? uiActionTargetOrNull)
        {
            if (uiActionTargetOrNull == null)
            {
                return false;
            }

            return uiActionTargetOrNull.Value switch
            {
                EUiActionTarget.ExitGame => true,
                EUiActionTarget.StartGame => mGameStateMachine.CurrentState == EGameFlowState.StartScreen,
                EUiActionTarget.TryAgain => mGameStateMachine.CurrentState == EGameFlowState.ShowingResult,
                EUiActionTarget.BackToTitle => mGameStateMachine.CurrentState == EGameFlowState.ShowingResult,
                _ => false,
            };
        }

        private void requestApplicationExit()
        {
            mAnswerCaptureInputAdapter?.CancelCollection();
            mFaceCaptureInputAdapter?.CancelCollection();
            restoreSystemCursor();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
                beginBottomCenterPointerSettle(mGameView.HandPromptPanelHoldDuration.Value);
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
                Debug.Log($"Answer capture finalization completed in {captureStopwatch.ElapsedMilliseconds} ms.");
                answerCaptureResult = answerCaptureTask.Result;
                faceCaptureResult = faceCaptureTask.Result;

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
                    Debug.Log($"Answer analysis completed in {analysisStopwatch.ElapsedMilliseconds} ms.");
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

                float elapsedAnalysisPresentationSeconds = Time.unscaledTime - analysisPresentationStartedAtSeconds;
                await waitForRealtimeSecondsAsync(MINIMUM_ANALYSIS_PRESENTATION_SECONDS - elapsedAnalysisPresentationSeconds, mLifecycleCancellationTokenSource.Token);
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

        private static async Task waitForRealtimeSecondsAsync(float durationSeconds, CancellationToken cancellationToken)
        {
            if (durationSeconds <= 0.0f)
            {
                return;
            }

            float targetTimeSeconds = Time.unscaledTime + durationSeconds;

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

        private void resetAnswerTracking()
        {
            mLastObservedTranscript = AnswerTranscript.Empty;
            mGameView?.ClearAnswerTranscript();
            mAnswerCaptureInputAdapter?.Reset();
            mFaceCaptureInputAdapter?.Reset();
            mLastObservedHandAnchorState = EHandAnchorState.OutsideMouth;
            resetHandPromptDismissalTracking();
        }

        private void runObservedAsync(Func<Task> asyncOperation, string operationName)
        {
            _ = runObservedTaskAsync(asyncOperation, operationName);
        }

        private async Task runObservedTaskAsync(Func<Task> asyncOperation, string operationName)
        {
            if (asyncOperation == null)
            {
                return;
            }

            try
            {
                await asyncOperation();
            }
            catch (OperationCanceledException)
            {
                mIsTransitionBusy = false;
            }
            catch (Exception exception)
            {
                Debug.LogError("MouthOfTruth transition failed while trying to " + operationName + ". Returning to the start screen.\n" + exception);
                recoverToStartScreenAfterTransitionFailure();
            }
        }

        private void recoverToStartScreenAfterTransitionFailure()
        {
            mIsTransitionBusy = false;
            mAnswerCaptureInputAdapter?.CancelCollection();
            mFaceCaptureInputAdapter?.CancelCollection();

            try
            {
                resetAnswerTracking();
                resetInteractionSelectionState();
                mGameStateMachine?.OpenStartScreen();
                mGameView?.ShowStartScreen();
            }
            catch (Exception recoveryException)
            {
                Debug.LogError("MouthOfTruth transition recovery failed.\n" + recoveryException);
            }
        }

        private void tryCleanAllSessionArtifacts(string reason)
        {
            try
            {
                MouthOfTruthSessionArtifactCleaner.CleanAllSessionArtifacts();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Session artifact cleanup failed during " + reason + ".\n" + exception);
            }
        }

        private void cleanSessionArtifactsOnExit()
        {
            if (mHasCleanedSessionArtifactsOnExit)
            {
                return;
            }

            mHasCleanedSessionArtifactsOnExit = true;
            tryCleanAllSessionArtifacts("application exit");
        }

    }
}
