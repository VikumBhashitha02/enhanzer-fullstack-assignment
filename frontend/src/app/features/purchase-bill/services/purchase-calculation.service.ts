import { Injectable } from '@angular/core';
import { PurchaseCalculation, PurchaseItem, PurchaseSummary } from '../../../core/models/purchase-bill.models';

@Injectable({
  providedIn: 'root'
})
export class PurchaseCalculationService {
  /**
   * Calculates Total Cost and Total Selling based on assignment formulas:
   * Subtotal = Standard Cost × Quantity
   * Discount Amount = Subtotal × Discount Percentage / 100
   * Total Cost = Subtotal - Discount Amount = Standard Cost × Quantity × (1 - Discount Percentage / 100)
   * Total Selling = Standard Price × Quantity
   */
  public calculateRow(
    standardCost: number,
    standardPrice: number,
    quantity: number,
    discountPercentage: number
  ): PurchaseCalculation {
    const cost = Math.max(0, Number(standardCost) || 0);
    const price = Math.max(0, Number(standardPrice) || 0);
    const qty = Math.max(0, Number(quantity) || 0);
    const discount = Math.min(100, Math.max(0, Number(discountPercentage) || 0));

    const subtotal = this.round(cost * qty);
    const discountAmount = this.round((subtotal * discount) / 100);
    const totalCost = this.round(subtotal - discountAmount);
    const totalSelling = this.round(price * qty);

    return {
      subtotal,
      discountAmount,
      totalCost,
      totalSelling
    };
  }

  /**
   * Derives Item Summary strictly from the collection of items:
   * Total Items = Number of rows in table
   * Total Quantity = Sum of Quantity values of all rows in table
   */
  public calculateSummary(items: PurchaseItem[]): PurchaseSummary {
    if (!items || items.length === 0) {
      return {
        totalItems: 0,
        totalQuantity: 0,
        grossTotal: 0,
        totalDiscount: 0,
        netTotalCost: 0,
        netTotalSelling: 0
      };
    }

    const totalItems = items.length;
    const totalQuantity = items.reduce((sum, item) => sum + (Number(item.quantity) || 0), 0);
    const grossTotal = this.round(items.reduce((sum, item) => sum + (Number(item.subtotal) || 0), 0));
    const totalDiscount = this.round(items.reduce((sum, item) => sum + (Number(item.discountAmount) || 0), 0));
    const netTotalCost = this.round(items.reduce((sum, item) => sum + (Number(item.totalCost) || 0), 0));
    const netTotalSelling = this.round(items.reduce((sum, item) => sum + (Number(item.totalSelling) || 0), 0));

    return {
      totalItems,
      totalQuantity,
      grossTotal,
      totalDiscount,
      netTotalCost,
      netTotalSelling
    };
  }

  public round(value: number): number {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}

