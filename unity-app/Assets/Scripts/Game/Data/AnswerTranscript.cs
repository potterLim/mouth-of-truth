using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct AnswerTranscript : IEquatable<AnswerTranscript>
    {
        private readonly string mValue;

        public AnswerTranscript(string value)
        {
            mValue = value ?? string.Empty;
        }

        public static AnswerTranscript Empty => new AnswerTranscript(string.Empty);

        public string Value => mValue ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public AnswerTranscript Trimmed()
        {
            return new AnswerTranscript(Value.Trim());
        }

        public bool Equals(AnswerTranscript other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is AnswerTranscript other && Equals(other);
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
