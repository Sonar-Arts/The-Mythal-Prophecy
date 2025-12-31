using System;
using System.IO;

namespace TheMythalProphecy.Tools;

/// <summary>
/// Generates procedural audio files for the startup animation.
/// Run GenerateStartupAudio() once to create the WAV files.
/// </summary>
public static class AudioGenerator
{
    private const int SampleRate = 44100;
    private const int BitsPerSample = 16;
    private const int Channels = 1;

    /// <summary>
    /// Generates both startup audio files.
    /// Call this once during development to create the audio assets.
    /// </summary>
    public static void GenerateStartupAudio(string contentPath)
    {
        var audioPath = Path.Combine(contentPath, "Audio", "SFX");
        Directory.CreateDirectory(audioPath);

        GenerateSonarSound(Path.Combine(audioPath, "StartupSonar.wav"));
        GenerateUnderwaterAmbient(Path.Combine(audioPath, "UnderwaterAmbient.wav"));
    }

    /// <summary>
    /// Generates a single retro-style sonar ping with triangle waves and bit-crushed character.
    /// Played each time a sonar ring spawns for perfect audio-visual sync.
    /// </summary>
    private static void GenerateSonarSound(string filePath)
    {
        // Single ping duration (played on each ring spawn)
        float duration = 0.5f;
        int numSamples = (int)(SampleRate * duration);
        var samples = new short[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;
            float sample = 0f;

            // Retro-style low frequency ping (around 150-180 Hz)
            float baseFreq = 160f;

            // Stepped frequency sweep (retro pitch bend effect)
            float freqSweep = baseFreq * (1.0f - MathF.Floor(t * 8f) / 32f);

            // Triangle wave - classic retro sound, softer than square
            float phase = (freqSweep * t) % 1f;
            float triangleWave = 4f * MathF.Abs(phase - 0.5f) - 1f;

            // Add a softer sub-octave triangle for depth
            float subPhase = (freqSweep * 0.5f * t) % 1f;
            float subTriangle = (4f * MathF.Abs(subPhase - 0.5f) - 1f) * 0.25f;

            // Retro-style envelope (snappier attack, stepped decay)
            float attack = MathF.Min(t / 0.01f, 1.0f); // 10ms attack
            float decayStep = MathF.Floor(t * 12f) / 12f; // Stepped decay
            float decay = MathF.Max(0f, 1f - decayStep * 2.8f);
            float envelope = attack * decay;

            // Combine waves
            sample = (triangleWave + subTriangle) * envelope * 0.4f;

            // Bit-crush effect (reduce to ~12-bit feel)
            sample = MathF.Floor(sample * 32f) / 32f;

            // Soft limiter
            sample = MathF.Max(-0.9f, MathF.Min(0.9f, sample));

            samples[i] = (short)(sample * 26000);
        }

        WriteWavFile(filePath, samples);
    }

    /// <summary>
    /// Generates retro-style underwater ambient sound.
    /// Uses triangle/square waves and stepped modulation for a 16-bit era feel.
    /// </summary>
    private static void GenerateUnderwaterAmbient(string filePath)
    {
        // 5 second seamless loop
        float duration = 5.0f;
        int numSamples = (int)(SampleRate * duration);
        var samples = new short[numSamples];

        var random = new Random(123);

        // Pre-generate retro noise (quantized)
        var noiseBuffer = new float[numSamples / 8]; // Lower sample rate noise
        for (int i = 0; i < noiseBuffer.Length; i++)
        {
            noiseBuffer[i] = MathF.Floor((float)(random.NextDouble() * 8)) / 4f - 1f;
        }

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / SampleRate;
            float sample = 0f;

            // === Retro deep rumble (triangle waves at low frequency) ===
            float rumblePhase1 = (12f * t) % 1f;
            float rumble1 = (4f * MathF.Abs(rumblePhase1 - 0.5f) - 1f) * 0.12f;

            float rumblePhase2 = (18f * t + 0.3f) % 1f;
            float rumble2 = (4f * MathF.Abs(rumblePhase2 - 0.5f) - 1f) * 0.08f;

            // Slow pulsing modulation (stepped for retro feel)
            float pulseStep = MathF.Floor(t * 4f) / 4f;
            float pulseMod = 0.7f + 0.3f * MathF.Sin(2 * MathF.PI * 0.5f * pulseStep);

            sample += (rumble1 + rumble2) * pulseMod;

            // === Retro filtered noise (sample-and-hold style) ===
            int noiseIdx = (i / 8) % noiseBuffer.Length;
            float retroNoise = noiseBuffer[noiseIdx] * 0.08f;

            // Slow amplitude modulation on noise
            float noiseMod = 0.5f + 0.5f * MathF.Sin(2 * MathF.PI * 0.2f * t);
            sample += retroNoise * noiseMod;

            // === Retro "sonar sweep" undertone ===
            // Slow ascending arpeggio-like pattern
            float sweepTime = t % 2.5f;
            int sweepNote = (int)(sweepTime * 2f) % 4;
            float[] sweepFreqs = { 30f, 35f, 40f, 35f };
            float sweepPhase = (sweepFreqs[sweepNote] * t) % 1f;
            float sweepWave = (4f * MathF.Abs(sweepPhase - 0.5f) - 1f) * 0.06f;
            sample += sweepWave;

            // === Retro bubble blips (square wave pops) ===
            float bubbleTime = (t * 0.8f) % 1f;
            if (bubbleTime < 0.015f)
            {
                // Quick square wave blip
                float blipFreq = 280f + MathF.Floor(t * 3f) % 4 * 40f;
                float blipPhase = (blipFreq * bubbleTime) % 1f;
                float blip = blipPhase < 0.5f ? 0.15f : -0.15f;

                // Quick decay
                float blipEnv = 1f - bubbleTime / 0.015f;
                sample += blip * blipEnv * 0.5f;
            }

            // === Deep bass drone (pulse wave) ===
            float dronePhase = (25f * t) % 1f;
            float dronePulseWidth = 0.3f + 0.1f * MathF.Sin(2 * MathF.PI * 0.15f * t);
            float drone = dronePhase < dronePulseWidth ? 0.08f : -0.08f;
            sample += drone;

            // Bit-crush for retro character
            sample = MathF.Floor(sample * 24f) / 24f;

            // Soft limiter
            sample = MathF.Max(-0.85f, MathF.Min(0.85f, sample));

            samples[i] = (short)(sample * 18000);
        }

        // Crossfade the loop endpoints for seamless looping
        int fadeLength = SampleRate / 10; // 100ms crossfade
        for (int i = 0; i < fadeLength; i++)
        {
            float fadeOut = 1f - (float)i / fadeLength;
            float fadeIn = (float)i / fadeLength;

            int endIdx = numSamples - fadeLength + i;
            samples[i] = (short)(samples[i] * fadeIn + samples[endIdx] * fadeOut);
        }

        WriteWavFile(filePath, samples);
    }

    private static void WriteWavFile(string filePath, short[] samples)
    {
        using var stream = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        int byteRate = SampleRate * Channels * BitsPerSample / 8;
        int blockAlign = Channels * BitsPerSample / 8;
        int dataSize = samples.Length * blockAlign;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize); // File size - 8
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // Chunk size
        writer.Write((short)1); // PCM format
        writer.Write((short)Channels);
        writer.Write(SampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)BitsPerSample);

        // data chunk
        writer.Write("data"u8);
        writer.Write(dataSize);

        foreach (var sample in samples)
        {
            writer.Write(sample);
        }
    }
}
