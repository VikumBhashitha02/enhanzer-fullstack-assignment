import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PurchaseBillComponent } from './purchase-bill.component';
import { AuthService } from '../../core/services/auth.service';
import { LocationService } from '../../core/services/location.service';
import { ModulesService } from '../../core/services/modules.service';
import { ItemCatalogService } from './services/item-catalog.service';
import { PurchaseCalculationService } from './services/purchase-calculation.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { LocationItem } from '../../core/models/location.models';
import { BillHeader, BillNote, BillDocument, BillAccount, BillTax } from '../../core/models/modules.models';

describe('PurchaseBillComponent', () => {
  let component: PurchaseBillComponent;
  let fixture: ComponentFixture<PurchaseBillComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let locationServiceSpy: jasmine.SpyObj<LocationService>;
  let modulesServiceSpy: jasmine.SpyObj<ModulesService>;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockLocations: LocationItem[] = [
    { id: 1, locationCode: 'LOC-1', locationName: 'Main Warehouse', companyCode: 'EZCMP-1' },
    { id: 2, locationCode: 'LOC-2', locationName: 'Block C', companyCode: 'EZCMP-1' }
  ];

  const mockHeaders: BillHeader[] = [
    {
      id: 1,
      companyCode: 'EZCMP-1',
      billNumber: 'BILL-001',
      billDate: '2026-11-01T00:00:00Z',
      supplierName: 'Fresh Fruits Ltd',
      status: 'Approved',
      totalAmount: 1200,
      createdAtUtc: '2026-11-01T00:00:00Z'
    }
  ];

  const mockNotes: BillNote[] = [
    {
      id: 1,
      companyCode: 'EZCMP-1',
      title: 'Delivery Note',
      description: 'Checked quality of mangoes',
      category: 'General',
      createdAtUtc: '2026-11-01T00:00:00Z',
      updatedAtUtc: '2026-11-01T00:00:00Z'
    }
  ];

  const mockDocuments: BillDocument[] = [
    {
      id: 1,
      companyCode: 'EZCMP-1',
      fileName: 'Invoice-001.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 204800,
      uploadedAtUtc: '2026-11-01T00:00:00Z'
    }
  ];

  const mockAccounts: BillAccount[] = [
    {
      id: 1,
      companyCode: 'EZCMP-1',
      accountCode: 'ACC-5001',
      accountName: 'Fruit Inventory Cost',
      accountType: 'Expense',
      balance: 5000,
      status: 'Active',
      createdAtUtc: '2026-11-01T00:00:00Z'
    }
  ];

  const mockTaxes: BillTax[] = [
    {
      id: 1,
      companyCode: 'EZCMP-1',
      taxCode: 'VAT-15',
      taxName: 'VAT 15%',
      taxRate: 15,
      status: 'Active',
      createdAtUtc: '2026-11-01T00:00:00Z'
    }
  ];

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['logout', 'currentUser']);
    locationServiceSpy = jasmine.createSpyObj('LocationService', ['getLocations']);
    modulesServiceSpy = jasmine.createSpyObj('ModulesService', [
      'getHeaders', 'getHeader', 'createHeader', 'updateHeader', 'deleteHeader',
      'getNotes', 'getNote', 'createNote', 'updateNote', 'deleteNote',
      'getDocuments', 'uploadDocument', 'downloadDocument', 'deleteDocument',
      'getAccounts', 'getAccount', 'createAccount', 'updateAccount', 'deleteAccount',
      'getTaxes', 'getTax', 'createTax', 'updateTax', 'deleteTax'
    ]);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    locationServiceSpy.getLocations.and.returnValue(of(mockLocations));
    modulesServiceSpy.getHeaders.and.returnValue(of(mockHeaders));
    modulesServiceSpy.getNotes.and.returnValue(of(mockNotes));
    modulesServiceSpy.getDocuments.and.returnValue(of(mockDocuments));
    modulesServiceSpy.getAccounts.and.returnValue(of(mockAccounts));
    modulesServiceSpy.getTaxes.and.returnValue(of(mockTaxes));

    const activatedRouteStub = {
      data: of({}),
      queryParamMap: of({ get: () => null })
    };

    await TestBed.configureTestingModule({
      imports: [PurchaseBillComponent, NoopAnimationsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: LocationService, useValue: locationServiceSpy },
        { provide: ModulesService, useValue: modulesServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRouteStub },
        ItemCatalogService,
        PurchaseCalculationService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PurchaseBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ==================== Baseline Purchase Bill Calculations Tests ====================
  it('should create purchase bill component and load locations from service', () => {
    expect(component).toBeTruthy();
    expect(locationServiceSpy.getLocations).toHaveBeenCalled();
    expect(component.locations().length).toBe(2);
    expect(component.billForm.get('batch')?.value).toBe('Main Warehouse');
  });

  it('should correctly preview calculations for assignment sample values (100, 150, 5, 20%)', () => {
    component.billForm.patchValue({
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      discountPercentage: 20
    });

    component.updateCalculatedTotals();
    const preview = component.calculatedPreview();

    expect(preview.subtotal).toBe(500);
    expect(preview.discountAmount).toBe(100);
    expect(preview.totalCost).toBe(400);
    expect(preview.totalSelling).toBe(750);
  });

  it('should add a valid item to the purchase table and update summaries', () => {
    component.billForm.patchValue({
      item: 'Mango',
      batch: 'Main Warehouse',
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      discountPercentage: 20
    });

    component.onAddItem();

    const items = component.purchaseItems();
    expect(items.length).toBe(1);
    expect(items[0].item).toBe('Mango');
    expect(items[0].totalCost).toBe(400);
    expect(items[0].totalSelling).toBe(750);

    // Verify derived summary
    expect(component.summary().totalItems).toBe(1);
    expect(component.summary().totalQuantity).toBe(5);
    expect(component.summary().netTotalCost).toBe(400);
  });

  it('should reject invalid fruit items not present in the 7 allowed fruits', () => {
    component.billForm.patchValue({
      item: 'Pineapple', // Invalid fruit
      batch: 'Main Warehouse',
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      discountPercentage: 20
    });

    component.onAddItem();

    expect(component.purchaseItems().length).toBe(0);
    expect(component.billForm.get('item')?.hasError('invalidItem')).toBeTrue();
  });

  it('should update Total Items and Total Quantity correctly when adding multiple items and removing one', () => {
    // Add Item 1 (Quantity = 5)
    component.billForm.patchValue({
      item: 'Mango',
      batch: 'Main Warehouse',
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      discountPercentage: 20
    });
    component.onAddItem();

    // Add Item 2 (Quantity = 3)
    component.billForm.patchValue({
      item: 'Apple',
      batch: 'Block C',
      standardCost: 200,
      standardPrice: 300,
      quantity: 3,
      discountPercentage: 10
    });
    component.onAddItem();

    expect(component.summary().totalItems).toBe(2);
    expect(component.summary().totalQuantity).toBe(8);

    // Remove first item
    const firstItemId = component.purchaseItems()[0].id;
    component.onRemoveItem(firstItemId);

    expect(component.summary().totalItems).toBe(1);
    expect(component.summary().totalQuantity).toBe(3);
  });

  // ==================== Navigation Tabs Tests ====================
  it('should switch between main navigation tabs (Header, Details, Notes, Documents)', () => {
    expect(component.activeTab()).toBe('details');

    component.setTab('header');
    expect(component.activeTab()).toBe('header');

    component.setTab('notes');
    expect(component.activeTab()).toBe('notes');

    component.setTab('documents');
    expect(component.activeTab()).toBe('documents');

    component.setTab('details');
    expect(component.activeTab()).toBe('details');
  });

  it('should switch between sub tabs under details (Items, Accounts, Taxes)', () => {
    expect(component.activeSubTab()).toBe('items');

    component.setSubTab('accounts');
    expect(component.activeSubTab()).toBe('accounts');

    component.setSubTab('taxes');
    expect(component.activeSubTab()).toBe('taxes');

    component.setSubTab('items');
    expect(component.activeSubTab()).toBe('items');
  });

  // ==================== Header Module Tests ====================
  it('should load headers from service and manage creation modal', () => {
    expect(modulesServiceSpy.getHeaders).toHaveBeenCalled();
    expect(component.headers().length).toBe(1);

    component.openCreateHeaderModal();
    expect(component.isHeaderModalOpen()).toBeTrue();
    expect(component.editingHeader()).toBeNull();

    component.headerForm.patchValue({
      billNumber: 'BILL-TEST-99',
      billDate: '2026-11-02',
      supplierName: 'Organic Orchards',
      status: 'Draft',
      totalAmount: 1500
    });

    modulesServiceSpy.createHeader.and.returnValue(of(mockHeaders[0]));
    component.saveHeader();

    expect(modulesServiceSpy.createHeader).toHaveBeenCalled();
    expect(component.isHeaderModalOpen()).toBeFalse();
  });

  // ==================== Notes Module Tests ====================
  it('should load notes and save a new note successfully', () => {
    expect(modulesServiceSpy.getNotes).toHaveBeenCalled();
    expect(component.notes().length).toBe(1);

    component.openCreateNoteModal();
    expect(component.isNoteModalOpen()).toBeTrue();

    component.noteForm.patchValue({
      title: 'Packaging Notice',
      category: 'Billing',
      description: 'Cardboard crates verified'
    });

    modulesServiceSpy.createNote.and.returnValue(of(mockNotes[0]));
    component.saveNote();

    expect(modulesServiceSpy.createNote).toHaveBeenCalled();
    expect(component.isNoteModalOpen()).toBeFalse();
  });

  // ==================== Documents Module Tests ====================
  it('should reject file upload when file size exceeds 10MB', () => {
    const largeBlob = new Blob(['x'.repeat(1024)], { type: 'application/pdf' });
    const largeFile = new File([largeBlob], 'large.pdf', { type: 'application/pdf' });
    Object.defineProperty(largeFile, 'size', { value: 11 * 1024 * 1024 });

    const mockEvent = {
      target: {
        files: [largeFile],
        value: ''
      }
    } as any;

    component.onFileSelected(mockEvent);
    expect(component.uploadError()).toContain('exceeds the 10 MB maximum size limit');
  });

  it('should reject file upload for unsupported file extensions', () => {
    const invalidFile = new File(['hello'], 'script.exe', { type: 'application/x-msdownload' });
    const mockEvent = {
      target: {
        files: [invalidFile],
        value: ''
      }
    } as any;

    component.onFileSelected(mockEvent);
    expect(component.uploadError()).toContain('not permitted');
  });

  // ==================== Accounts Module Tests ====================
  it('should load accounts and save account', () => {
    expect(modulesServiceSpy.getAccounts).toHaveBeenCalled();
    expect(component.accounts().length).toBe(1);

    component.openCreateAccountModal();
    expect(component.isAccountModalOpen()).toBeTrue();

    component.accountForm.patchValue({
      accountCode: 'ACC-8800',
      accountName: 'Freight Logistics',
      accountType: 'Expense',
      balance: 1000,
      status: 'Active'
    });

    modulesServiceSpy.createAccount.and.returnValue(of(mockAccounts[0]));
    component.saveAccount();

    expect(modulesServiceSpy.createAccount).toHaveBeenCalled();
    expect(component.isAccountModalOpen()).toBeFalse();
  });

  // ==================== Taxes Module Tests ====================
  it('should load taxes and validate tax rate 0-100%', () => {
    expect(modulesServiceSpy.getTaxes).toHaveBeenCalled();
    expect(component.taxes().length).toBe(1);

    component.openCreateTaxModal();
    expect(component.isTaxModalOpen()).toBeTrue();

    component.taxForm.patchValue({
      taxCode: 'VAT-150',
      taxName: 'Invalid Rate',
      taxRate: 150, // exceeds 100
      status: 'Active'
    });

    expect(component.taxForm.get('taxRate')?.hasError('max')).toBeTrue();
  });

  // ==================== Top Toolbar Buttons Tests ====================
  it('should toggle search visibility and open settings modal', () => {
    const initialVisibility = component.isSearchBarVisible();
    component.onTopBarSearchToggle();
    expect(component.isSearchBarVisible()).toBe(!initialVisibility);

    component.onTopBarSettings();
    expect(component.isSettingsModalOpen()).toBeTrue();

    component.closeSettingsModal();
    expect(component.isSettingsModalOpen()).toBeFalse();
  });

  it('should trigger contextual creation when clicking topbar New button', () => {
    component.setTab('header');
    component.onTopBarNew();
    expect(component.isHeaderModalOpen()).toBeTrue();
    component.closeHeaderModal();

    component.setTab('notes');
    component.onTopBarNew();
    expect(component.isNoteModalOpen()).toBeTrue();
    component.closeNoteModal();
  });
});
