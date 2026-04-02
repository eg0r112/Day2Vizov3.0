using Microsoft.AspNetCore.Mvc;
using Day2Vizov3._0.Services;

namespace Day2Vizov3._0.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly RateLimiterManager _rateLimiter;
    private readonly ILogger<TestController> _logger;

    public TestController(RateLimiterManager rateLimiter, ILogger<TestController> logger)
    {
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    [HttpPost("reset-limits")]
    public IActionResult ResetLimits()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        _rateLimiter.ResetForIp(ip);
        
        _logger.LogInformation($"Rate limits reset for IP: {ip}");
        
        return Ok(new 
        { 
            success = true, 
            message = $"Rate limits reset for IP: {ip}",
            timestamp = DateTime.UtcNow
        });
    }
}