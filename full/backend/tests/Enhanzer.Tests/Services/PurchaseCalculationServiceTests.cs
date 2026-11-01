using Enhanzer.Api.Services;
using Xunit;

namespace Enhanzer.Tests.Services;

public class PurchaseCalculationServiceTests
{
    private readonly PurchaseCalculationService _calculationService = new();

    [Fact]
    public void Calculate_RequiredAssignmentExample_ProducesExpectedResults()
    {
        // Assignment Example:
        // Standard Cost = 100
        // Standard Price = 150
        // Quantity = 5
        // Discount = 20%
        // Subtotal = 100 × 5 = 500
        // Discount Amount = 500 × 20 / 100 = 100
        // Total Cost = 500 - 100 = 400
        // Total Selling = 150 × 5 = 750

        // Arrange
        const decimal standardCost = 100m;
        const decimal standardPrice = 150m;
        const int quantity = 5;
        const decimal discountPercentage = 20m;

        // Act
        var result = _calculationService.Calculate(standardCost, standardPrice, quantity, discountPercentage);

        // Assert
        Assert.Equal(500m, result.Subtotal);
        Assert.Equal(100m, result.DiscountAmount);
        Assert.Equal(400m, result.TotalCost);
        Assert.Equal(750m, result.TotalSelling);
    }

    [Fact]
    public void Calculate_ZeroDiscount_ProducesFullSubtotalAsTotalCost()
    {
        // Arrange
        const decimal standardCost = 100m;
        const decimal standardPrice = 150m;
        const int quantity = 5;
        const decimal discountPercentage = 0m;

        // Act
        var result = _calculationService.Calculate(standardCost, standardPrice, quantity, discountPercentage);

        // Assert
        Assert.Equal(500m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(500m, result.TotalCost);
        Assert.Equal(750m, result.TotalSelling);
    }

    [Fact]
    public void Calculate_HundredPercentDiscount_ProducesZeroTotalCost()
    {
        // Arrange
        const decimal standardCost = 100m;
        const decimal standardPrice = 150m;
        const int quantity = 5;
        const decimal discountPercentage = 100m;

        // Act
        var result = _calculationService.Calculate(standardCost, standardPrice, quantity, discountPercentage);

        // Assert
        Assert.Equal(500m, result.Subtotal);
        Assert.Equal(500m, result.DiscountAmount);
        Assert.Equal(0m, result.TotalCost);
        Assert.Equal(750m, result.TotalSelling);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(-25.5)]
    public void Calculate_InvalidDiscount_ThrowsArgumentOutOfRangeException(decimal invalidDiscount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculationService.Calculate(100m, 150m, 5, invalidDiscount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Calculate_ZeroOrNegativeQuantity_ThrowsArgumentOutOfRangeException(int invalidQuantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculationService.Calculate(100m, 150m, invalidQuantity, 20m));
    }

    [Fact]
    public void Calculate_NegativeStandardCost_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculationService.Calculate(-10m, 150m, 5, 20m));
    }

    [Fact]
    public void Calculate_NegativeStandardPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculationService.Calculate(100m, -15m, 5, 20m));
    }
}

