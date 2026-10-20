using System;
using Godot;

namespace HanoiTowers.Core;

public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void PlayPickup()
    {
        if (!GameProgress.Current.SoundEnabled) return;
        PlayTone(480f, 0.08f, 0.15f, ToneType.Sine);
    }

    public void PlayDrop()
    {
        if (!GameProgress.Current.SoundEnabled) return;
        PlayTone(320f, 0.10f, 0.2f, ToneType.Sine);
    }

    public void PlayError()
    {
        if (!GameProgress.Current.SoundEnabled) return;
        PlayTone(160f, 0.18f, 0.25f, ToneType.Saw);
    }

    public void PlayStar()
    {
        if (!GameProgress.Current.SoundEnabled) return;
        PlayTone(880f, 0.25f, 0.2f, ToneType.Sine);
    }

    public void PlayVictory()
    {
        if (!GameProgress.Current.SoundEnabled) return;
        // Simple arpeggio
        PlayToneSequence(new[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.12f);
    }

    private enum ToneType { Sine, Saw }

    private void PlayTone(float frequency, float duration, float volume, ToneType type)
    {
        int sampleRate = 22050;
        int frameCount = (int)(sampleRate * duration);
        var pcm = new byte[frameCount * 2];

        for (int i = 0; i < frameCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / frameCount;
            float envelope = 1.0f - progress; // Linear fade-out

            float sample = 0;
            if (type == ToneType.Sine)
            {
                sample = MathF.Sin(2.0f * MathF.PI * frequency * t);
            }
            else
            {
                sample = (2.0f * ((frequency * t) % 1.0f)) - 1.0f;
            }

            short intSample = (short)(sample * envelope * volume * 32767);
            pcm[i * 2] = (byte)(intSample & 0xFF);
            pcm[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
        }

        var stream = new AudioStreamWav
        {
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = sampleRate,
            Data = pcm,
            Stereo = false
        };

        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = stream;
        player.Finished += () => player.QueueFree();
        player.Play();
    }

    private void PlayToneSequence(float[] freqs, float noteDuration)
    {
        for (int i = 0; i < freqs.Length; i++)
        {
            float freq = freqs[i];
            float delay = i * noteDuration;
            GetTree().CreateTimer(delay).Timeout += () =>
            {
                PlayTone(freq, noteDuration * 1.5f, 0.2f, ToneType.Sine);
            };
        }
    }
}
