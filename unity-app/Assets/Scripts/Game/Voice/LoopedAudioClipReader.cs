using System;
using UnityEngine;

namespace MouthOfTruth.Game.Voice
{
    public static class LoopedAudioClipReader
    {
        public static float[] ReadMonoSamples(AudioClip audioClip, AudioSamplePosition startSamplePosition, AudioSampleCount sampleCount)
        {
            if (audioClip == null || sampleCount.Value <= 0)
            {
                return Array.Empty<float>();
            }

            int channels = Mathf.Max(1, audioClip.channels);
            int clipSampleCount = audioClip.samples;

            if (clipSampleCount <= 0)
            {
                return Array.Empty<float>();
            }

            int remainingSampleCount = Mathf.Min(sampleCount.Value, clipSampleCount);
            int readSamplePosition = Mathf.Clamp(startSamplePosition.Value, 0, clipSampleCount - 1);
            int outputSampleOffset = 0;
            float[] monoSamples = new float[remainingSampleCount];

            while (remainingSampleCount > 0)
            {
                int chunkSampleCount = Math.Min(remainingSampleCount, clipSampleCount - readSamplePosition);
                float[] interleavedSamples = new float[chunkSampleCount * channels];
                audioClip.GetData(interleavedSamples, readSamplePosition);

                for (int sampleIndex = 0; sampleIndex < chunkSampleCount; sampleIndex += 1)
                {
                    float mixedValue = 0.0f;

                    for (int channelIndex = 0; channelIndex < channels; channelIndex += 1)
                    {
                        mixedValue += interleavedSamples[(sampleIndex * channels) + channelIndex];
                    }

                    monoSamples[outputSampleOffset + sampleIndex] = mixedValue / channels;
                }

                outputSampleOffset += chunkSampleCount;
                remainingSampleCount -= chunkSampleCount;
                readSamplePosition = 0;
            }

            return monoSamples;
        }

        public static AudioSampleCount CalculateLoopedSampleDistance(
            AudioSamplePosition startSamplePosition,
            AudioSamplePosition endSamplePosition,
            AudioSampleCount clipSampleCount)
        {
            if (clipSampleCount.Value <= 0)
            {
                return AudioSampleCount.Zero;
            }

            int clampedStartSamplePosition = Mathf.Clamp(startSamplePosition.Value, 0, clipSampleCount.Value);
            int clampedEndSamplePosition = Mathf.Clamp(endSamplePosition.Value, 0, clipSampleCount.Value);

            if (clampedEndSamplePosition >= clampedStartSamplePosition)
            {
                return new AudioSampleCount(clampedEndSamplePosition - clampedStartSamplePosition);
            }

            return new AudioSampleCount((clipSampleCount.Value - clampedStartSamplePosition) + clampedEndSamplePosition);
        }
    }
}
