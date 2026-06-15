using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct QuestionDeckRandomSeed : IEquatable<QuestionDeckRandomSeed>
    {
        public QuestionDeckRandomSeed(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(QuestionDeckRandomSeed other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is QuestionDeckRandomSeed other && Equals(other);
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
