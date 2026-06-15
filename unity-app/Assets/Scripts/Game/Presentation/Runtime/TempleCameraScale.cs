using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public readonly struct TempleCameraScale
    {
        public TempleCameraScale(float value)
        {
            Value = Mathf.Max(0.01f, value);
        }

        public float Value { get; }
    }
}
