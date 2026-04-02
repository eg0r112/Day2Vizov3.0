using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Day2Vizov3._0.Data;
using Day2Vizov3._0.Models;

namespace Day2Vizov3._0.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly VkService _vkService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context, 
        IConfiguration configuration, 
        VkService vkService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _vkService = vkService;
        _logger = logger;
    }

    // генерируем Access Token который живёт 1 час
    private string GenerateAccessToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("username", username),
            new Claim("role", role)
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            // Expires = DateTime.UtcNow.AddMinutes(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    // генерируем Refresh Token который живёт 7 дней
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Message = "Пользователь с таким именем уже существует" };
        }

        var validRoles = new[] { "User", "Manager", "Admin" };
        if (!validRoles.Contains(request.Role))
        {
            return new AuthResponse { Success = false, Message = "Недопустимая роль. Доступны: User, Manager, Admin" };
        }

        if (request.Role == "User")
        {
            var user = new AppUser
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user.Username, user.Role);
            var refreshToken = GenerateRefreshToken();
            
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            
            return new AuthResponse
            {
                Success = true,
                Message = "Регистрация успешно завершена",
                Token = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Role = user.Role,
                RequiresConfirmation = false
            };
        }

        var confirmationCode = new Random().Next(100000, 999999).ToString();
        
        var pendingUser = new AppUser
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PendingRole = request.Role,
            ConfirmationCode = confirmationCode,
            ConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(5)
        };
        
        _context.Users.Add(pendingUser);
        await _context.SaveChangesAsync();
        
        await _vkService.SendConfirmationCodeAsync(request.Username, request.Role, confirmationCode);
        
        var tempAccessToken = GenerateAccessToken(request.Username, request.Role);
        
        return new AuthResponse
        {
            Success = true,
            Message = "Код подтверждения отправлен администратору. Подтвердите регистрацию.",
            Token = tempAccessToken,
            Username = request.Username,
            Role = request.Role,
            RequiresConfirmation = true
        };
    }

    public async Task<AuthResponse> ConfirmAsync(string username, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        
        if (user == null)
        {
            return new AuthResponse { Success = false, Message = "Пользователь не найден" };
        }
        
        if (user.ConfirmationCode != code)
        {
            return new AuthResponse { Success = false, Message = "Неверный код подтверждения" };
        }
        
        if (user.ConfirmationCodeExpiry < DateTime.UtcNow)
        {
            return new AuthResponse { Success = false, Message = "Срок действия кода истек" };
        }
        
        user.Role = user.PendingRole ?? "User";
        user.PendingRole = null;
        user.ConfirmationCode = null;
        user.ConfirmationCodeExpiry = null;
        
        var accessToken = GenerateAccessToken(user.Username, user.Role);
        var refreshToken = GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _context.SaveChangesAsync();
        
        return new AuthResponse
        {
            Success = true,
            Message = "Регистрация подтверждена",
            Token = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Role = user.Role,
            RequiresConfirmation = false
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Message = "Неверное имя пользователя или пароль" };
        }
        
        var accessToken = GenerateAccessToken(user.Username, user.Role);
        var refreshToken = GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();
        
        return new AuthResponse
        {
            Success = true,
            Message = "Вход выполнен успешно",
            Token = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Role = user.Role,
            RequiresConfirmation = false
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        
        if (user == null)
        {
            return new AuthResponse { Success = false, Message = "Неверный refresh token" };
        }
        
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return new AuthResponse { Success = false, Message = "Refresh token истек, выполните вход заново" };
        }
        
        var newAccessToken = GenerateAccessToken(user.Username, user.Role);
        var newRefreshToken = GenerateRefreshToken();
        
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();
        
        return new AuthResponse
        {
            Success = true,
            Message = "Токены обновлены",
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Username = user.Username,
            Role = user.Role,
            RequiresConfirmation = false
        };
    }
}