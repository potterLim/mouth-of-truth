using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct AudioSampleRate : IEquatable<AudioSampleRate>
    {
        public AudioSampleRate(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Audio sample rate must be positive.");
            }

            Value = value;
        }

        public int Value
        {
            get;
        }

        public bool Equals(AudioSampleRate other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioSampleRate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
