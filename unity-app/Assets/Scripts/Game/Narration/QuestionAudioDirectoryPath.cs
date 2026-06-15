using System;

namespace MouthOfTruth.Game.Narration
{
    public readonly struct QuestionAudioDirectoryPath : IEquatable<QuestionAudioDirectoryPath>
    {
        public QuestionAudioDirectoryPath(string value)
        {
            Value = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public static QuestionAudioDirectoryPath Empty => new QuestionAudioDirectoryPath(string.Empty);

        public string Value
        {
            get;
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(QuestionAudioDirectoryPath other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionAudioDirectoryPath other && Equals(other);
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
