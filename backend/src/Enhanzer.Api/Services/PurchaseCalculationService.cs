namespace Enhanzer.Api.Services;

public record CalculationResult(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TotalCost,
    decimal TotalSelling
);

public interface IPurchaseCalculationService
{
    CalculationResult Calculate(decimal standardCost, decimal standardPrice, int quantity, decimal discountPercentage);
}

public class PurchaseCalculationService : IPurchaseCalculationService
{
    public CalculationResult Calculate(decimal standardCost, decimal standardPrice, int quantity, decimal discountPercentage)
    {
        if (standardCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(standardCost), "Standard cost cannot be negative.");
        }

        if (standardPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(standardPrice), "Standard price cannot be negative.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (discountPercentage < 0 || discountPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount percentage must be between 0 and 100.");
        }

        var subtotal = standardCost * quantity;
        var discountAmount = (subtotal * discountPercentage) / 100m;
        var totalCost = subtotal - discountAmount;
        var totalSelling = standardPrice * quantity;

        // Round to 2 decimal places using standard financial rounding
        return new CalculationResult(
            Math.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
            Math.Round(totalCost, 2, MidpointRounding.AwayFromZero),
            Math.Round(totalSelling, 2, MidpointRounding.AwayFromZero)
        );
    }
}

