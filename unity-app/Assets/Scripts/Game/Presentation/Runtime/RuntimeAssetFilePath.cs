using System;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    internal readonly struct RuntimeAssetFilePath : IEquatable<RuntimeAssetFilePath>
    {
        private readonly string mValue;

        public RuntimeAssetFilePath(string value)
        {
            mValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public static RuntimeAssetFilePath Empty => new RuntimeAssetFilePath(string.Empty);

        public string Value
        {
            get
            {
                if (mValue == null)
                {
                    return string.Empty;
                }

                return mValue;
            }
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(RuntimeAssetFilePath other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeAssetFilePath other && Equals(other);
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
