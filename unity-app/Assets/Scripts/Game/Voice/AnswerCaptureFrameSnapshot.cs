using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Voice
{
    public class AnswerCaptureFrameSnapshot
    {
        public AnswerCaptureFrameSnapshot(AnswerTranscript answerTranscript, ESpeechDetectionState speechDetectionState)
        {
            AnswerTranscript = answerTranscript;
            SpeechDetectionState = speechDetectionState;
        }

        public AnswerTranscript AnswerTranscript
        {
            get;
        }

        public ESpeechDetectionState SpeechDetectionState
        {
            get;
        }
    }
}
