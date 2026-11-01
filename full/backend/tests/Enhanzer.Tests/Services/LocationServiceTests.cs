using Enhanzer.Api.Data;
using Enhanzer.Api.Models;
using Enhanzer.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Enhanzer.Tests.Services;

public class LocationServiceTests
{
    private readonly Mock<ILogger<LocationService>> _loggerMock = new();

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SaveLocationsAsync_NewLocations_SavesAllRecords()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new LocationService(context, _loggerMock.Object);

        var locations = new List<ExternalLocationItem>
        {
            new() { LocationCode = "LOC-1", LocationName = "Warehouse 1", StockHandle = 1 },
            new() { LocationCode = "LOC-2", LocationName = "Warehouse 2", StockHandle = 1 }
        };

        // Act
        var result = await service.SaveLocationsAsync("EZCMP-1", "user@enhanzer.com", locations);

        // Assert
        Assert.Equal(2, result);
        Assert.Equal(2, await context.LocationDetails.CountAsync());

        var saved = await context.LocationDetails.FirstAsync(l => l.LocationCode == "LOC-1");
        Assert.Equal("Warehouse 1", saved.LocationName);
        Assert.Equal("EZCMP-1", saved.CompanyCode);
    }

    [Fact]
    public async Task SaveLocationsAsync_RepeatedLoginWithSameLocations_DoesNotDuplicateRecords()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new LocationService(context, _loggerMock.Object);

        var initialLocations = new List<ExternalLocationItem>
        {
            new() { LocationCode = "LOC-1", LocationName = "Warehouse 1" },
            new() { LocationCode = "LOC-2", LocationName = "Warehouse 2" }
        };

        // First login
        await service.SaveLocationsAsync("EZCMP-1", "user@enhanzer.com", initialLocations);
        Assert.Equal(2, await context.LocationDetails.CountAsync());

        // Second login with updated name on LOC-1 and same LOC-2
        var secondLoginLocations = new List<ExternalLocationItem>
        {
            new() { LocationCode = "LOC-1", LocationName = "Warehouse 1 - Updated" },
            new() { LocationCode = "LOC-2", LocationName = "Warehouse 2" }
        };

        // Act
        var result = await service.SaveLocationsAsync("EZCMP-1", "user@enhanzer.com", secondLoginLocations);

        // Assert
        Assert.Equal(2, await context.LocationDetails.CountAsync()); // No duplicate rows created!
        var updated = await context.LocationDetails.FirstAsync(l => l.LocationCode == "LOC-1");
        Assert.Equal("Warehouse 1 - Updated", updated.LocationName);
    }

    [Fact]
    public async Task GetLocationsAsync_RetrievesSavedLocationsOrderedByName()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new LocationService(context, _loggerMock.Object);

        var locations = new List<ExternalLocationItem>
        {
            new() { LocationCode = "LOC-Z", LocationName = "Zebra Store" },
            new() { LocationCode = "LOC-A", LocationName = "Alpha Store" }
        };

        await service.SaveLocationsAsync("EZCMP-1", "user@enhanzer.com", locations);

        // Act
        var retrieved = await service.GetLocationsAsync("EZCMP-1");

        // Assert
        Assert.Equal(2, retrieved.Count);
        Assert.Equal("Alpha Store", retrieved[0].LocationName);
        Assert.Equal("Zebra Store", retrieved[1].LocationName);
    }
}

