import { TestBed } from '@angular/core/testing';
import { ItemCatalogService } from './item-catalog.service';

describe('ItemCatalogService', () => {
  let service: ItemCatalogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ItemCatalogService);
  });

  it('should contain all 7 required items in the assignment catalog', () => {
    const items = service.getAllowedItems();
    const expected = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'];

    expect(items.length).toBe(7);
    expected.forEach(fruit => {
      expect(items).toContain(fruit);
    });
  });

  it('should filter items case-insensitively', () => {
    const results = service.filterItems('ap');
    expect(results).toContain('Apple');
    expect(results).toContain('Grapes'); // contains 'ap'
  });

  it('should validate allowed items correctly', () => {
    expect(service.isValidItem('Mango')).toBeTrue();
    expect(service.isValidItem('mango')).toBeTrue();
    expect(service.isValidItem('STRAWBERRY')).toBeTrue();
    expect(service.isValidItem('Watermelon')).toBeFalse();
    expect(service.isValidItem('')).toBeFalse();
  });

  it('should canonicalize item names to match catalog casing', () => {
    expect(service.canonicalizeItem('kIwI')).toBe('Kiwi');
    expect(service.canonicalizeItem('banana')).toBe('Banana');
  });
});

