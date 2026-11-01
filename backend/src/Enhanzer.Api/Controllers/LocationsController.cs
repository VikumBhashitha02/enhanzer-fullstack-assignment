using System.Security.Claims;
using Enhanzer.Api.Models;
using Enhanzer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetLocations(CancellationToken cancellationToken)
    {
        var companyCode = User.FindFirst("CompanyCode")?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogInformation("Retrieving locations for CompanyCode: {CompanyCode}, Email: {Email}", companyCode, userEmail);

        var locations = await _locationService.GetLocationsAsync(companyCode, userEmail, cancellationToken);

        // If no records found for specific company, fetch all saved locations
        if (locations.Count == 0)
        {
            locations = await _locationService.GetLocationsAsync(null, null, cancellationToken);
        }

        return Ok(locations);
    }
}

