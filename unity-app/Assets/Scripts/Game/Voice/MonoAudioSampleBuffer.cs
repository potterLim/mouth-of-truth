using System;

namespace MouthOfTruth.Game.Voice
{
    internal readonly struct MonoAudioSampleBuffer
    {
        private readonly float[] mSamples;

        public MonoAudioSampleBuffer(float[] samples)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            mSamples = samples;
        }

        public int SampleCount => mSamples == null ? 0 : mSamples.Length;

        public float this[int sampleIndex] => mSamples[sampleIndex];
    }
}
