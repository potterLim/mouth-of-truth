using System;

namespace MouthOfTruth.Game.Face
{
    public readonly struct FaceFramesDirectoryPath : IEquatable<FaceFramesDirectoryPath>
    {
        private readonly string mValue;

        public FaceFramesDirectoryPath(string value)
        {
            mValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public static FaceFramesDirectoryPath Empty => new FaceFramesDirectoryPath(string.Empty);

        public string Value => mValue ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(FaceFramesDirectoryPath other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is FaceFramesDirectoryPath other && Equals(other);
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
