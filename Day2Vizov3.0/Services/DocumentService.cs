using Microsoft.EntityFrameworkCore;
using Day2Vizov3._0.Data;
using Day2Vizov3._0.Models;

namespace Day2Vizov3._0.Services;

public class DocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ApplicationDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<DocumentResponse>> GetDocumentsAsync(string username, string role)
    {
        var query = _context.Documents
            .Include(d => d.Owner)
            .Where(d => !d.IsDeleted);
        
        switch (role)
        {
            case "Admin":
                break;
                
            case "Manager":
                query = query.Where(d => d.Owner != null && d.Owner.Username == username || d.IsPublic);
                break;
                
            case "User":
            default:
                query = query.Where(d => d.IsPublic);
                break;
        }
        
        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentResponse
            {
                Id = d.Id,
                Title = d.Title,
                IsPublic = d.IsPublic,
                OwnerName = d.Owner != null ? d.Owner.Username : "Unknown",
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
        
        return documents;
    }

    public async Task<Document?> CreateDocumentAsync(string username, string role, CreateDocumentRequest request)
    {
        if (role != "Manager" && role != "Admin")
        {
            return null;
        }
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return null;
        }
        
        var document = new Document
        {
            Title = request.Title,
            Content = request.Content,
            IsPublic = request.IsPublic,
            OwnerId = user.Id
        };
        
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        
        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int id, string username, string role)
    {
        var document = await _context.Documents
            .Include(d => d.Owner)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        
        if (document == null)
        {
            return false;
        }
        
        if (role == "Admin")
        {
            document.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        
        if (role == "Manager" && document.Owner != null && document.Owner.Username == username)
        {
            document.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }
}