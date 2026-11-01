export interface PurchaseItem {
  id: string;
  item: string;
  batch: string;
  locationCode?: string;
  standardCost: number;
  standardPrice: number;
  quantity: number;
  freeQty?: number;
  discountPercentage: number;
  discountAmount: number;
  subtotal: number;
  totalCost: number;
  totalSelling: number;
  margin?: number;
}

export interface PurchaseCalculation {
  subtotal: number;
  discountAmount: number;
  totalCost: number;
  totalSelling: number;
}

export interface PurchaseSummary {
  totalItems: number;
  totalQuantity: number;
  grossTotal: number;
  totalDiscount: number;
  netTotalCost: number;
  netTotalSelling: number;
}
