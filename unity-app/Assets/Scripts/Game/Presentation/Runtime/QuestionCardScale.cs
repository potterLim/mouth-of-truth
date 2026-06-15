using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public readonly struct QuestionCardScale
    {
        public QuestionCardScale(float horizontal, float vertical)
        {
            Horizontal = Mathf.Max(0.01f, horizontal);
            Vertical = Mathf.Max(0.01f, vertical);
        }

        public float Horizontal { get; }

        public float Vertical { get; }

        public static QuestionCardScale Uniform(float value)
        {
            return new QuestionCardScale(value, value);
        }
    }
}
