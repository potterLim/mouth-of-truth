using System;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Voice;

namespace MouthOfTruth.Game.Session
{
    public class MouthOfTruthGameStateMachine
    {
        private readonly QuestionDeckService mQuestionDeckService;
        private readonly CardDwellSelectionTracker mCardDwellSelectionTracker;
        private readonly AnswerCollectionPolicy mAnswerCollectionPolicy;

        private QuestionRoundSelection mCurrentRoundSelection;
        private EQuestionCardSlot? mSelectedQuestionCardSlotOrNull;
        private QuestionDefinition mSelectedQuestionDefinitionOrNull;
        private EVerdictKind? mCurrentVerdictKindOrNull;
        private AnswerTranscript mCurrentAnswerTranscript = AnswerTranscript.Empty;
        private SecondsDuration mElapsedAnswerDuration;
        private SecondsDuration mElapsedSilenceDuration;

        public MouthOfTruthGameStateMachine(
            QuestionDeckService questionDeckService,
            CardDwellSelectionTracker cardDwellSelectionTracker,
            AnswerCollectionPolicy answerCollectionPolicy)
        {
            if (questionDeckService == null)
            {
                throw new ArgumentNullException(nameof(questionDeckService));
            }

            if (cardDwellSelectionTracker == null)
            {
                throw new ArgumentNullException(nameof(cardDwellSelectionTracker));
            }

            if (answerCollectionPolicy == null)
            {
                throw new ArgumentNullException(nameof(answerCollectionPolicy));
            }

            mQuestionDeckService = questionDeckService;
            mCardDwellSelectionTracker = cardDwellSelectionTracker;
            mAnswerCollectionPolicy = answerCollectionPolicy;
            CurrentState = EGameFlowState.StartScreen;
        }

        public EGameFlowState CurrentState
        {
            get; private set;
        }

        public void OpenStartScreen()
        {
            resetTransientRoundState();
            CurrentState = EGameFlowState.StartScreen;
        }

        public void StartGame()
        {
            mCurrentRoundSelection = mQuestionDeckService.DrawNextRound();
            mSelectedQuestionCardSlotOrNull = null;
            mSelectedQuestionDefinitionOrNull = null;
            mCurrentVerdictKindOrNull = null;
            mCurrentAnswerTranscript = AnswerTranscript.Empty;
            mElapsedAnswerDuration = SecondsDuration.Zero;
            mElapsedSilenceDuration = SecondsDuration.Zero;
            CurrentState = EGameFlowState.PresentingCards;
        }

        public void MarkCardPresentationCompleted()
        {
            ensureCurrentState(EGameFlowState.PresentingCards);
            CurrentState = EGameFlowState.AwaitingCardSelection;
        }

        public EQuestionCardSlot? UpdateCardSelectionOrNull(EQuestionCardSlot? hoveredQuestionCardSlotOrNull, SecondsDuration deltaTimeDuration)
        {
            ensureCurrentState(EGameFlowState.AwaitingCardSelection);

            EQuestionCardSlot? confirmedQuestionCardSlotOrNull = mCardDwellSelectionTracker
                .UpdateHoveredCardOrNull(hoveredQuestionCardSlotOrNull, deltaTimeDuration);

            if (confirmedQuestionCardSlotOrNull == null)
            {
                return null;
            }

            mSelectedQuestionCardSlotOrNull = confirmedQuestionCardSlotOrNull;
            mSelectedQuestionDefinitionOrNull = mCurrentRoundSelection.GetQuestionBySlot(confirmedQuestionCardSlotOrNull.Value);
            CurrentState = EGameFlowState.RevealingQuestionCard;
            return confirmedQuestionCardSlotOrNull;
        }

        public void MarkQuestionRevealCompleted()
        {
            ensureCurrentState(EGameFlowState.RevealingQuestionCard);
            CurrentState = EGameFlowState.NarratingQuestion;
        }

        public void MarkQuestionNarrationCompleted()
        {
            ensureCurrentState(EGameFlowState.NarratingQuestion);
            CurrentState = EGameFlowState.AwaitingHandInsertion;
        }

        public void NotifyHandReachedFrontAnchor()
        {
            ensureCurrentState(EGameFlowState.AwaitingHandInsertion);
            CurrentState = EGameFlowState.InsertingHand;
        }

        public void NotifyHandReachedInnerAnchor()
        {
            if (CurrentState == EGameFlowState.InsertingHand)
            {
                mElapsedAnswerDuration = SecondsDuration.Zero;
                mElapsedSilenceDuration = SecondsDuration.Zero;
                CurrentState = EGameFlowState.Answering;
                return;
            }

            if (CurrentState == EGameFlowState.AnswerPaused)
            {
                CurrentState = EGameFlowState.Answering;
                return;
            }

            throw new InvalidOperationException($"Cannot move to the inner anchor from state: {CurrentState}");
        }

        public void NotifyHandExitedFrontAnchor()
        {
            if (CurrentState != EGameFlowState.Answering)
            {
                return;
            }

            // Hand insertion is a start trigger. Once answering starts, temporary hand loss does not pause collection.
        }

        public EAnswerCollectionFinishReason AdvanceAnswerCollection(SecondsDuration deltaTimeDuration, ESpeechDetectionState speechDetectionState)
        {
            ensureCurrentState(EGameFlowState.Answering);

            AnswerCollectionTickResult answerCollectionTickResult = mAnswerCollectionPolicy.Advance(
                mElapsedAnswerDuration,
                mElapsedSilenceDuration,
                deltaTimeDuration,
                speechDetectionState);

            mElapsedAnswerDuration = answerCollectionTickResult.ElapsedAnswerDuration;
            mElapsedSilenceDuration = answerCollectionTickResult.ElapsedSilenceDuration;

            if (answerCollectionTickResult.FinishReason != EAnswerCollectionFinishReason.None)
            {
                CurrentState = EGameFlowState.AnalyzingAnswer;
            }

            return answerCollectionTickResult.FinishReason;
        }

        public void UpdateAnswerTranscript(AnswerTranscript answerTranscript)
        {
            mCurrentAnswerTranscript = answerTranscript;
        }

        public void ResetCardSelectionHover()
        {
            mCardDwellSelectionTracker.Reset();
        }

        public void CompleteAnalysis(AnswerAnalysisResult answerAnalysisResult)
        {
            ensureCurrentState(EGameFlowState.AnalyzingAnswer);

            if (answerAnalysisResult == null)
            {
                throw new ArgumentNullException(nameof(answerAnalysisResult));
            }

            mCurrentVerdictKindOrNull = answerAnalysisResult.VerdictKind;
            CurrentState = EGameFlowState.ShowingResult;
        }

        public void TryAgain()
        {
            ensureCurrentState(EGameFlowState.ShowingResult);
            StartGame();
        }

        public void ReturnToStart()
        {
            ensureCurrentState(EGameFlowState.ShowingResult);
            OpenStartScreen();
        }

        public GameSessionSnapshot CreateSnapshot()
        {
            return new GameSessionSnapshot(
                CurrentState,
                mCurrentRoundSelection,
                mSelectedQuestionCardSlotOrNull,
                mSelectedQuestionDefinitionOrNull,
                mCurrentVerdictKindOrNull,
                mCurrentAnswerTranscript,
                mCardDwellSelectionTracker.HoveredDuration,
                mElapsedAnswerDuration,
                mElapsedSilenceDuration);
        }

        private void ensureCurrentState(EGameFlowState expectedGameFlowState)
        {
            if (CurrentState != expectedGameFlowState)
            {
                throw new InvalidOperationException($"Expected state {expectedGameFlowState}, but was {CurrentState}.");
            }
        }

        private void resetTransientRoundState()
        {
            mCardDwellSelectionTracker.Reset();
            mCurrentRoundSelection = null;
            mSelectedQuestionCardSlotOrNull = null;
            mSelectedQuestionDefinitionOrNull = null;
            mCurrentVerdictKindOrNull = null;
            mCurrentAnswerTranscript = AnswerTranscript.Empty;
            mElapsedAnswerDuration = SecondsDuration.Zero;
            mElapsedSilenceDuration = SecondsDuration.Zero;
        }
    }
}
