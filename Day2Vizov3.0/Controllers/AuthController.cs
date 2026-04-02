using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Day2Vizov3._0.Models;
using Day2Vizov3._0.Services;

namespace Day2Vizov3._0.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly RateLimiterManager _rateLimiter;

    public AuthController(AuthService authService, RateLimiterManager rateLimiter)
    {
        _authService = authService;
        _rateLimiter = rateLimiter;
    }

    [HttpPost("register")] //5 запросов в час
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!_rateLimiter.IsRegisterAllowed(ip))
        {
            return StatusCode(429, new { error = "Too many registration attempts. Please try again later." });
        }
        
        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpPost("confirm")] //5 запроса в час
    [Authorize]
    public async Task<IActionResult> Confirm([FromBody] ConfirmRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!_rateLimiter.IsConfirmAllowed(ip))
        {
            return StatusCode(429, new { error = "Too many confirmation attempts. Please try again later." });
        }
        
        var username = User.FindFirst("username")?.Value;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "Невалидный токен" });
        }
        
        var result = await _authService.ConfirmAsync(username, request.Code);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpPost("login")] //5 запросов в час
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!_rateLimiter.IsLoginAllowed(ip))
        {
            return StatusCode(429, new { error = "Too many login attempts. Please try again later." });
        }
        
        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }
}