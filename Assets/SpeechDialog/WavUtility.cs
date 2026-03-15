using System;
using System.IO;
using UnityEngine;

/// WavUtility
/// Utility class for converting between AudioClip and WAV byte arrays.
public static class WavUtility
{
    public static byte[] FromSamples(float[] samples, int sampleRate, int channels)
    {
        return ConvertToWav(samples, sampleRate, channels);
    }

    /// Creates an AudioClip from a WAV byte array.
    /// Searches for the "data" chunk to handle variable-length headers.
    public static AudioClip ToAudioClip(byte[] wav, string clipName = "tts_clip")
    {
        int channels = BitConverter.ToInt16(wav, 22);
        int sampleRate = BitConverter.ToInt32(wav, 24);

        // Walk chunks after the RIFF/WAVE header to find "data"
        int offset = 12;
        while (offset < wav.Length - 8)
        {
            string chunkId = System.Text.Encoding.ASCII.GetString(wav, offset, 4);
            int chunkSize = BitConverter.ToInt32(wav, offset + 4);
            offset += 8;
            if (chunkId == "data")
                break;
            offset += chunkSize;
        }

        int sampleCount = (wav.Length - offset) / 2;
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short pcm = BitConverter.ToInt16(wav, offset + i * 2);
            samples[i] = pcm / (float)short.MaxValue;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount / channels, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
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