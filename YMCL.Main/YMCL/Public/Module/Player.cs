using NAudio.Wave;
using System;
using System.IO;
using System.Net.Http;
using System.Timers;

public class AudioPlayer : IDisposable
{
    private static AudioPlayer? _instance;
    private static readonly object _lock = new object();
    private IWavePlayer? _waveOut;
    private AudioFileReader? _waveSource;
    private Mp3FileReader? _waveStream;
    private Timer? _timer;
    private bool _isPlaying;

    public event EventHandler<ProgressEventArgs>? ProgressChanged;
    public event EventHandler? PlaybackCompleted;

    private AudioPlayer()
    {
    }

    public static AudioPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AudioPlayer();
                    }
                }
            }
            return _instance;
        }
    }

    public void PlayLocal(string localFilePath)
    {
        Stop();
        _waveOut = new WaveOutEvent();
        _waveSource = new AudioFileReader(localFilePath);
        InitializeAndPlay(_waveSource);
    }

    public async Task PlayNetwork(string networkUrl)
    {
        Stop();
        _waveOut = new WaveOutEvent();
        using var client = new HttpClient();
        var response = await client.GetAsync(networkUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        _waveStream = new Mp3FileReader(memoryStream);
        InitializeAndPlay(_waveStream);
    }

    private void InitializeAndPlay(WaveStream? waveStream)
    {
        if (waveStream == null) return;
        _waveOut = new WaveOutEvent();
        _waveOut?.Init(waveStream);
        _waveOut?.Play();
        _isPlaying = true;

        _timer = new Timer(1000); // 每秒更新进度
        _timer.Elapsed += OnTimedEvent;
        _timer.Start();
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        if (_waveSource != null)
        {
            var progress = _waveSource.CurrentTime.TotalMilliseconds;
            var totalDuration = _waveSource.TotalTime.TotalMilliseconds;
            ProgressChanged?.Invoke(this, new ProgressEventArgs(progress, totalDuration));
        }
        else if (_waveStream != null)
        {
            var progress = _waveStream.CurrentTime.TotalMilliseconds;
            var totalDuration = _waveStream.TotalTime.TotalMilliseconds;
            ProgressChanged?.Invoke(this, new ProgressEventArgs(progress, totalDuration));
        }
    }

    public void Pause()
    {
        if (_waveOut != null && _isPlaying)
        {
            _waveOut.Pause();
            _isPlaying = false;
        }
    }

    public void Resume()
    {
        if (_waveOut != null && !_isPlaying)
        {
            _waveOut.Play();
            _isPlaying = true;
        }
    }

    public void Stop()
    {
        _waveOut?.Stop();
        _waveSource?.Dispose();
        _waveStream?.Dispose();
        _waveSource = null;
        _waveStream = null;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        _isPlaying = false;
        PlaybackCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _waveOut?.Dispose();
        _waveSource?.Dispose();
        _waveStream?.Dispose();
        _timer?.Dispose();
    }
}

public class ProgressEventArgs : EventArgs
{
    public double CurrentTime { get; }
    public double TotalDuration { get; }

    public ProgressEventArgs(double currentTime, double totalDuration)
    {
        CurrentTime = currentTime;
        TotalDuration = totalDuration;
    }
}