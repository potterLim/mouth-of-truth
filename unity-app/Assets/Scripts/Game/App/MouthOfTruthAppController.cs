using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Diagnostics;
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
                MouthOfTruthLog.LogInfo("MouthOfTruthAppController started.");
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
                MouthOfTruthLog.LogInfo("MouthOfTruthGameView initialized.");
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

            PointerScreenPosition? pointerScreenPositionOrNull = tryGetPointerScreenPositionOrNull();
            PointerScreenPosition? presentedPointerScreenPositionOrNull = getPresentedPointerScreenPositionOrNull(pointerScreenPositionOrNull);
            updatePointerPresentation(presentedPointerScreenPositionOrNull);

            if (mGameView.ConsumeExitRequested())
            {
                requestApplicationExit();
                return;
            }

            bool canAcceptPointerActivation = mPointerPresentationOverrideRemainingSeconds <= 0.0f
                && updatePointerActivationGuard(presentedPointerScreenPositionOrNull);
            PointerScreenPosition? activatablePointerScreenPositionOrNull = canAcceptPointerActivation ? presentedPointerScreenPositionOrNull : null;

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

    }
}
