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
public class TaxesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaxesController> _logger;

    public TaxesController(AppDbContext context, ILogger<TaxesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCompanyCode() =>
        User.FindFirst("CompanyCode")?.Value ?? "EZCMP-1";

    private string GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    public async Task<ActionResult<List<BillTaxDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var query = _context.BillTaxes.AsNoTracking().Where(t => t.CompanyCode == company);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.TaxCode.ToLower().Contains(term) ||
                t.TaxName.ToLower().Contains(term) ||
                t.Status.ToLower().Contains(term));
        }

        var taxes = await query
            .OrderBy(t => t.TaxCode)
            .Select(t => new BillTaxDto
            {
                Id = t.Id,
                CompanyCode = t.CompanyCode,
                TaxCode = t.TaxCode,
                TaxName = t.TaxName,
                TaxRate = t.TaxRate,
                Status = t.Status,
                CreatedBy = t.CreatedBy,
                CreatedAtUtc = t.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(taxes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillTaxDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var t = await _context.BillTaxes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyCode == company, cancellationToken);

        if (t == null) return NotFound(new { message = "Tax record not found." });

        return Ok(new BillTaxDto
        {
            Id = t.Id,
            CompanyCode = t.CompanyCode,
            TaxCode = t.TaxCode,
            TaxName = t.TaxName,
            TaxRate = t.TaxRate,
            Status = t.Status,
            CreatedBy = t.CreatedBy,
            CreatedAtUtc = t.CreatedAtUtc
        });
    }

    [HttpPost]
    public async Task<ActionResult<BillTaxDto>> Create([FromBody] CreateBillTaxDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var user = GetUserEmail();

        var exists = await _context.BillTaxes
            .AnyAsync(t => t.CompanyCode == company && t.TaxCode == dto.TaxCode.Trim(), cancellationToken);

        if (exists)
        {
            return Conflict(new { message = $"Tax with code '{dto.TaxCode}' already exists." });
        }

        var entity = new BillTax
        {
            CompanyCode = company,
            TaxCode = dto.TaxCode.Trim(),
            TaxName = dto.TaxName.Trim(),
            TaxRate = dto.TaxRate,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CreatedBy = user,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.BillTaxes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new BillTaxDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            TaxCode = entity.TaxCode,
            TaxName = entity.TaxName,
            TaxRate = entity.TaxRate,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillTaxDto>> Update(int id, [FromBody] UpdateBillTaxDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var entity = await _context.BillTaxes
            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Tax record not found." });

        entity.TaxName = dto.TaxName.Trim();
        entity.TaxRate = dto.TaxRate;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new BillTaxDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            TaxCode = entity.TaxCode,
            TaxName = entity.TaxName,
            TaxRate = entity.TaxRate,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var entity = await _context.BillTaxes
            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Tax record not found." });

        _context.BillTaxes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Tax record deleted successfully." });
    }
}
