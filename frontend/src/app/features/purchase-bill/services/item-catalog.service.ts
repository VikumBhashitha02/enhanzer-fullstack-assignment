import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ItemCatalogService {
  // Exact 7 items required by assignment specification:
  // "Mango", "Apple", "Banana", "Orange", "Grapes", "Kiwi", "Strawberry"
  private readonly allowedItems: string[] = [
    'Mango',
    'Apple',
    'Banana',
    'Orange',
    'Grapes',
    'Kiwi',
    'Strawberry'
  ];

  public getAllowedItems(): string[] {
    return [...this.allowedItems];
  }

  public filterItems(searchTerm: string): string[] {
    if (!searchTerm || typeof searchTerm !== 'string') {
      return this.getAllowedItems();
    }
    const term = searchTerm.trim().toLowerCase();
    return this.allowedItems.filter(item => item.toLowerCase().includes(term));
  }

  public isValidItem(item: string): boolean {
    if (!item) return false;
    return this.allowedItems.some(allowed => allowed.toLowerCase() === item.trim().toLowerCase());
  }

  public canonicalizeItem(item: string): string {
    const matched = this.allowedItems.find(allowed => allowed.toLowerCase() === item.trim().toLowerCase());
    return matched || item.trim();
  }
}

