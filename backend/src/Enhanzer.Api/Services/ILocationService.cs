using Enhanzer.Api.Models;

namespace Enhanzer.Api.Services;

public interface ILocationService
{
    Task<int> SaveLocationsAsync(string companyCode, string userEmail, IEnumerable<ExternalLocationItem> locations, CancellationToken cancellationToken = default);
    Task<List<LocationDto>> GetLocationsAsync(string? companyCode = null, string? userEmail = null, CancellationToken cancellationToken = default);
}

