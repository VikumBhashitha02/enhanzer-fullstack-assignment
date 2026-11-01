using System.Security.Claims;
using Enhanzer.Api.Controllers;
using Enhanzer.Api.Models;
using Enhanzer.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Enhanzer.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IExternalAuthService> _authServiceMock = new();
    private readonly Mock<ILocationService> _locationServiceMock = new();
    private readonly Mock<ILogger<AuthController>> _loggerMock = new();

    private AuthController CreateController()
    {
        var controller = new AuthController(
            _authServiceMock.Object,
            _locationServiceMock.Object,
            _loggerMock.Object
        );

        var serviceCollection = new ServiceCollection();
        var authenticationServiceMock = new Mock<IAuthenticationService>();
        serviceCollection.AddSingleton(authenticationServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceCollection.BuildServiceProvider()
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController();
        var request = new LoginRequestDto { Email = "info@enhanzer.com", Password = "Wrong" };

        _authServiceMock.Setup(s => s.AuthenticateAsync(request.Email, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalAuthResult(false, "Invalid login credentials.", null, []));

        // Act
        var result = await controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDto>(unauthorized.Value);
        Assert.False(response.Success);
        Assert.Equal("Invalid login credentials.", response.Message);
    }

    [Fact]
    public async Task Login_WhenModelStateInvalid_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Email is required");
        var request = new LoginRequestDto { Email = "", Password = "" };

        // Act
        var result = await controller.Login(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDto>(badRequest.Value);
        Assert.False(response.Success);
    }
}

