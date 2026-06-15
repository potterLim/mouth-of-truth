using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct QuestionCategory : IEquatable<QuestionCategory>
    {
        private readonly string mValue;

        public QuestionCategory(string value)
        {
            mValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static QuestionCategory Empty => new QuestionCategory(string.Empty);

        public string Value => mValue ?? string.Empty;

        public bool Equals(QuestionCategory other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionCategory other && Equals(other);
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
