// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

public class PcmAudioGenerator
{
    public int SampleRate => 8000; // 8 kHz
    public int BitsPerSample => 16; // 16-bit PCM
    public int Channels => 1; // Mono

    private const double s_maxAmplitude = 32767.0; // Max value for 16-bit PCM

    public byte[] GenerateSineWave(double frequency, double durationSeconds, double amplitude = 0.5)
    {
        int totalSamples = (int)(SampleRate * durationSeconds);
        int bytesPerSample = BitsPerSample / 8;
        byte[] pcmData = new byte[totalSamples * bytesPerSample * Channels];

        amplitude = Clamp(amplitude, 0.0, 1.0) * s_maxAmplitude;

        for (int i = 0; i < totalSamples; i++)
        {
            double time = i / (double)SampleRate;
            double sampleValue = amplitude * Math.Sin(2 * Math.PI * frequency * time);

            short sampleShort = (short)sampleValue;
            int sampleIndex = i * bytesPerSample * Channels;

            // Write 16-bit PCM data (little-endian)
            pcmData[sampleIndex] = (byte)(sampleShort & 0xFF);
            pcmData[sampleIndex + 1] = (byte)((sampleShort >> 8) & 0xFF);
        }

        return pcmData;

        static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
    }
}