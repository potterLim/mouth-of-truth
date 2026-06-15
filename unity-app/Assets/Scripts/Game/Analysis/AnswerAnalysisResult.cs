using System;
using System.Collections.Generic;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Analysis
{
    public class AnswerAnalysisResult
    {
        public AnswerAnalysisResult(EVerdictKind verdictKind, AnswerTranscript answerTranscript, IReadOnlyList<AnalysisReasonCode> reasonCodes)
        {
            VerdictKind = verdictKind;
            AnswerTranscript = answerTranscript;
            ReasonCodes = reasonCodes == null ? Array.Empty<AnalysisReasonCode>() : reasonCodes;
        }

        public EVerdictKind VerdictKind { get; }

        public AnswerTranscript AnswerTranscript { get; }

        public IReadOnlyList<AnalysisReasonCode> ReasonCodes { get; }
    }
}
