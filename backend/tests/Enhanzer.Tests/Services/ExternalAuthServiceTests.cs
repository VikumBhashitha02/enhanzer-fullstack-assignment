using System.Net;
using System.Text.Json;
using Enhanzer.Api.Models;
using Enhanzer.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Enhanzer.Tests.Services;

public class ExternalAuthServiceTests
{
    private readonly Mock<ILogger<ExternalAuthService>> _loggerMock = new();
    private readonly IConfiguration _configuration;

    public ExternalAuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ExternalApi:EndpointUrl", "https://test-api.example.com/api/invoke"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyCredentials_ReturnsFailureImmediately()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ExternalAuthService(httpClient, _loggerMock.Object, _configuration);

        // Act
        var result = await service.AuthenticateAsync("", "");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("required", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        handlerMock.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenCredentialsValid_ReturnsSuccessAndLocations()
    {
        // Arrange
        var mockResponse = new ExternalApiResponse
        {
            StatusCode = 200,
            Message = "Executed Successfully",
            ResponseBody =
            [
                new ExternalUserPayload
                {
                    Email = "info@enhanzer.com",
                    CompanyCode = "EZCMP-1",
                    UserDisplayName = "eZuite Admin",
                    UserLocations =
                    [
                        new ExternalLocationItem { LocationCode = "LOC-1", LocationName = "Main Warehouse" },
                        new ExternalLocationItem { LocationCode = "LOC-2", LocationName = "Block A" }
                    ]
                }
            ]
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockResponse))
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ExternalAuthService(httpClient, _loggerMock.Object, _configuration);

        // Act
        var result = await service.AuthenticateAsync("info@enhanzer.com", "Welcome#5");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.User);
        Assert.Equal("info@enhanzer.com", result.User.Email);
        Assert.Equal(2, result.Locations.Count);
        Assert.Equal("Main Warehouse", result.Locations[0].LocationName);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenInvalidCredentialsDocMsgReturned_ReturnsFailure()
    {
        // Arrange (matches the exact live API behavior we verified: StatusCode 200 but Doc_Msg: Invalid Login Details)
        var mockResponse = new ExternalApiResponse
        {
            StatusCode = 200,
            ResponseBody =
            [
                new ExternalUserPayload
                {
                    Email = "info@enhanzer.com",
                    DocMsg = "Invalid Login Details"
                }
            ]
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockResponse))
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ExternalAuthService(httpClient, _loggerMock.Object, _configuration);

        // Act
        var result = await service.AuthenticateAsync("info@enhanzer.com", "WrongPassword");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid Login Details", result.ErrorMessage);
        Assert.Empty(result.Locations);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUpstreamReturnsHttp500_ReturnsFailure()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ExternalAuthService(httpClient, _loggerMock.Object, _configuration);

        // Act
        var result = await service.AuthenticateAsync("info@enhanzer.com", "password");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("500", result.ErrorMessage);
    }
}

