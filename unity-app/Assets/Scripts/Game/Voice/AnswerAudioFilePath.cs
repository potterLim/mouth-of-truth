using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct AnswerAudioFilePath : IEquatable<AnswerAudioFilePath>
    {
        private readonly string mValue;

        public AnswerAudioFilePath(string value)
        {
            mValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public static AnswerAudioFilePath Empty => new AnswerAudioFilePath(string.Empty);

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

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(AnswerAudioFilePath other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is AnswerAudioFilePath other && Equals(other);
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
