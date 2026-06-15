using System;

namespace MouthOfTruth.Game.Data
{
    internal readonly struct QuestionPoolFilePath : IEquatable<QuestionPoolFilePath>
    {
        private readonly string mValue;

        public QuestionPoolFilePath(string value)
        {
            mValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
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

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(QuestionPoolFilePath other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionPoolFilePath other && Equals(other);
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
