using System.Net.Http.Json;
using System.Text.Json;
using Enhanzer.Api.Models;

namespace Enhanzer.Api.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalAuthService> _logger;
    private const string DefaultEndpoint = "https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke";

    private readonly string _endpointUrl;

    public ExternalAuthService(HttpClient httpClient, ILogger<ExternalAuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpointUrl = configuration["ExternalApi:EndpointUrl"] ?? DefaultEndpoint;
    }

    public async Task<ExternalAuthResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new ExternalAuthResult(false, "Email and password are required.", null, []);
        }

        var requestPayload = new ExternalApiLoginRequest
        {
            ApiAction = "GetLoginData",
            DeviceId = "D001",
            SyncTime = string.Empty,
            CompanyCode = email.Trim(),
            ApiBody = new ExternalApiBody
            {
                Username = email.Trim(),
                Password = password
            }
        };

        try
        {
            _logger.LogInformation("Sending external authentication request for user: {Email}", email);

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            using var httpContent = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpointUrl, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External auth API returned non-success status code: {StatusCode}", response.StatusCode);
                return new ExternalAuthResult(false, $"Upstream authentication service returned status {(int)response.StatusCode}.", null, []);
            }

            var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            var apiResponse = JsonSerializer.Deserialize<ExternalApiResponse>(
                rawResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (apiResponse != null && apiResponse.StatusCode != 200)
            {
                _logger.LogWarning("Upstream API returned non-200 payload: StatusCode={StatusCode}, Message={Message}", apiResponse.StatusCode, apiResponse.Message);
            }

            if (apiResponse == null)
            {
                _logger.LogWarning("Failed to parse external auth API response.");
                return new ExternalAuthResult(false, "Invalid response received from authentication provider.", null, []);
            }

            if (apiResponse.StatusCode != 200)
            {
                _logger.LogWarning("External auth API returned business status code: {StatusCode}", apiResponse.StatusCode);
                return new ExternalAuthResult(false, apiResponse.Message ?? "Authentication failed.", null, []);
            }

            var userPayload = apiResponse.ResponseBody?.FirstOrDefault();
            if (userPayload == null)
            {
                _logger.LogWarning("External auth API returned empty Response_Body.");
                return new ExternalAuthResult(false, "Authentication failed: No user profile returned.", null, []);
            }

            // The external API returns Doc_Msg = 'Invalid Login Details' on wrong credentials
            if (!string.IsNullOrWhiteSpace(userPayload.DocMsg))
            {
                _logger.LogWarning("Authentication failed with message: {DocMsg}", userPayload.DocMsg);
                return new ExternalAuthResult(false, userPayload.DocMsg, null, []);
            }

            var locations = userPayload.UserLocations ?? [];
            _logger.LogInformation("Authentication succeeded for {Email}. Locations received: {Count}", email, locations.Count);

            return new ExternalAuthResult(true, null, userPayload, locations);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogError(ex, "External authentication timed out.");
            return new ExternalAuthResult(false, "The authentication request timed out. Please try again.", null, []);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while calling external auth API.");
            return new ExternalAuthResult(false, "Network error reaching authentication service. Please check your connection.", null, []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ExternalAuthService.");
            return new ExternalAuthResult(false, "An unexpected error occurred during authentication.", null, []);
        }
    }
}

