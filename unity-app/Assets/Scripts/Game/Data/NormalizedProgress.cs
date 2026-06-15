using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct NormalizedProgress : IEquatable<NormalizedProgress>
    {
        private readonly float mValue;

        public NormalizedProgress(float value)
        {
            if (value < 0.0f || value > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            mValue = value;
        }

        public static NormalizedProgress Zero => new NormalizedProgress(0.0f);

        public static NormalizedProgress Complete => new NormalizedProgress(1.0f);

        public float Value => mValue;

        public static NormalizedProgress FromUnclamped(float value)
        {
            if (float.IsNaN(value))
            {
                return Zero;
            }

            return new NormalizedProgress(Math.Min(1.0f, Math.Max(0.0f, value)));
        }

        public bool Equals(NormalizedProgress other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is NormalizedProgress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString("0.###");
        }
    }
}
