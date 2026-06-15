using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct QuestionId : IEquatable<QuestionId>
    {
        private readonly string mValue;

        public QuestionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Question id cannot be empty.", nameof(value));
            }

            mValue = value.Trim();
        }

        public static QuestionId Fallback => new QuestionId("question");

        public string Value => mValue ?? string.Empty;

        public bool Equals(QuestionId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionId other && Equals(other);
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
