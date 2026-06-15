using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct AnswerTranscriptPlaceholderText : IEquatable<AnswerTranscriptPlaceholderText>
    {
        private readonly string mValue;

        public AnswerTranscriptPlaceholderText(string value)
        {
            if (value == null)
            {
                mValue = string.Empty;
                return;
            }

            mValue = value;
        }

        public static AnswerTranscriptPlaceholderText Empty => new AnswerTranscriptPlaceholderText(string.Empty);

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

        public bool Equals(AnswerTranscriptPlaceholderText other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object otherObject)
        {
            return otherObject is AnswerTranscriptPlaceholderText other && Equals(other);
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
