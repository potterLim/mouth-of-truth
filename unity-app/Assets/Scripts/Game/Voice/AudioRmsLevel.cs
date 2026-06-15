using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct AudioRmsLevel : IEquatable<AudioRmsLevel>
    {
        public AudioRmsLevel(float value)
        {
            if (float.IsNaN(value) || value < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public static AudioRmsLevel Zero => new AudioRmsLevel(0.0f);

        public float Value { get; }

        public bool IsAtLeast(AudioRmsLevel threshold)
        {
            return Value >= threshold.Value;
        }

        public bool Equals(AudioRmsLevel other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioRmsLevel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString("0.####");
        }
    }
}
