using System;

namespace MouthOfTruth.Game.Analysis
{
    public readonly struct AnalysisReasonCode : IEquatable<AnalysisReasonCode>
    {
        private const string INSUFFICIENT_FACE_DATA_VALUE = "insufficient_face_data";
        private const string INSUFFICIENT_VOICE_DATA_VALUE = "insufficient_voice_data";

        private readonly string mValue;

        public AnalysisReasonCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Analysis reason code is required.", nameof(value));
            }

            mValue = value;
        }

        public static AnalysisReasonCode InsufficientFaceData => new AnalysisReasonCode(INSUFFICIENT_FACE_DATA_VALUE);

        public static AnalysisReasonCode InsufficientVoiceData => new AnalysisReasonCode(INSUFFICIENT_VOICE_DATA_VALUE);

        public string Value
        {
            get
            {
                if (mValue == null)
                {
                    return string.Empty;
                }

                return mValue;
            }
        }

        public bool Equals(AnalysisReasonCode other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object otherObject)
        {
            return otherObject is AnalysisReasonCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
