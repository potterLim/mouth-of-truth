using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Session
{
    public class GameSessionSnapshot
    {
        public GameSessionSnapshot(EGameFlowState currentState, QuestionRoundSelection currentRoundSelection, EQuestionCardSlot? selectedQuestionCardSlotOrNull, QuestionDefinition selectedQuestionDefinitionOrNull, EVerdictKind? currentVerdictKindOrNull, AnswerTranscript currentAnswerTranscript, SecondsDuration hoveredCardDwellDuration, SecondsDuration elapsedAnswerDuration, SecondsDuration elapsedSilenceDuration)
        {
            CurrentState = currentState;
            CurrentRoundSelection = currentRoundSelection;
            SelectedQuestionCardSlotOrNull = selectedQuestionCardSlotOrNull;
            SelectedQuestionDefinitionOrNull = selectedQuestionDefinitionOrNull;
            CurrentVerdictKindOrNull = currentVerdictKindOrNull;
            CurrentAnswerTranscript = currentAnswerTranscript;
            HoveredCardDwellDuration = hoveredCardDwellDuration;
            ElapsedAnswerDuration = elapsedAnswerDuration;
            ElapsedSilenceDuration = elapsedSilenceDuration;
        }

        public EGameFlowState CurrentState { get; }

        public QuestionRoundSelection CurrentRoundSelection { get; }

        public EQuestionCardSlot? SelectedQuestionCardSlotOrNull { get; }

        public QuestionDefinition SelectedQuestionDefinitionOrNull { get; }

        public EVerdictKind? CurrentVerdictKindOrNull { get; }

        public AnswerTranscript CurrentAnswerTranscript { get; }

        public SecondsDuration HoveredCardDwellDuration { get; }

        public SecondsDuration ElapsedAnswerDuration { get; }

        public SecondsDuration ElapsedSilenceDuration { get; }

        public bool IsAnswerPaused => CurrentState == EGameFlowState.AnswerPaused;
    }
}
