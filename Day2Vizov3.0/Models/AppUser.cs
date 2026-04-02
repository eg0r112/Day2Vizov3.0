using System.ComponentModel.DataAnnotations;

namespace Day2Vizov3._0.Models;

public class AppUser
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string? PendingRole { get; set; }
    
    [MaxLength(10)]
    public string? ConfirmationCode { get; set; }
    
    public DateTime? ConfirmationCodeExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}