using System;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private async Task animateOverTimeAsync(SecondsDuration duration, Action<NormalizedProgress> updateAction)
        {
            if (duration.Value <= 0.0f)
            {
                updateAction?.Invoke(NormalizedProgress.Complete);
                return;
            }

            float elapsedSeconds = 0.0f;

            while (elapsedSeconds < duration.Value)
            {
                elapsedSeconds += Time.deltaTime;
                updateAction?.Invoke(NormalizedProgress.FromUnclamped(elapsedSeconds / duration.Value));
                await Task.Yield();
            }

            updateAction?.Invoke(NormalizedProgress.Complete);
        }

        private float easeOut(NormalizedProgress progress)
        {
            return easeOut(progress.Value);
        }

        private static float easeOut(float progress)
        {
            float inverse = 1.0f - progress;
            return 1.0f - (inverse * inverse * inverse);
        }

        private float easeIn(NormalizedProgress progress)
        {
            return easeIn(progress.Value);
        }

        private static float easeIn(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            return clampedProgress * clampedProgress * clampedProgress;
        }

        private static float easeInOut(NormalizedProgress progress)
        {
            return easeInOut(progress.Value);
        }

        private static float easeInOut(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            return clampedProgress * clampedProgress * (3.0f - (2.0f * clampedProgress));
        }
    }
}
