using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Day2Vizov3._0.Models;
using Day2Vizov3._0.Services;

namespace Day2Vizov3._0.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;

    public DocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var username = User.FindFirst("username")?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        
        var documents = await _documentService.GetDocumentsAsync(username, role);
        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        var username = User.FindFirst("username")?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
        var document = await _documentService.CreateDocumentAsync(username, role, request);
        
        if (document == null)
        {
            return Forbid();
        }
        
        return CreatedAtAction(nameof(GetDocuments), new { id = document.Id }, new DocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            IsPublic = document.IsPublic,
            OwnerName = username,
            CreatedAt = document.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentRequest request)
    {
        var username = User.FindFirst("username")?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
        var document = await _documentService.UpdateDocumentAsync(id, username, role, request);
        
        if (document == null)
        {
            return NotFound(new { error = "Документ не найден или у вас нет прав на его редактирование" });
        }
        
        return Ok(new DocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            IsPublic = document.IsPublic,
            OwnerName = username,
            CreatedAt = document.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var username = User.FindFirst("username")?.Value ?? "";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
        var result = await _documentService.DeleteDocumentAsync(id, username, role);
        
        if (!result)
        {
            return NotFound(new { error = "Документ не найден или у вас нет прав на его удаление" });
        }
        
        return NoContent();
    }
}
