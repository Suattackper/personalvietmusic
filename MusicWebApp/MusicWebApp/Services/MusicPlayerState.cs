using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>
/// State trung tâm của trình phát nhạc. Đăng ký Scoped trong Program.cs.
/// Component subscribe OnChange để re-render và OnTrackChanged để nạp video mới.
/// </summary>
public class MusicPlayerState
{
    private readonly ILocalStorageService _storage;
    private readonly Random _random = new();

    public MusicPlayerState(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public MusicTrack? CurrentTrack { get; private set; }
    public List<MusicTrack> Queue { get; private set; } = new();
    public int CurrentIndex { get; private set; } = -1;
    public bool IsPlaying { get; private set; }
    public bool IsShuffle { get; private set; }
    public RepeatMode RepeatMode { get; private set; } = RepeatMode.None;
    public double CurrentTime { get; private set; }
    public double Duration { get; private set; }
    public int Volume { get; private set; } = 100;

    /// <summary>State thay đổi — component nên re-render.</summary>
    public event Action? OnChange;

    /// <summary>Bài hát hiện tại thay đổi — MusicPlayer cần nạp video mới.</summary>
    public event Action<MusicTrack>? OnTrackChanged;

    /// <summary>Yêu cầu play/pause bài hiện tại (true = play).</summary>
    public event Action<bool>? OnPlayPauseRequested;

    /// <summary>Khôi phục queue, volume, repeat, shuffle từ localStorage.</summary>
    public async Task InitializeAsync()
    {
        var queue = await _storage.GetItemAsync<List<MusicTrack>>(StorageKeys.Queue);
        if (queue is { Count: > 0 })
        {
            Queue = queue;
            foreach (var track in Queue)
            {
                track.IsPlaying = false;
            }
        }

        var volume = await _storage.GetItemAsync<int?>(StorageKeys.Volume);
        if (volume is >= 0 and <= 100)
        {
            Volume = volume.Value;
        }

        var repeat = await _storage.GetItemAsync<RepeatMode?>(StorageKeys.RepeatMode);
        if (repeat.HasValue && Enum.IsDefined(repeat.Value))
        {
            RepeatMode = repeat.Value;
        }

        var shuffle = await _storage.GetItemAsync<bool?>(StorageKeys.Shuffle);
        if (shuffle.HasValue)
        {
            IsShuffle = shuffle.Value;
        }

        NotifyChanged();
    }

    /// <summary>Đặt queue mới và phát bài tại vị trí selectedIndex.</summary>
    public void SetQueue(IEnumerable<MusicTrack> tracks, int selectedIndex)
    {
        Queue = tracks.ToList();
        if (Queue.Count == 0)
        {
            CurrentIndex = -1;
            NotifyChanged();
            PersistQueue();
            return;
        }

        CurrentIndex = Math.Clamp(selectedIndex, 0, Queue.Count - 1);
        StartTrack(Queue[CurrentIndex]);
        PersistQueue();
    }

    /// <summary>Phát một bài. Nếu bài chưa có trong queue thì thêm vào cuối.</summary>
    public void PlayTrack(MusicTrack track)
    {
        var index = Queue.FindIndex(t => t.VideoId == track.VideoId);
        if (index < 0)
        {
            Queue.Add(track);
            index = Queue.Count - 1;
            PersistQueue();
        }

        CurrentIndex = index;
        StartTrack(Queue[index]);
    }

    /// <summary>Phát bài tại vị trí index trong queue.</summary>
    public void PlayTrackAt(int index)
    {
        if (index < 0 || index >= Queue.Count)
        {
            return;
        }

        CurrentIndex = index;
        StartTrack(Queue[index]);
    }

    public void AddToQueue(MusicTrack track)
    {
        Queue.Add(track);
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }
        PersistQueue();
        NotifyChanged();
    }

    public void RemoveFromQueue(string videoId)
    {
        var index = Queue.FindIndex(t => t.VideoId == videoId);
        if (index < 0)
        {
            return;
        }

        Queue.RemoveAt(index);

        if (index < CurrentIndex)
        {
            CurrentIndex--;
        }
        else if (index == CurrentIndex)
        {
            // Bài đang phát bị xóa khỏi queue: CurrentTrack vẫn phát tiếp,
            // lùi index để Next trỏ đúng bài kế tiếp trong queue.
            CurrentIndex = index - 1;
        }

        if (Queue.Count == 0)
        {
            CurrentIndex = -1;
        }

        PersistQueue();
        NotifyChanged();
    }

    public void ClearQueue()
    {
        Queue.Clear();
        CurrentIndex = -1;
        PersistQueue();
        NotifyChanged();
    }

    /// <summary>Lấy bài kế tiếp theo shuffle/repeat. Trả về null nếu không có.</summary>
    public MusicTrack? GetNextTrack()
    {
        if (Queue.Count == 0)
        {
            return null;
        }

        if (IsShuffle && Queue.Count > 1)
        {
            int next;
            do
            {
                next = _random.Next(Queue.Count);
            } while (next == CurrentIndex);
            return Queue[next];
        }

        var nextIndex = CurrentIndex + 1;
        if (nextIndex >= Queue.Count)
        {
            return RepeatMode == RepeatMode.All ? Queue[0] : null;
        }

        return Queue[nextIndex];
    }

    /// <summary>Lấy bài trước đó. Trả về null nếu không có.</summary>
    public MusicTrack? GetPreviousTrack()
    {
        if (Queue.Count == 0)
        {
            return null;
        }

        if (IsShuffle && Queue.Count > 1)
        {
            int prev;
            do
            {
                prev = _random.Next(Queue.Count);
            } while (prev == CurrentIndex);
            return Queue[prev];
        }

        var prevIndex = CurrentIndex - 1;
        if (prevIndex < 0)
        {
            return RepeatMode == RepeatMode.All ? Queue[^1] : null;
        }

        return Queue[prevIndex];
    }

    /// <summary>Chuyển sang bài kế tiếp. Trả về false nếu không có bài nào.</summary>
    public bool PlayNext()
    {
        var next = GetNextTrack();
        if (next is null)
        {
            return false;
        }

        CurrentIndex = Queue.FindIndex(t => t.VideoId == next.VideoId);
        StartTrack(next);
        return true;
    }

    /// <summary>Quay về bài trước. Trả về false nếu không có bài nào.</summary>
    public bool PlayPrevious()
    {
        var prev = GetPreviousTrack();
        if (prev is null)
        {
            return false;
        }

        CurrentIndex = Queue.FindIndex(t => t.VideoId == prev.VideoId);
        StartTrack(prev);
        return true;
    }

    public void ToggleShuffle()
    {
        IsShuffle = !IsShuffle;
        _ = _storage.SetItemAsync(StorageKeys.Shuffle, IsShuffle);
        NotifyChanged();
    }

    public void ChangeRepeatMode()
    {
        RepeatMode = RepeatMode switch
        {
            RepeatMode.None => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            _ => RepeatMode.None
        };
        _ = _storage.SetItemAsync(StorageKeys.RepeatMode, RepeatMode);
        NotifyChanged();
    }

    public void SetPlaying(bool isPlaying)
    {
        if (IsPlaying == isPlaying)
        {
            return;
        }

        IsPlaying = isPlaying;
        if (CurrentTrack is not null)
        {
            CurrentTrack.IsPlaying = isPlaying;
        }
        NotifyChanged();
    }

    /// <summary>UI yêu cầu play/pause — MusicPlayer component xử lý qua JS.</summary>
    public void RequestPlayPause(bool play)
    {
        OnPlayPauseRequested?.Invoke(play);
    }

    public void SetVolume(int volume)
    {
        Volume = Math.Clamp(volume, 0, 100);
        _ = _storage.SetItemAsync(StorageKeys.Volume, Volume);
        NotifyChanged();
    }

    public void SetProgress(double currentTime, double duration)
    {
        CurrentTime = currentTime;
        Duration = duration;
        NotifyChanged();
    }

    private void StartTrack(MusicTrack track)
    {
        if (CurrentTrack is not null)
        {
            CurrentTrack.IsPlaying = false;
        }

        CurrentTrack = track;
        track.IsPlaying = true;
        CurrentTime = 0;
        Duration = 0;
        IsPlaying = true;

        NotifyChanged();
        OnTrackChanged?.Invoke(track);
    }

    private void PersistQueue()
    {
        _ = _storage.SetItemAsync(StorageKeys.Queue, Queue);
    }

    private void NotifyChanged()
    {
        OnChange?.Invoke();
    }
}
