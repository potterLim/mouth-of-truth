using System;

namespace MouthOfTruth.Game.Voice
{
    public readonly struct VoiceSegmentCount : IEquatable<VoiceSegmentCount>
    {
        public VoiceSegmentCount(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Value = value;
        }

        public static VoiceSegmentCount Zero => new VoiceSegmentCount(0);

        public int Value
        {
            get;
        }

        public bool Equals(VoiceSegmentCount other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object otherObject)
        {
            return otherObject is VoiceSegmentCount other && Equals(other);
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
