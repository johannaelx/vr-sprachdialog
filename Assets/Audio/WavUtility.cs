using System;
using System.IO;
using UnityEngine;

/// WavUtility
/// Utility class for converting AudioClip to WAV byte array.

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        return ConvertToWav(samples, clip.frequency, clip.channels);
    }

    private static byte[] ConvertToWav(float[] samples, int sampleRate, int channels)
    {
        int dataLength = samples.Length * 2; // 16-bit PCM
        int byteRate = sampleRate * channels * 2;

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);                  // Subchunk size
            writer.Write((short)1);             // Audio format (PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2));
            writer.Write((short)16);            // Bits per sample

            // data subchunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);

            foreach (float sample in samples)
            {
                short pcmSample = (short)Mathf.Clamp(
                    sample * short.MaxValue,
                    short.MinValue,
                    short.MaxValue
                );
                writer.Write(pcmSample);
            }

            return stream.ToArray();
        }
    }   
}
