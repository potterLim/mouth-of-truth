using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Voice
{
    public class AnswerCaptureResult
    {
        public AnswerCaptureResult(AnswerTranscript answerTranscript, AnswerAudioFilePath audioFilePath, VoiceSegmentCount voiceSegmentCount)
        {
            AnswerTranscript = answerTranscript;
            AudioFilePath = audioFilePath;
            VoiceSegmentCount = voiceSegmentCount;
        }

        public AnswerTranscript AnswerTranscript { get; }

        public AnswerAudioFilePath AudioFilePath { get; }

        public VoiceSegmentCount VoiceSegmentCount { get; }
    }
}
