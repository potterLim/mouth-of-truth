using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct AudioSampleCount : IEquatable<AudioSampleCount>
    {
        public AudioSampleCount(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public static AudioSampleCount Zero => new AudioSampleCount(0);

        public int Value { get; }

        public bool Equals(AudioSampleCount other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioSampleCount other && Equals(other);
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
