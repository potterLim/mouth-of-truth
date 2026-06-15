using System;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Session;
using MouthOfTruth.Game.Voice;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    public partial class MouthOfTruthAppController
    {
        private void updateCardSelection(PointerScreenPosition? pointerScreenPositionOrNull)
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

        private void updateHandInsertion(PointerScreenPosition? pointerScreenPositionOrNull)
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

        private void updateAnswering(PointerScreenPosition? pointerScreenPositionOrNull)
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

        private bool updateUiActionSelection(PointerScreenPosition? pointerScreenPositionOrNull)
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
    }
}
