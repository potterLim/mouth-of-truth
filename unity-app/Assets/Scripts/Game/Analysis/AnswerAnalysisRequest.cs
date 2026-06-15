using System;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Voice;

namespace MouthOfTruth.Game.Analysis
{
    public class AnswerAnalysisRequest
    {
        public AnswerAnalysisRequest(QuestionDefinition questionDefinition, AnswerTranscript answerTranscript, AnswerAudioFilePath answerAudioFilePath, FaceFramesDirectoryPath faceFramesDirectoryPath, FaceFrameCount faceFrameCount, VoiceSegmentCount voiceSegmentCount)
        {
            if (questionDefinition == null)
            {
                throw new ArgumentNullException(nameof(questionDefinition));
            }

            QuestionDefinition = questionDefinition;
            AnswerTranscript = answerTranscript;
            AnswerAudioFilePath = answerAudioFilePath;
            FaceFramesDirectoryPath = faceFramesDirectoryPath;
            FaceFrameCount = faceFrameCount;
            VoiceSegmentCount = voiceSegmentCount;
        }

        public QuestionDefinition QuestionDefinition { get; }

        public AnswerTranscript AnswerTranscript { get; }

        public AnswerAudioFilePath AnswerAudioFilePath { get; }

        public FaceFramesDirectoryPath FaceFramesDirectoryPath { get; }

        public FaceFrameCount FaceFrameCount { get; }

        public VoiceSegmentCount VoiceSegmentCount { get; }
    }
}
