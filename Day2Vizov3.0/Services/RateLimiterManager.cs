namespace Day2Vizov3._0.Services;

public class RateLimiterManager
{
    private readonly SimpleRateLimiter _registerLimiter;
    private readonly SimpleRateLimiter _confirmLimiter;
    private readonly SimpleRateLimiter _loginLimiter;

    public RateLimiterManager()
    {
        _registerLimiter = new SimpleRateLimiter(5, TimeSpan.FromHours(1));
        _confirmLimiter = new SimpleRateLimiter(3, TimeSpan.FromHours(1));
        _loginLimiter = new SimpleRateLimiter(5, TimeSpan.FromHours(1));
    }

    public bool IsRegisterAllowed(string ip) => _registerLimiter.IsAllowed(ip);
    public bool IsConfirmAllowed(string ip) => _confirmLimiter.IsAllowed(ip);
    public bool IsLoginAllowed(string ip) => _loginLimiter.IsAllowed(ip);
    
    public void ResetForIp(string ip)
    {
        _registerLimiter.Reset(ip);
        _confirmLimiter.Reset(ip);
        _loginLimiter.Reset(ip);
    }
    
    public void ResetAll()
    {
        _registerLimiter.ResetAll();
        _confirmLimiter.ResetAll();
        _loginLimiter.ResetAll();
    }
    
    public object GetStatus(string ip)
    {
        return new
        {
            register = new { remaining = _registerLimiter.GetRemaining(ip), limit = 5, window = "1 hour" },
            confirm = new { remaining = _confirmLimiter.GetRemaining(ip), limit = 3, window = "1 hour" },
            login = new { remaining = _loginLimiter.GetRemaining(ip), limit = 5, window = "1 hour" }
        };
    }
}