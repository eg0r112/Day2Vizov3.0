using System.ComponentModel.DataAnnotations;

namespace Day2Vizov3._0.Models;

public class CreateDocumentRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
}

public class UpdateDocumentRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
}

public class DocumentResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
