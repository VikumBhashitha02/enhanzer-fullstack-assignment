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
public class HeadersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HeadersController> _logger;

    public HeadersController(AppDbContext context, ILogger<HeadersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCompanyCode() =>
        User.FindFirst("CompanyCode")?.Value ?? "EZCMP-1";

    private string GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    public async Task<ActionResult<List<BillHeaderDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var query = _context.BillHeaders.AsNoTracking().Where(h => h.CompanyCode == company);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(h =>
                h.BillNumber.ToLower().Contains(term) ||
                h.SupplierName.ToLower().Contains(term) ||
                (h.ReferenceNumber != null && h.ReferenceNumber.ToLower().Contains(term)) ||
                (h.Remarks != null && h.Remarks.ToLower().Contains(term)));
        }

        var headers = await query
            .OrderByDescending(h => h.BillDate)
            .Select(h => new BillHeaderDto
            {
                Id = h.Id,
                CompanyCode = h.CompanyCode,
                BillNumber = h.BillNumber,
                BillDate = h.BillDate,
                SupplierName = h.SupplierName,
                ReferenceNumber = h.ReferenceNumber,
                Status = h.Status,
                Remarks = h.Remarks,
                TotalAmount = h.TotalAmount,
                CreatedBy = h.CreatedBy,
                CreatedAtUtc = h.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(headers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillHeaderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var h = await _context.BillHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyCode == company, cancellationToken);

        if (h == null) return NotFound(new { message = "Header not found." });

        return Ok(new BillHeaderDto
        {
            Id = h.Id,
            CompanyCode = h.CompanyCode,
            BillNumber = h.BillNumber,
            BillDate = h.BillDate,
            SupplierName = h.SupplierName,
            ReferenceNumber = h.ReferenceNumber,
            Status = h.Status,
            Remarks = h.Remarks,
            TotalAmount = h.TotalAmount,
            CreatedBy = h.CreatedBy,
            CreatedAtUtc = h.CreatedAtUtc
        });
    }

    [HttpPost]
    public async Task<ActionResult<BillHeaderDto>> Create([FromBody] CreateBillHeaderDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var user = GetUserEmail();

        var exists = await _context.BillHeaders
            .AnyAsync(h => h.CompanyCode == company && h.BillNumber == dto.BillNumber.Trim(), cancellationToken);

        if (exists)
        {
            return Conflict(new { message = $"A header with Bill Number '{dto.BillNumber}' already exists." });
        }

        var entity = new BillHeader
        {
            CompanyCode = company,
            BillNumber = dto.BillNumber.Trim(),
            BillDate = dto.BillDate,
            SupplierName = dto.SupplierName.Trim(),
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Status = dto.Status.Trim(),
            Remarks = dto.Remarks?.Trim(),
            TotalAmount = dto.TotalAmount,
            CreatedBy = user,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.BillHeaders.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new BillHeaderDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            BillNumber = entity.BillNumber,
            BillDate = entity.BillDate,
            SupplierName = entity.SupplierName,
            ReferenceNumber = entity.ReferenceNumber,
            Status = entity.Status,
            Remarks = entity.Remarks,
            TotalAmount = entity.TotalAmount,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillHeaderDto>> Update(int id, [FromBody] UpdateBillHeaderDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var entity = await _context.BillHeaders
            .FirstOrDefaultAsync(h => h.Id == id && h.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Header not found." });

        entity.SupplierName = dto.SupplierName.Trim();
        entity.BillDate = dto.BillDate;
        entity.ReferenceNumber = dto.ReferenceNumber?.Trim();
        entity.Status = dto.Status.Trim();
        entity.Remarks = dto.Remarks?.Trim();
        entity.TotalAmount = dto.TotalAmount;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new BillHeaderDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            BillNumber = entity.BillNumber,
            BillDate = entity.BillDate,
            SupplierName = entity.SupplierName,
            ReferenceNumber = entity.ReferenceNumber,
            Status = entity.Status,
            Remarks = entity.Remarks,
            TotalAmount = entity.TotalAmount,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var entity = await _context.BillHeaders
            .FirstOrDefaultAsync(h => h.Id == id && h.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Header not found." });

        _context.BillHeaders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Header deleted successfully." });
    }
}
