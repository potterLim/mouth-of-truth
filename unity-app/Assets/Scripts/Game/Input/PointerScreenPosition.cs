using UnityEngine;

namespace MouthOfTruth.Game.Input
{
    public readonly struct PointerScreenPosition
    {
        public PointerScreenPosition(Vector2 value)
        {
            Value = value;
        }

        public Vector2 Value { get; }
    }
}
