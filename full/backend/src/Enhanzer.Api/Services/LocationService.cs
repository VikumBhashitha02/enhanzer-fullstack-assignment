using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LocationService> _logger;

    public LocationService(AppDbContext context, ILogger<LocationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> SaveLocationsAsync(
        string companyCode,
        string userEmail,
        IEnumerable<ExternalLocationItem> locations,
        CancellationToken cancellationToken = default)
    {
        var locationList = locations.ToList();
        if (locationList.Count == 0)
        {
            return 0;
        }

        try
        {
            var normalizedCompany = companyCode.Trim();
            var normalizedEmail = userEmail.Trim();

            // Fetch existing locations for this company to prevent duplicates
            var existingLocations = await _context.LocationDetails
                .Where(l => l.CompanyCode == normalizedCompany)
                .ToListAsync(cancellationToken);

            var existingMap = existingLocations.ToDictionary(l => l.LocationCode, StringComparer.OrdinalIgnoreCase);

            var addedCount = 0;
            var updatedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var item in locationList)
            {
                if (string.IsNullOrWhiteSpace(item.LocationCode) || string.IsNullOrWhiteSpace(item.LocationName))
                {
                    continue;
                }

                var code = item.LocationCode.Trim();
                var name = item.LocationName.Trim();

                if (existingMap.TryGetValue(code, out var existing))
                {
                    // Update existing record if details changed
                    existing.LocationName = name;
                    existing.StockHandle = item.StockHandle;
                    existing.Address = item.Address;
                    existing.Phone = item.Phone;
                    existing.Status = item.Status;
                    existing.UserEmail = normalizedEmail;
                    existing.UpdatedAtUtc = now;
                    updatedCount++;
                }
                else
                {
                    // Add new record
                    var newEntity = new LocationDetail
                    {
                        CompanyCode = normalizedCompany,
                        LocationCode = code,
                        LocationName = name,
                        StockHandle = item.StockHandle,
                        Address = item.Address,
                        Phone = item.Phone,
                        Status = item.Status,
                        UserEmail = normalizedEmail,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    };

                    _context.LocationDetails.Add(newEntity);
                    addedCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved locations successfully. Added: {Added}, Updated: {Updated}", addedCount, updatedCount);
            return addedCount + updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist locations into SQL Server.");
            throw;
        }
    }

    public async Task<List<LocationDto>> GetLocationsAsync(
        string? companyCode = null,
        string? userEmail = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LocationDetails.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(companyCode))
        {
            query = query.Where(l => l.CompanyCode == companyCode.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(userEmail))
        {
            query = query.Where(l => l.UserEmail == userEmail.Trim());
        }

        return await query
            .OrderBy(l => l.LocationName)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                LocationCode = l.LocationCode,
                LocationName = l.LocationName,
                CompanyCode = l.CompanyCode,
                StockHandle = l.StockHandle
            })
            .ToListAsync(cancellationToken);
    }
}

