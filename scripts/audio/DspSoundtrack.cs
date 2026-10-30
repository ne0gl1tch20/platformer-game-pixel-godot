using Godot;
using System;

public partial class DspSoundtrack : Node
{
    private const int SampleRate = 22050;
    private const int DurationSeconds = 16;
    private AudioStreamPlayer _player;

    public override void _Ready()
    {
        _player = new AudioStreamPlayer
        {
            Name = "LoveglowPlayer",
            Stream = BuildLoveglowLoop(),
            VolumeDb = -13f,
            Autoplay = false
        };
        AddChild(_player);
        _player.Play();
        GD.Print("[DSP Audio] Loveglow ambient loop started (synthesized PCM, no audio files).");
    }

    private static AudioStreamWav BuildLoveglowLoop()
    {
        var frames = SampleRate * DurationSeconds;
        var pcm = new byte[frames * 4];
        var roots = new[] { 174.61, 130.81, 110.00, 146.83 };
        var melody = new[] { 523.25, 587.33, 659.25, 783.99, 698.46, 659.25, 587.33, 523.25,
                             440.00, 523.25, 587.33, 659.25, 587.33, 523.25, 392.00, 440.00 };
        var twoPi = Math.PI * 2.0;

        for (var i = 0; i < frames; i++)
        {
            var time = (double)i / SampleRate;
            var chord = (int)(time / 4.0) % roots.Length;
            var root = roots[chord];
            var chordWave =
                Math.Sin(twoPi * root * time) * 0.050 +
                Math.Sin(twoPi * root * 1.2599 * time + 0.2) * 0.031 +
                Math.Sin(twoPi * root * 1.4983 * time + 0.5) * 0.026 +
                Math.Sin(twoPi * root * 1.8877 * time + 0.8) * 0.014;
            var breath = 0.78 + 0.22 * Math.Sin(twoPi * time / 8.0);
            var bass = Math.Sin(twoPi * (root * 0.5) * time) * 0.035;

            var noteStep = (int)(time / 0.5);
            var noteTime = time - noteStep * 0.5;
            var note = melody[noteStep % melody.Length];
            var pluckEnvelope = Math.Exp(-5.2 * noteTime);
            var pluck = (Math.Sin(twoPi * note * time) +
                         0.22 * Math.Sin(twoPi * note * 2.01 * time)) * pluckEnvelope * 0.075;

            var shimmer = Math.Sin(twoPi * 1046.5 * time + Math.Sin(time * 0.7) * 0.08) * 0.003;
            var sample = (chordWave * breath) + bass + pluck + shimmer;
            sample = Math.Tanh(sample * 1.45) * 0.72;

            var left = ToPcm(sample);
            var right = ToPcm(sample * 0.985 + Math.Sin(twoPi * 0.19 * time) * 0.0015);
            var offset = i * 4;
            pcm[offset] = (byte)(left & 0xff);
            pcm[offset + 1] = (byte)((left >> 8) & 0xff);
            pcm[offset + 2] = (byte)(right & 0xff);
            pcm[offset + 3] = (byte)((right >> 8) & 0xff);
        }

        return new AudioStreamWav
        {
            Data = pcm,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = SampleRate,
            Stereo = true,
            LoopMode = AudioStreamWav.LoopModeEnum.Forward,
            LoopBegin = 0,
            LoopEnd = frames
        };
    }

    private static short ToPcm(double sample)
    {
        return (short)Math.Clamp((int)(sample * short.MaxValue), short.MinValue, short.MaxValue);
    }
}