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
public class NotesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotesController> _logger;

    public NotesController(AppDbContext context, ILogger<NotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCompanyCode() =>
        User.FindFirst("CompanyCode")?.Value ?? "EZCMP-1";

    private string GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    public async Task<ActionResult<List<BillNoteDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var query = _context.BillNotes.AsNoTracking().Where(n => n.CompanyCode == company);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(n =>
                n.Title.ToLower().Contains(term) ||
                n.Description.ToLower().Contains(term) ||
                (n.Category != null && n.Category.ToLower().Contains(term)));
        }

        var notes = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new BillNoteDto
            {
                Id = n.Id,
                CompanyCode = n.CompanyCode,
                Title = n.Title,
                Description = n.Description,
                Category = n.Category,
                CreatedBy = n.CreatedBy,
                CreatedAtUtc = n.CreatedAtUtc,
                UpdatedAtUtc = n.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillNoteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var n = await _context.BillNotes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyCode == company, cancellationToken);

        if (n == null) return NotFound(new { message = "Note not found." });

        return Ok(new BillNoteDto
        {
            Id = n.Id,
            CompanyCode = n.CompanyCode,
            Title = n.Title,
            Description = n.Description,
            Category = n.Category,
            CreatedBy = n.CreatedBy,
            CreatedAtUtc = n.CreatedAtUtc,
            UpdatedAtUtc = n.UpdatedAtUtc
        });
    }

    [HttpPost]
    public async Task<ActionResult<BillNoteDto>> Create([FromBody] CreateBillNoteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var user = GetUserEmail();

        var entity = new BillNote
        {
            CompanyCode = company,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
            CreatedBy = user,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.BillNotes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new BillNoteDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            Title = entity.Title,
            Description = entity.Description,
            Category = entity.Category,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillNoteDto>> Update(int id, [FromBody] UpdateBillNoteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var entity = await _context.BillNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Note not found." });

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description.Trim();
        entity.Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new BillNoteDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            Title = entity.Title,
            Description = entity.Description,
            Category = entity.Category,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var entity = await _context.BillNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Note not found." });

        _context.BillNotes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Note deleted successfully." });
    }
}
