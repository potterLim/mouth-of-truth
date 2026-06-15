using System;

namespace MouthOfTruth.Game.Face
{
    public readonly struct FaceFrameCount : IEquatable<FaceFrameCount>
    {
        public FaceFrameCount(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public static FaceFrameCount Zero => new FaceFrameCount(0);

        public int Value
        {
            get;
        }

        public bool Equals(FaceFrameCount other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is FaceFrameCount other && Equals(other);
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
