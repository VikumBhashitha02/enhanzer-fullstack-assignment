import { TestBed } from '@angular/core/testing';
import { PurchaseCalculationService } from './purchase-calculation.service';
import { PurchaseItem } from '../../../core/models/purchase-bill.models';

describe('PurchaseCalculationService', () => {
  let service: PurchaseCalculationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PurchaseCalculationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should correctly calculate the required assignment example (100, 150, 5, 20%)', () => {
    // Assignment Specification:
    // Standard Cost = 100
    // Standard Price = 150
    // Quantity = 5
    // Discount = 20%
    // Expected:
    // Subtotal = 100 × 5 = 500
    // Discount Amount = 500 × 20 / 100 = 100
    // Total Cost = 500 - 100 = 400
    // Total Selling = 150 × 5 = 750

    const result = service.calculateRow(100, 150, 5, 20);

    expect(result.subtotal).toBe(500);
    expect(result.discountAmount).toBe(100);
    expect(result.totalCost).toBe(400);
    expect(result.totalSelling).toBe(750);
  });

  it('should handle 0% discount correctly', () => {
    const result = service.calculateRow(100, 150, 5, 0);

    expect(result.subtotal).toBe(500);
    expect(result.discountAmount).toBe(0);
    expect(result.totalCost).toBe(500);
    expect(result.totalSelling).toBe(750);
  });

  it('should handle 100% discount correctly', () => {
    const result = service.calculateRow(100, 150, 5, 100);

    expect(result.subtotal).toBe(500);
    expect(result.discountAmount).toBe(500);
    expect(result.totalCost).toBe(0);
    expect(result.totalSelling).toBe(750);
  });

  it('should clamp discount percentage to max 100 and min 0', () => {
    const negativeDiscount = service.calculateRow(100, 150, 5, -10);
    expect(negativeDiscount.totalCost).toBe(500);

    const excessiveDiscount = service.calculateRow(100, 150, 5, 150);
    expect(excessiveDiscount.totalCost).toBe(0);
  });

  it('should calculate Item Summary (Total Items and Total Quantity) strictly from items', () => {
    const mockItems: PurchaseItem[] = [
      {
        id: '1',
        item: 'Mango',
        batch: 'Warehouse 1',
        standardCost: 100,
        standardPrice: 150,
        quantity: 5,
        freeQty: 0,
        discountPercentage: 20,
        discountAmount: 100,
        subtotal: 500,
        totalCost: 400,
        totalSelling: 750
      },
      {
        id: '2',
        item: 'Apple',
        batch: 'Stores',
        standardCost: 200,
        standardPrice: 300,
        quantity: 3,
        freeQty: 1,
        discountPercentage: 10,
        discountAmount: 60,
        subtotal: 600,
        totalCost: 540,
        totalSelling: 900
      }
    ];

    const summary = service.calculateSummary(mockItems);

    // Row 1 Quantity = 5, Row 2 Quantity = 3
    // Expected: Total Items = 2, Total Quantity = 8
    expect(summary.totalItems).toBe(2);
    expect(summary.totalQuantity).toBe(8);
    expect(summary.grossTotal).toBe(1100);
    expect(summary.totalDiscount).toBe(160);
    expect(summary.netTotalCost).toBe(940);
    expect(summary.netTotalSelling).toBe(1650);
  });

  it('should return zeroes for summary when item list is empty', () => {
    const summary = service.calculateSummary([]);
    expect(summary.totalItems).toBe(0);
    expect(summary.totalQuantity).toBe(0);
    expect(summary.netTotalCost).toBe(0);
  });
});

