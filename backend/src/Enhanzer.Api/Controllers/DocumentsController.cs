using System.Security.Claims;
using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DocumentsController> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".xlsx", ".txt", ".csv"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public DocumentsController(AppDbContext context, IWebHostEnvironment env, ILogger<DocumentsController> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    private string GetCompanyCode() =>
        User.FindFirst("CompanyCode")?.Value ?? "EZCMP-1";

    private string GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    public async Task<ActionResult<List<BillDocumentDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var query = _context.BillDocuments.AsNoTracking().Where(d => d.CompanyCode == company);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(d =>
                d.FileName.ToLower().Contains(term) ||
                d.ContentType.ToLower().Contains(term) ||
                (d.UploadedBy != null && d.UploadedBy.ToLower().Contains(term)));
        }

        var docs = await query
            .OrderByDescending(d => d.UploadedAtUtc)
            .Select(d => new BillDocumentDto
            {
                Id = d.Id,
                CompanyCode = d.CompanyCode,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedBy = d.UploadedBy,
                UploadedAtUtc = d.UploadedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(docs);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<ActionResult<BillDocumentDto>> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please select a valid file to upload." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "File size exceeds the 10 MB limit." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"File type '{extension}' is not supported. Allowed formats: PDF, PNG, JPG, DOCX, XLSX, TXT, CSV." });
        }

        var company = GetCompanyCode();
        var user = GetUserEmail();

        var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", company);
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDir, storedFileName);

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var entity = new BillDocument
        {
            CompanyCode = company,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSizeBytes = file.Length,
            FilePath = physicalPath,
            UploadedBy = user,
            UploadedAtUtc = DateTime.UtcNow
        };

        _context.BillDocuments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document uploaded successfully: {FileName} by {User}", entity.FileName, user);

        return Ok(new BillDocumentDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            FileSizeBytes = entity.FileSizeBytes,
            UploadedBy = entity.UploadedBy,
            UploadedAtUtc = entity.UploadedAtUtc
        });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var doc = await _context.BillDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.CompanyCode == company, cancellationToken);

        if (doc == null) return NotFound(new { message = "Document record not found." });

        if (!System.IO.File.Exists(doc.FilePath))
        {
            return NotFound(new { message = "File is missing on server storage." });
        }

        var stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, doc.ContentType, doc.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var doc = await _context.BillDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.CompanyCode == company, cancellationToken);

        if (doc == null) return NotFound(new { message = "Document not found." });

        try
        {
            if (System.IO.File.Exists(doc.FilePath))
            {
                System.IO.File.Delete(doc.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete physical file: {FilePath}", doc.FilePath);
        }

        _context.BillDocuments.Remove(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Document deleted successfully." });
    }
}
