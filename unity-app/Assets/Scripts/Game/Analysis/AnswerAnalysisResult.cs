using System;
using System.Collections.Generic;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Analysis
{
    public class AnswerAnalysisResult
    {
        public AnswerAnalysisResult(EVerdictKind verdictKind, AnswerTranscript answerTranscript, IReadOnlyList<string> reasonCodes)
        {
            VerdictKind = verdictKind;
            AnswerTranscript = answerTranscript;
            ReasonCodes = reasonCodes == null ? Array.Empty<string>() : reasonCodes;
        }

        public EVerdictKind VerdictKind { get; }

        public AnswerTranscript AnswerTranscript { get; }

        public IReadOnlyList<string> ReasonCodes { get; }
    }
}
