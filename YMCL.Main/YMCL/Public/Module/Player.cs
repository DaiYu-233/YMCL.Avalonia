using System.IO;
using System.Net.Http;
using System.Timers;
using NAudio.Wave;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module;

public class AudioPlayer : IDisposable
{
    private static AudioPlayer? _instance;
    private static readonly object _lock = new object();
    private IWavePlayer? _waveOut;
    private AudioFileReader? _waveSource;
    private Mp3FileReader? _waveStream;
    private Timer? _timer;

    public event EventHandler<ProgressEventArgs>? ProgressChanged;
    public event EventHandler<StoppedEventArgs>? PlaybackCompleted;

    private AudioPlayer()
    {
        Data.Setting.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Setting.Volume)) return;
            if (_waveOut == null) return;
            try
            {
                _waveOut.Volume = Convert.ToSingle(Data.Setting.Volume / 100);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        };
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

    public double PlayLocal(string localFilePath)
    {
        Stop();
        _waveSource = new AudioFileReader(localFilePath);
        InitializeAndPlay(_waveSource);
        return _waveSource.TotalTime.TotalMilliseconds;
    }

    public async Task PlayNetwork(string networkUrl)
    {
        Stop();
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
        _waveOut.Volume = Convert.ToSingle(Data.Setting.Volume / 100);
        _timer = new Timer(10); 
        _timer.Elapsed += OnTimedEvent;
        _timer.Start();

        _waveOut.PlaybackStopped += (s, e) => { PlaybackCompleted?.Invoke(s, e); };
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
        _waveOut?.Pause();
    }

    public void Resume()
    {
        if (_waveOut == null) return;
        try
        {
            _waveOut.Play();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
    }

    public void Dispose()
    {
        _waveOut?.Dispose();
        _waveSource?.Dispose();
        _waveStream?.Dispose();
        _timer?.Dispose();
    }

    public void UpdateProgress(double value)
    {
        if (_waveSource != null)
            _waveSource.CurrentTime = TimeSpan.FromMilliseconds(value);
        if (_waveStream != null)
            _waveStream.CurrentTime = TimeSpan.FromMilliseconds(value);
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