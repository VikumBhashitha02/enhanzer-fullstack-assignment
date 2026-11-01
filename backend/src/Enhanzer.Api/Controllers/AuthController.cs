using System.Security.Claims;
using Enhanzer.Api.Models;
using Enhanzer.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly ILocationService _locationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IExternalAuthService externalAuthService,
        ILocationService locationService,
        ILogger<AuthController> logger)
    {
        _externalAuthService = externalAuthService;
        _locationService = locationService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponseDto
            {
                Success = false,
                Message = "Please check your inputs and try again."
            });
        }

        _logger.LogInformation("Processing login request for: {Email}", request.Email);

        var authResult = await _externalAuthService.AuthenticateAsync(request.Email, request.Password, cancellationToken);

        if (!authResult.IsSuccess || authResult.User == null)
        {
            _logger.LogWarning("Authentication failed for: {Email}. Reason: {Reason}", request.Email, authResult.ErrorMessage);
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                Message = authResult.ErrorMessage ?? "Invalid login credentials."
            });
        }

        var user = authResult.User;
        var email = user.Email ?? request.Email.Trim();
        var displayName = user.UserDisplayName ?? user.UserCode ?? email;
        var companyCode = user.CompanyCode ?? email;

        // Persist User_Locations in SQL Server as required by Task 1
        var savedCount = 0;
        try
        {
            savedCount = await _locationService.SaveLocationsAsync(
                companyCode,
                email,
                authResult.Locations,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist locations into database during login.");
            // We still proceed or alert - the user is authenticated
        }

        // Establish secure HttpOnly cookie session
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, displayName),
            new("CompanyCode", companyCode)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        _logger.LogInformation("User session established for {Email}. Total locations synced: {Count}", email, savedCount);

        return Ok(new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            User = new UserInfoDto
            {
                Email = email,
                DisplayName = displayName,
                CompanyCode = companyCode
            },
            LocationsCount = authResult.Locations.Count
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User signed out.");
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("me")]
    public ActionResult<UserInfoDto> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new { message = "Not authenticated." });
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;

        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? email;
        var company = User.FindFirst("CompanyCode")?.Value ?? string.Empty;

        return Ok(new UserInfoDto
        {
            Email = email,
            DisplayName = name,
            CompanyCode = company
        });
    }
}

