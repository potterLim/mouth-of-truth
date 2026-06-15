using System;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Voice;

namespace MouthOfTruth.Game.Session
{
    public class AnswerCollectionPolicy
    {
        public AnswerCollectionPolicy()
            : this(new SecondsDuration(2.6f), new SecondsDuration(1.2f), new SecondsDuration(8.0f))
        {
        }

        public AnswerCollectionPolicy(SecondsDuration initialSilenceGraceDuration, SecondsDuration silenceTimeoutDuration, SecondsDuration maximumAnswerDuration)
        {
            if (silenceTimeoutDuration.Value <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(silenceTimeoutDuration));
            }

            if (maximumAnswerDuration.Value <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumAnswerDuration));
            }

            InitialSilenceGraceDuration = initialSilenceGraceDuration;
            SilenceTimeoutDuration = silenceTimeoutDuration;
            MaximumAnswerDuration = maximumAnswerDuration;
        }

        public SecondsDuration InitialSilenceGraceDuration { get; }

        public SecondsDuration SilenceTimeoutDuration { get; }

        public SecondsDuration MaximumAnswerDuration { get; }

        public AnswerCollectionTickResult Advance(SecondsDuration elapsedAnswerDuration, SecondsDuration elapsedSilenceDuration, SecondsDuration deltaTimeDuration, ESpeechDetectionState speechDetectionState)
        {
            SecondsDuration nextElapsedAnswerDuration = elapsedAnswerDuration.Add(deltaTimeDuration);
            SecondsDuration nextElapsedSilenceDuration = speechDetectionState == ESpeechDetectionState.SpeechDetected
                ? SecondsDuration.Zero
                : elapsedSilenceDuration.Add(deltaTimeDuration);

            EAnswerCollectionFinishReason finishReason = EAnswerCollectionFinishReason.None;
            if (nextElapsedAnswerDuration.Value >= MaximumAnswerDuration.Value)
            {
                finishReason = EAnswerCollectionFinishReason.Timeout;
            }
            else if (nextElapsedAnswerDuration.Value >= InitialSilenceGraceDuration.Value && nextElapsedSilenceDuration.Value >= SilenceTimeoutDuration.Value)
            {
                finishReason = EAnswerCollectionFinishReason.Silence;
            }

            return new AnswerCollectionTickResult(nextElapsedAnswerDuration, nextElapsedSilenceDuration, finishReason);
        }
    }

    public readonly struct AnswerCollectionTickResult
    {
        public AnswerCollectionTickResult(SecondsDuration elapsedAnswerDuration, SecondsDuration elapsedSilenceDuration, EAnswerCollectionFinishReason finishReason)
        {
            ElapsedAnswerDuration = elapsedAnswerDuration;
            ElapsedSilenceDuration = elapsedSilenceDuration;
            FinishReason = finishReason;
        }

        public SecondsDuration ElapsedAnswerDuration { get; }

        public SecondsDuration ElapsedSilenceDuration { get; }

        public EAnswerCollectionFinishReason FinishReason { get; }

        public bool ShouldFinishForSilence => FinishReason == EAnswerCollectionFinishReason.Silence;

        public bool ShouldFinishForTimeout => FinishReason == EAnswerCollectionFinishReason.Timeout;
    }
}
