using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day2Vizov3._0.Models;

public class Document
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    [ForeignKey("OwnerId")]
    public virtual AppUser? Owner { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}