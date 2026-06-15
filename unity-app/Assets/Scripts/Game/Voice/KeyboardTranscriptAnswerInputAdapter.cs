using System;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Presentation.Runtime;

namespace MouthOfTruth.Game.Voice
{
    public class KeyboardTranscriptAnswerInputAdapter : IAnswerCaptureInputAdapter
    {
        private const float TYPING_ACTIVITY_GRACE_SECONDS = 0.75f;

        private readonly MouthOfTruthGameView mGameView;

        private float mTypingActivityGraceSeconds;
        private AnswerTranscript mLastObservedTranscript = AnswerTranscript.Empty;

        public KeyboardTranscriptAnswerInputAdapter(MouthOfTruthGameView gameView)
        {
            if (gameView == null)
            {
                throw new ArgumentNullException(nameof(gameView));
            }

            mGameView = gameView;
        }

        public bool RequiresManualTextEntry => true;

        public string TranscriptPlaceholderText =>
            "답변을 입력하세요. 입력이 멈추면 3초 뒤 자동 종료됩니다.";

        public void Reset()
        {
            mTypingActivityGraceSeconds = 0.0f;
            mLastObservedTranscript = AnswerTranscript.Empty;
        }

        public void BeginCollection()
        {
        }

        public void PauseCollection()
        {
        }

        public void ResumeCollection()
        {
        }

        public void CancelCollection()
        {
            Reset();
        }

        public AnswerCaptureFrameSnapshot Update(SecondsDuration deltaTime)
        {
            AnswerTranscript currentTranscript = mGameView.GetAnswerTranscript();

            if (currentTranscript.Equals(mLastObservedTranscript) == false)
            {
                mLastObservedTranscript = currentTranscript;
                mTypingActivityGraceSeconds = TYPING_ACTIVITY_GRACE_SECONDS;
            }

            if (mTypingActivityGraceSeconds > 0.0f)
            {
                mTypingActivityGraceSeconds = Math.Max(0.0f, mTypingActivityGraceSeconds - deltaTime.Value);
            }

            ESpeechDetectionState speechDetectionState = mTypingActivityGraceSeconds > 0.0f
                ? ESpeechDetectionState.SpeechDetected
                : ESpeechDetectionState.Silent;
            return new AnswerCaptureFrameSnapshot(currentTranscript, speechDetectionState);
        }

        public Task<AnswerCaptureResult> CompleteCollectionAsync(QuestionId questionId, CancellationToken cancellationToken)
        {
            _ = questionId;
            cancellationToken.ThrowIfCancellationRequested();
            AnswerTranscript answerTranscript = mGameView.GetAnswerTranscript().Trimmed();
            VoiceSegmentCount voiceSegmentCount = answerTranscript.IsEmpty ? VoiceSegmentCount.Zero : new VoiceSegmentCount(1);
            return Task.FromResult(new AnswerCaptureResult(answerTranscript, AnswerAudioFilePath.Empty, voiceSegmentCount));
        }
    }
}
