using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct AudioSamplePosition : IEquatable<AudioSamplePosition>
    {
        public AudioSamplePosition(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public int Value { get; }

        public bool Equals(AudioSamplePosition other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioSamplePosition other && Equals(other);
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
