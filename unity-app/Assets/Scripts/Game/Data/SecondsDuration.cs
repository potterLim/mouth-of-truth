using System;

namespace MouthOfTruth.Game.Data
{
    public readonly struct SecondsDuration : IEquatable<SecondsDuration>
    {
        public SecondsDuration(float value)
        {
            if (value < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public static SecondsDuration Zero => new SecondsDuration(0.0f);

        public float Value
        {
            get;
        }

        public SecondsDuration Add(SecondsDuration duration)
        {
            return new SecondsDuration(Value + duration.Value);
        }

        public bool Equals(SecondsDuration other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is SecondsDuration other && Equals(other);
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
