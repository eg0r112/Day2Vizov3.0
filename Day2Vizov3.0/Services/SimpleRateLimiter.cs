using System.Collections.Concurrent;

namespace Day2Vizov3._0.Services;

public class SimpleRateLimiter
{
    private readonly ConcurrentDictionary<string, UserRequestInfo> _requests = new();
    private readonly int _limit;
    private readonly TimeSpan _window;

    public SimpleRateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        
        var info = _requests.AddOrUpdate(key, 
            new UserRequestInfo { Count = 1, WindowStart = now },
            (_, existing) =>
            {
                if (now - existing.WindowStart > _window)
                {
                    return new UserRequestInfo { Count = 1, WindowStart = now };
                }
                else
                {
                    existing.Count++;
                    return existing;
                }
            });

        return info.Count <= _limit;
    }
    
    public void Reset(string key)
    {
        _requests.TryRemove(key, out _);
    }
    
    public void ResetAll()
    {
        _requests.Clear();
    }
    
    public int GetRemaining(string key)
    {
        var now = DateTime.UtcNow;
        
        if (_requests.TryGetValue(key, out var info))
        {
            if (now - info.WindowStart > _window)
            {
                return _limit;
            }
            return Math.Max(0, _limit - info.Count);
        }
        
        return _limit;
    }

    private class UserRequestInfo
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}