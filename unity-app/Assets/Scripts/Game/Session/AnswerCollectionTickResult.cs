using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Session
{
    public readonly struct AnswerCollectionTickResult
    {
        public AnswerCollectionTickResult(
            SecondsDuration elapsedAnswerDuration,
            SecondsDuration elapsedSilenceDuration,
            EAnswerCollectionFinishReason finishReason)
        {
            ElapsedAnswerDuration = elapsedAnswerDuration;
            ElapsedSilenceDuration = elapsedSilenceDuration;
            FinishReason = finishReason;
        }

        public SecondsDuration ElapsedAnswerDuration
        {
            get;
        }

        public SecondsDuration ElapsedSilenceDuration
        {
            get;
        }

        public EAnswerCollectionFinishReason FinishReason
        {
            get;
        }

        public bool ShouldFinishForSilence => FinishReason == EAnswerCollectionFinishReason.Silence;

        public bool ShouldFinishForTimeout => FinishReason == EAnswerCollectionFinishReason.Timeout;
    }
}
