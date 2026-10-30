using Godot;
using System;

public partial class DspSoundtrack : Node
{
    private const int SampleRate = 22050;
    private const int DurationSeconds = 16;
    public static DspSoundtrack Instance { get; private set; }
    private AudioStreamPlayer _player;
    private AudioStreamPlayer _jumpPlayer;
    private AudioStreamPlayer _attackPlayer;
    private AudioStreamPlayer _collectPlayer;

    public override void _Ready()
    {
        Instance = this;
        _player = new AudioStreamPlayer
        {
            Name = "LoveglowPlayer",
            Stream = BuildLoveglowLoop(),
            VolumeDb = -13f,
            Autoplay = false
        };
        AddChild(_player);
        _jumpPlayer = AddEffectPlayer("JumpSfx", BuildEffect(320, 610, 0.18, 0.12, false));
        _attackPlayer = AddEffectPlayer("AttackSfx", BuildEffect(560, 145, 0.16, 0.10, false));
        _collectPlayer = AddEffectPlayer("CollectSfx", BuildEffect(760, 1320, 0.24, 0.13, true));
        _player.Play();
        GD.Print("[DSP Audio] Loveglow ambient loop started (synthesized PCM, no audio files).");
    }

    public void PlayJump() => _jumpPlayer?.Play();
    public void PlayAttack() => _attackPlayer?.Play();
    public void PlayCollect() => _collectPlayer?.Play();

    private AudioStreamPlayer AddEffectPlayer(string playerName, AudioStreamWav stream)
    {
        var player = new AudioStreamPlayer
        {
            Name = playerName,
            Stream = stream,
            VolumeDb = -2f
        };
        AddChild(player);
        return player;
    }

    private static AudioStreamWav BuildEffect(double startFrequency, double endFrequency, double duration, double volume, bool sparkle)
    {
        var frames = (int)(SampleRate * duration);
        var pcm = new byte[frames * 2];
        for (var i = 0; i < frames; i++)
        {
            var t = (double)i / SampleRate;
            var progress = t / duration;
            var frequency = startFrequency + (endFrequency - startFrequency) * progress;
            var envelope = Math.Pow(1.0 - progress, sparkle ? 2.2 : 1.7);
            var wave = Math.Sin(Math.PI * 2.0 * frequency * t);
            if (sparkle)
                wave = wave * 0.72 + Math.Sin(Math.PI * 4.0 * frequency * t) * 0.2 +
                       Math.Sin(Math.PI * 6.0 * frequency * t) * 0.08;
            else
                wave = wave * 0.82 + Math.Sin(Math.PI * 4.0 * frequency * t) * 0.18;
            var sample = (short)Math.Clamp((int)(Math.Tanh(wave * envelope) * volume * short.MaxValue), short.MinValue, short.MaxValue);
            pcm[i * 2] = (byte)(sample & 0xff);
            pcm[i * 2 + 1] = (byte)((sample >> 8) & 0xff);
        }
        return new AudioStreamWav
        {
            Data = pcm,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = SampleRate,
            Stereo = false
        };
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