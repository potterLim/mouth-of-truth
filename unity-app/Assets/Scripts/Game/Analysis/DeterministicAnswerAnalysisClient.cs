using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Analysis
{
    public class DeterministicAnswerAnalysisClient : IAnswerAnalysisClient
    {
        private const int MINIMUM_FACE_RECOGNITION_COUNT = 4;
        private const int MINIMUM_VOICE_SEGMENT_COUNT = 1;

        public Task WarmUpAsync(CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<AnswerAnalysisResult> AnalyzeAsync(AnswerAnalysisRequest answerAnalysisRequest, CancellationToken cancellationToken)
        {
            if (answerAnalysisRequest == null)
            {
                throw new ArgumentNullException(nameof(answerAnalysisRequest));
            }

            _ = cancellationToken;

            bool hasFaceSignal = answerAnalysisRequest.FaceFrameCount.Value >= MINIMUM_FACE_RECOGNITION_COUNT;
            bool hasVoiceSignal = answerAnalysisRequest.VoiceSegmentCount.Value >= MINIMUM_VOICE_SEGMENT_COUNT;
            List<AnalysisReasonCode> reasonCodes = new List<AnalysisReasonCode>();

            if (hasFaceSignal == false)
            {
                reasonCodes.Add(AnalysisReasonCode.InsufficientFaceData);
            }

            if (hasVoiceSignal == false)
            {
                reasonCodes.Add(AnalysisReasonCode.InsufficientVoiceData);
            }

            if (hasFaceSignal == false || hasVoiceSignal == false)
            {
                return Task.FromResult(new AnswerAnalysisResult(EVerdictKind.Uncertain, answerAnalysisRequest.AnswerTranscript, reasonCodes));
            }

            int paritySeed = calculateStableParitySeed(answerAnalysisRequest.QuestionDefinition.Id, answerAnalysisRequest.AnswerTranscript);

            EVerdictKind verdictKind = paritySeed % 2 == 0 ? EVerdictKind.True : EVerdictKind.False;

            return Task.FromResult(new AnswerAnalysisResult(verdictKind, answerAnalysisRequest.AnswerTranscript, reasonCodes));
        }

        private int calculateStableParitySeed(QuestionId questionId, AnswerTranscript answerTranscript)
        {
            string combinedText = $"{questionId.Value}|{answerTranscript.Value.Trim()}";
            int checksum = 0;

            foreach (char character in combinedText)
            {
                checksum += character;
            }

            return checksum;
        }
    }
}
