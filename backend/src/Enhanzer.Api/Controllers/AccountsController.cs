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
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(AppDbContext context, ILogger<AccountsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCompanyCode() =>
        User.FindFirst("CompanyCode")?.Value ?? "EZCMP-1";

    private string GetUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

    [HttpGet]
    public async Task<ActionResult<List<BillAccountDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var query = _context.BillAccounts.AsNoTracking().Where(a => a.CompanyCode == company);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.AccountCode.ToLower().Contains(term) ||
                a.AccountName.ToLower().Contains(term) ||
                a.AccountType.ToLower().Contains(term) ||
                a.Status.ToLower().Contains(term));
        }

        var accounts = await query
            .OrderBy(a => a.AccountCode)
            .Select(a => new BillAccountDto
            {
                Id = a.Id,
                CompanyCode = a.CompanyCode,
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                AccountType = a.AccountType,
                Balance = a.Balance,
                Status = a.Status,
                CreatedBy = a.CreatedBy,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillAccountDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var a = await _context.BillAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyCode == company, cancellationToken);

        if (a == null) return NotFound(new { message = "Account not found." });

        return Ok(new BillAccountDto
        {
            Id = a.Id,
            CompanyCode = a.CompanyCode,
            AccountCode = a.AccountCode,
            AccountName = a.AccountName,
            AccountType = a.AccountType,
            Balance = a.Balance,
            Status = a.Status,
            CreatedBy = a.CreatedBy,
            CreatedAtUtc = a.CreatedAtUtc
        });
    }

    [HttpPost]
    public async Task<ActionResult<BillAccountDto>> Create([FromBody] CreateBillAccountDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var user = GetUserEmail();

        var exists = await _context.BillAccounts
            .AnyAsync(a => a.CompanyCode == company && a.AccountCode == dto.AccountCode.Trim(), cancellationToken);

        if (exists)
        {
            return Conflict(new { message = $"Account with code '{dto.AccountCode}' already exists." });
        }

        var entity = new BillAccount
        {
            CompanyCode = company,
            AccountCode = dto.AccountCode.Trim(),
            AccountName = dto.AccountName.Trim(),
            AccountType = dto.AccountType.Trim(),
            Balance = dto.Balance,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CreatedBy = user,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _context.BillAccounts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new BillAccountDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            Balance = entity.Balance,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillAccountDto>> Update(int id, [FromBody] UpdateBillAccountDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var company = GetCompanyCode();
        var entity = await _context.BillAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Account not found." });

        entity.AccountName = dto.AccountName.Trim();
        entity.AccountType = dto.AccountType.Trim();
        entity.Balance = dto.Balance;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new BillAccountDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            Balance = entity.Balance,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAtUtc = entity.CreatedAtUtc
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var company = GetCompanyCode();
        var entity = await _context.BillAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.CompanyCode == company, cancellationToken);

        if (entity == null) return NotFound(new { message = "Account not found." });

        _context.BillAccounts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Account deleted successfully." });
    }
}
