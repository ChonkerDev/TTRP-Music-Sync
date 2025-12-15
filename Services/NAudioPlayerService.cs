using System;
using NAudio.Wave;

namespace Music_Synchronizer.Services;

public class NAudioPlayerService {
    private WaveOutEvent? _output;
    private AudioFileReader? _reader;

    public bool IsPlaying => _output?.PlaybackState == PlaybackState.Playing;

    public TimeSpan Position {
        get => _reader?.CurrentTime ?? TimeSpan.Zero;
        set {
            if (_reader != null)
                _reader.CurrentTime = value;
        }
    }

    public TimeSpan Duration => _reader?.TotalTime ?? TimeSpan.Zero;

    public float Volume {
        get => _reader?.Volume ?? 1f;
        set {
            if (_reader != null)
                _reader.Volume = Math.Clamp(value, 0f, 1f);
        }
    }

    public void Load(string filePath) {
        StopInternal();

        _reader = new AudioFileReader(filePath);
        _output = new WaveOutEvent();

        _output.Init(_reader);
        _output.PlaybackStopped += OnPlaybackStopped;
    }

    public void Play() {
        _output?.Play();
    }

    public void Pause() {
        _output?.Pause();
    }

    public void Stop() {
        StopInternal();
    }

    private void StopInternal() {
        if (_output != null) {
            _output.PlaybackStopped -= OnPlaybackStopped;
            _output.Stop();
            _output.Dispose();
            _output = null;
        }

        if (_reader != null) {
            _reader.Dispose();
            _reader = null;
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e) {
        // Optional: auto-reset position
        if (_reader != null)
            _reader.Position = 0;
    }

    public bool IsMediaLoaded() {
        return _reader != null;
    }


    public void Dispose() {
        StopInternal();
    }
}