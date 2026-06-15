using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct QuestionText : IEquatable<QuestionText>
    {
        private readonly string mValue;

        public QuestionText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Question text cannot be empty.", nameof(value));
            }

            mValue = value.Trim();
        }

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

        public bool Equals(QuestionText other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionText other && Equals(other);
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
