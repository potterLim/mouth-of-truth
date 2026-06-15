using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Voice
{
    public class MicrophoneAnswerInputAdapter : IAnswerCaptureInputAdapter
    {
        private static readonly AudioSampleRate MICROPHONE_SAMPLE_RATE = new AudioSampleRate(16000);
        private const int MAX_SEGMENT_DURATION_SECONDS = 20;

        private enum ERecordingStopMode
        {
            DiscardActiveSegment,
            PreserveActiveSegment,
        }

        private readonly SpeechEvidenceDetector mSpeechEvidenceDetector = new SpeechEvidenceDetector(MICROPHONE_SAMPLE_RATE);
        private readonly List<float[]> mRecordedSegments = new List<float[]>();

        private AudioClip mActiveRecordingClip;
        private string mSelectedDeviceName;
        private bool mIsCollecting;
        private bool mIsMicrophoneRunning;
        private int mSegmentStartSamplePosition;
        private int mRecordedSegmentCount;

        public MicrophoneAnswerInputAdapter()
        {
            mSelectedDeviceName = selectDefaultDeviceName();
        }

        public EAnswerTranscriptInputMode TranscriptInputMode => EAnswerTranscriptInputMode.AutomaticCapture;

        public AnswerTranscriptPlaceholderText TranscriptPlaceholderText =>
            new AnswerTranscriptPlaceholderText("음성 입력이 자동으로 수집됩니다.");

        public void Reset()
        {
            stopCurrentRecording(ERecordingStopMode.DiscardActiveSegment);
            mRecordedSegments.Clear();
            mRecordedSegmentCount = 0;
        }

        public void PrepareAudioSession()
        {
            startMicrophoneIfNeeded();
        }

        public void BeginCollection()
        {
            startNewRecordingSegment();
        }

        public void PauseCollection()
        {
            stopCurrentRecording(ERecordingStopMode.PreserveActiveSegment);
        }

        public void ResumeCollection()
        {
            startNewRecordingSegment();
        }

        public void CancelCollection()
        {
            Reset();
            stopMicrophoneIfRunning();
        }

        public AnswerCaptureFrameSnapshot Update(SecondsDuration deltaTime)
        {
            _ = deltaTime;
            ESpeechDetectionState speechDetectionState = mIsCollecting
                ? evaluateCurrentSpeechState()
                : ESpeechDetectionState.Silent;
            return new AnswerCaptureFrameSnapshot(AnswerTranscript.Empty, speechDetectionState);
        }

        public Task<AnswerCaptureResult> CompleteCollectionAsync(QuestionId questionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stopCurrentRecording(ERecordingStopMode.PreserveActiveSegment);

            if (mRecordedSegments.Count == 0)
            {
                return Task.FromResult(new AnswerCaptureResult(AnswerTranscript.Empty, AnswerAudioFilePath.Empty, VoiceSegmentCount.Zero));
            }

            int totalSampleCount = 0;

            foreach (float[] segmentSamples in mRecordedSegments)
            {
                totalSampleCount += segmentSamples.Length;
            }

            float[] mergedSamples = new float[totalSampleCount];
            int nextOffset = 0;

            foreach (float[] segmentSamples in mRecordedSegments)
            {
                Array.Copy(segmentSamples, 0, mergedSamples, nextOffset, segmentSamples.Length);
                nextOffset += segmentSamples.Length;
            }

            AnswerAudioFilePath audioFilePath = AnswerAudioWorkspacePaths.BuildAudioFilePath(questionId);
            WaveFileWriter.WriteMono16BitPcm(audioFilePath, new MonoAudioSampleBuffer(mergedSamples), MICROPHONE_SAMPLE_RATE);

            return Task.FromResult(new AnswerCaptureResult(AnswerTranscript.Empty, audioFilePath, new VoiceSegmentCount(mRecordedSegmentCount)));
        }

        public bool HasAvailableDevice()
        {
            return string.IsNullOrWhiteSpace(mSelectedDeviceName) == false;
        }

        private void startNewRecordingSegment()
        {
            if (HasAvailableDevice() == false)
            {
                throw new InvalidOperationException("No microphone input device is available.");
            }

            if (mIsCollecting)
            {
                return;
            }

            startMicrophoneIfNeeded();
            mSegmentStartSamplePosition = Mathf.Clamp(Microphone.GetPosition(mSelectedDeviceName), 0, mActiveRecordingClip.samples);
            mIsCollecting = true;
        }

        private void startMicrophoneIfNeeded()
        {
            if (HasAvailableDevice() == false)
            {
                throw new InvalidOperationException("No microphone input device is available.");
            }

            if (mIsMicrophoneRunning && mActiveRecordingClip != null)
            {
                return;
            }

            mActiveRecordingClip = Microphone.Start(mSelectedDeviceName, true, MAX_SEGMENT_DURATION_SECONDS, MICROPHONE_SAMPLE_RATE.Value);

            if (mActiveRecordingClip == null)
            {
                mIsMicrophoneRunning = false;
                throw new InvalidOperationException($"Failed to start microphone capture for device '{mSelectedDeviceName}'.");
            }

            mIsMicrophoneRunning = true;
        }

        private void stopMicrophoneIfRunning()
        {
            if (mIsMicrophoneRunning == false || string.IsNullOrWhiteSpace(mSelectedDeviceName))
            {
                return;
            }

            Microphone.End(mSelectedDeviceName);
            mActiveRecordingClip = null;
            mIsMicrophoneRunning = false;
            mIsCollecting = false;
            mSegmentStartSamplePosition = 0;
        }

        private void stopCurrentRecording(ERecordingStopMode recordingStopMode)
        {
            if (mIsCollecting == false || string.IsNullOrWhiteSpace(mSelectedDeviceName))
            {
                return;
            }

            float[] activeSegmentSamples = recordingStopMode == ERecordingStopMode.PreserveActiveSegment
                ? readActiveSegmentSamples()
                : Array.Empty<float>();

            mIsCollecting = false;

            if (activeSegmentSamples.Length == 0)
            {
                return;
            }

            if (mSpeechEvidenceDetector.ContainsSpeechEvidence(activeSegmentSamples) == false)
            {
                return;
            }

            mRecordedSegments.Add(activeSegmentSamples);
            mRecordedSegmentCount += 1;
        }

        private float[] readActiveSegmentSamples()
        {
            if (mActiveRecordingClip == null || string.IsNullOrWhiteSpace(mSelectedDeviceName))
            {
                return Array.Empty<float>();
            }

            AudioSamplePosition currentSamplePosition = new AudioSamplePosition(Mathf.Clamp(Microphone.GetPosition(mSelectedDeviceName), 0, mActiveRecordingClip.samples));
            AudioSamplePosition segmentStartSamplePosition = new AudioSamplePosition(Mathf.Clamp(mSegmentStartSamplePosition, 0, mActiveRecordingClip.samples));
            AudioSampleCount recordedSampleCount = LoopedAudioClipReader.CalculateLoopedSampleDistance(segmentStartSamplePosition, currentSamplePosition, new AudioSampleCount(mActiveRecordingClip.samples));

            if (recordedSampleCount.Value <= 0)
            {
                return Array.Empty<float>();
            }

            return LoopedAudioClipReader.ReadMonoSamples(mActiveRecordingClip, segmentStartSamplePosition, recordedSampleCount);
        }

        private ESpeechDetectionState evaluateCurrentSpeechState()
        {
            if (mActiveRecordingClip == null || mIsMicrophoneRunning == false || string.IsNullOrWhiteSpace(mSelectedDeviceName))
            {
                return ESpeechDetectionState.Silent;
            }

            AudioSamplePosition currentSamplePosition = new AudioSamplePosition(Mathf.Clamp(Microphone.GetPosition(mSelectedDeviceName), 0, mActiveRecordingClip.samples));
            AudioSampleCount availableSampleCount = LoopedAudioClipReader.CalculateLoopedSampleDistance(new AudioSamplePosition(mSegmentStartSamplePosition), currentSamplePosition, new AudioSampleCount(mActiveRecordingClip.samples));
            AudioSampleCount windowSampleCount = new AudioSampleCount(Mathf.Min(availableSampleCount.Value, Mathf.CeilToInt(MICROPHONE_SAMPLE_RATE.Value * mSpeechEvidenceDetector.SpeechWindowDuration.Value)));

            if (windowSampleCount.Value <= 0)
            {
                return ESpeechDetectionState.Silent;
            }

            int startSampleOffset = currentSamplePosition.Value - windowSampleCount.Value;

            if (startSampleOffset < 0)
            {
                startSampleOffset += mActiveRecordingClip.samples;
            }

            float[] activeSpeechSamples = LoopedAudioClipReader.ReadMonoSamples(mActiveRecordingClip, new AudioSamplePosition(startSampleOffset), windowSampleCount);
            return mSpeechEvidenceDetector.EvaluateSpeechState(activeSpeechSamples);
        }

        private string selectDefaultDeviceName()
        {
            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                return string.Empty;
            }

            return Microphone.devices[0];
        }
    }
}
