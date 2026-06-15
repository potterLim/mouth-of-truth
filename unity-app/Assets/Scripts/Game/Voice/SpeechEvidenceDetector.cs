using System;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Voice
{
    public sealed class SpeechEvidenceDetector
    {
        private static readonly SecondsDuration SPEECH_WINDOW_DURATION = new SecondsDuration(0.20f);
        private static readonly AudioRmsLevel SPEECH_ACTIVITY_RMS_THRESHOLD = new AudioRmsLevel(0.0085f);
        private static readonly AudioRmsLevel SPEECH_EVIDENCE_RMS_THRESHOLD = new AudioRmsLevel(0.0145f);
        private static readonly AudioRmsLevel SPEECH_EVIDENCE_PEAK_RMS_THRESHOLD = new AudioRmsLevel(0.0200f);
        private const int MINIMUM_SPEECH_EVIDENCE_WINDOW_COUNT = 4;

        private readonly AudioSampleRate mSampleRate;

        public SpeechEvidenceDetector(AudioSampleRate sampleRate)
        {
            mSampleRate = sampleRate;
        }

        public SecondsDuration SpeechWindowDuration => SPEECH_WINDOW_DURATION;

        public ESpeechDetectionState EvaluateSpeechState(float[] monoSamples)
        {
            AudioRmsLevel rmsLevel = calculateWindowRms(monoSamples, 0, monoSamples == null ? 0 : monoSamples.Length);
            return rmsLevel.IsAtLeast(SPEECH_ACTIVITY_RMS_THRESHOLD)
                ? ESpeechDetectionState.SpeechDetected
                : ESpeechDetectionState.Silent;
        }

        public bool ContainsSpeechEvidence(float[] monoSamples)
        {
            if (monoSamples == null || monoSamples.Length == 0)
            {
                return false;
            }

            int windowSampleCount = Mathf.Max(1, Mathf.CeilToInt(mSampleRate.Value * SPEECH_WINDOW_DURATION.Value));
            int strideSampleCount = Mathf.Max(1, windowSampleCount / 2);

            if (monoSamples.Length <= windowSampleCount)
            {
                AudioRmsLevel singleWindowRmsLevel = calculateWindowRms(monoSamples, 0, monoSamples.Length);
                return singleWindowRmsLevel.IsAtLeast(SPEECH_EVIDENCE_RMS_THRESHOLD)
                    && singleWindowRmsLevel.IsAtLeast(SPEECH_EVIDENCE_PEAK_RMS_THRESHOLD);
            }

            int speechWindowCount = 0;
            AudioRmsLevel peakRmsLevel = AudioRmsLevel.Zero;

            for (int startSampleIndex = 0;
                 startSampleIndex + windowSampleCount <= monoSamples.Length;
                 startSampleIndex += strideSampleCount)
            {
                AudioRmsLevel windowRmsLevel = calculateWindowRms(monoSamples, startSampleIndex, windowSampleCount);
                peakRmsLevel = windowRmsLevel.Value > peakRmsLevel.Value ? windowRmsLevel : peakRmsLevel;

                if (windowRmsLevel.IsAtLeast(SPEECH_EVIDENCE_RMS_THRESHOLD))
                {
                    speechWindowCount += 1;
                }
            }

            int tailWindowStartIndex = Math.Max(0, monoSamples.Length - windowSampleCount);
            int tailSampleCount = monoSamples.Length - tailWindowStartIndex;
            AudioRmsLevel tailWindowRmsLevel = calculateWindowRms(monoSamples, tailWindowStartIndex, tailSampleCount);
            peakRmsLevel = tailWindowRmsLevel.Value > peakRmsLevel.Value ? tailWindowRmsLevel : peakRmsLevel;

            if (tailWindowRmsLevel.IsAtLeast(SPEECH_EVIDENCE_RMS_THRESHOLD))
            {
                speechWindowCount += 1;
            }

            return speechWindowCount >= MINIMUM_SPEECH_EVIDENCE_WINDOW_COUNT
                && peakRmsLevel.IsAtLeast(SPEECH_EVIDENCE_PEAK_RMS_THRESHOLD);
        }

        private static AudioRmsLevel calculateWindowRms(float[] monoSamples, int startSampleIndex, int sampleCount)
        {
            if (monoSamples == null || sampleCount <= 0)
            {
                return AudioRmsLevel.Zero;
            }

            double squaredSum = 0.0d;

            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex += 1)
            {
                float sampleValue = monoSamples[startSampleIndex + sampleIndex];
                squaredSum += sampleValue * sampleValue;
            }

            double meanSquare = squaredSum / sampleCount;
            return new AudioRmsLevel((float)Math.Sqrt(meanSquare));
        }
    }
}
