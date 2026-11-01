using Enhanzer.Api.Models;

namespace Enhanzer.Api.Services;

public record ExternalAuthResult(
    bool IsSuccess,
    string? ErrorMessage,
    ExternalUserPayload? User,
    List<ExternalLocationItem> Locations
);

public interface IExternalAuthService
{
    Task<ExternalAuthResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}

