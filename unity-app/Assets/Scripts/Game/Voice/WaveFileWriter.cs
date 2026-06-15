using System;
using System.IO;
using UnityEngine;

namespace MouthOfTruth.Game.Voice
{
    internal static class WaveFileWriter
    {
        internal static void WriteMono16BitPcm(AnswerAudioFilePath outputFilePath, float[] monoSamples, AudioSampleRate sampleRate)
        {
            if (outputFilePath.IsEmpty)
            {
                throw new ArgumentException("Output file path is required.", nameof(outputFilePath));
            }

            if (monoSamples == null)
            {
                throw new ArgumentNullException(nameof(monoSamples));
            }

            string directoryPath = Path.GetDirectoryName(outputFilePath.Value);

            if (string.IsNullOrWhiteSpace(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (FileStream fileStream = new FileStream(outputFilePath.Value, FileMode.Create, FileAccess.Write))
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                int bytesPerSample = sizeof(short);
                int channelCount = 1;
                int byteRate = sampleRate.Value * channelCount * bytesPerSample;
                int dataChunkSize = monoSamples.Length * bytesPerSample;

                binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                binaryWriter.Write(36 + dataChunkSize);
                binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                binaryWriter.Write(16);
                binaryWriter.Write((short)1);
                binaryWriter.Write((short)channelCount);
                binaryWriter.Write(sampleRate.Value);
                binaryWriter.Write(byteRate);
                binaryWriter.Write((short)(channelCount * bytesPerSample));
                binaryWriter.Write((short)(bytesPerSample * 8));
                binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                binaryWriter.Write(dataChunkSize);

                foreach (float sample in monoSamples)
                {
                    short pcmSample = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
                    binaryWriter.Write(pcmSample);
                }
            }

        }
    }
}
