using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct QuestionDifficulty : IEquatable<QuestionDifficulty>
    {
        public QuestionDifficulty(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public int Value
        {
            get;
        }

        public bool Equals(QuestionDifficulty other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionDifficulty other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
