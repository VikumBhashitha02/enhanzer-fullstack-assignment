import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AuthService } from '../../core/services/auth.service';
import { LocationService } from '../../core/services/location.service';
import { ModulesService } from '../../core/services/modules.service';
import { ItemCatalogService } from './services/item-catalog.service';
import { PurchaseCalculationService } from './services/purchase-calculation.service';
import { LocationItem } from '../../core/models/location.models';
import { PurchaseItem, PurchaseCalculation, PurchaseSummary } from '../../core/models/purchase-bill.models';
import {
  BillHeader,
  CreateBillHeaderDto,
  BillNote,
  CreateBillNoteDto,
  BillDocument,
  BillAccount,
  CreateBillAccountDto,
  BillTax,
  CreateBillTaxDto
} from '../../core/models/modules.models';

export type MainTab = 'header' | 'details' | 'notes' | 'documents';
export type SubTab = 'items' | 'accounts' | 'taxes';

@Component({
  selector: 'app-purchase-bill',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './purchase-bill.component.html',
  styleUrls: ['./purchase-bill.component.scss']
})
export class PurchaseBillComponent implements OnInit {
  // Navigation tabs state
  activeTab = signal<MainTab>('details');
  activeSubTab = signal<SubTab>('items');

  // Search input bar visibility and terms
  isSearchBarVisible = signal<boolean>(true);
  headerSearch = signal<string>('');
  noteSearch = signal<string>('');
  documentSearch = signal<string>('');
  accountSearch = signal<string>('');
  taxSearch = signal<string>('');

  // Forms
  billForm: FormGroup;
  headerForm: FormGroup;
  noteForm: FormGroup;
  accountForm: FormGroup;
  taxForm: FormGroup;

  // Active document context
  selectedHeaderId = signal<number | null>(null);
  activeDocument = computed<BillHeader | null>(() => {
    const list = this.headers();
    if (list.length === 0) return null;
    const selId = this.selectedHeaderId();
    if (selId) {
      const found = list.find(h => h.id === selId);
      if (found) return found;
    }
    return list[0];
  });

  // Autocomplete suggestions for 7 allowed fruits
  filteredItems = signal<string[]>([]);

  // Locations / Batches from SQL Server
  locations = signal<LocationItem[]>([]);
  isLoadingLocations = signal<boolean>(false);
  locationLoadError = signal<string>('');

  // Table items state
  purchaseItems = signal<PurchaseItem[]>([]);

  // Module data signals
  headers = signal<BillHeader[]>([]);
  isLoadingHeaders = signal<boolean>(false);

  notes = signal<BillNote[]>([]);
  isLoadingNotes = signal<boolean>(false);

  documents = signal<BillDocument[]>([]);
  isLoadingDocuments = signal<boolean>(false);

  accounts = signal<BillAccount[]>([]);
  isLoadingAccounts = signal<boolean>(false);

  taxes = signal<BillTax[]>([]);
  isLoadingTaxes = signal<boolean>(false);

  // Modals state
  isHeaderModalOpen = signal<boolean>(false);
  editingHeader = signal<BillHeader | null>(null);
  isViewHeaderModalOpen = signal<boolean>(false);
  selectedHeader = signal<BillHeader | null>(null);

  isNoteModalOpen = signal<boolean>(false);
  editingNote = signal<BillNote | null>(null);
  isViewNoteModalOpen = signal<boolean>(false);
  selectedNote = signal<BillNote | null>(null);

  isAccountModalOpen = signal<boolean>(false);
  editingAccount = signal<BillAccount | null>(null);

  isTaxModalOpen = signal<boolean>(false);
  editingTax = signal<BillTax | null>(null);

  isSettingsModalOpen = signal<boolean>(false);
  isDarkMode = signal<boolean>(false);
  isCompactDensity = signal<boolean>(false);

  // Delete confirmation modal state
  isDeleteConfirmModalOpen = signal<boolean>(false);
  deleteTarget = signal<{ type: 'header' | 'note' | 'document' | 'account' | 'tax'; id: number; title: string } | null>(null);

  // Document upload state
  selectedUploadFile: File | null = null;
  uploadError = signal<string>('');
  isUploading = signal<boolean>(false);

  // Derived summaries using Angular Signals for reactivity & zero desynchronization
  summary = computed<PurchaseSummary>(() => {
    return this.calculationService.calculateSummary(this.purchaseItems());
  });

  // Live calculated preview for current form input
  calculatedPreview = signal<PurchaseCalculation>({
    subtotal: 0,
    discountAmount: 0,
    totalCost: 0,
    totalSelling: 0
  });

  // Computed Margin for live input
  computedMargin = computed<number>(() => {
    const cost = Number(this.billForm?.get('standardCost')?.value) || 0;
    const price = Number(this.billForm?.get('standardPrice')?.value) || 0;
    if (price <= 0) return 0;
    return this.calculationService.round(((price - cost) / price) * 100);
  });

  displayedColumns: string[] = [
    'item',
    'batch',
    'standardCost',
    'standardPrice',
    'margin',
    'quantity',
    'freeQty',
    'discountPercentage',
    'totalCost',
    'totalSelling',
    'actions'
  ];

  formSubmitAttempted = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    public authService: AuthService,
    private locationService: LocationService,
    private modulesService: ModulesService,
    private itemCatalogService: ItemCatalogService,
    private calculationService: PurchaseCalculationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // 1. Purchase Bill Fruit Entry Form
    this.billForm = this.fb.group({
      item: ['', [Validators.required]],
      batch: ['', [Validators.required]],
      standardCost: [100, [Validators.required, Validators.min(0)]],
      standardPrice: [150, [Validators.required, Validators.min(0)]],
      quantity: [5, [Validators.required, Validators.min(1)]],
      freeQty: [0, [Validators.min(0)]],
      discountPercentage: [20, [Validators.required, Validators.min(0), Validators.max(100)]]
    });

    // 2. Header Form
    this.headerForm = this.fb.group({
      billNumber: ['', [Validators.required, Validators.maxLength(50)]],
      billDate: [new Date().toISOString().substring(0, 10), [Validators.required]],
      supplierName: ['', [Validators.required, Validators.maxLength(250)]],
      referenceNumber: ['', [Validators.maxLength(100)]],
      status: ['Draft', [Validators.required]],
      remarks: ['', [Validators.maxLength(500)]],
      totalAmount: [0, [Validators.required, Validators.min(0)]]
    });

    // 3. Note Form
    this.noteForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(250)]],
      category: ['General', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required]]
    });

    // 4. Account Form
    this.accountForm = this.fb.group({
      accountCode: ['', [Validators.required, Validators.maxLength(50)]],
      accountName: ['', [Validators.required, Validators.maxLength(250)]],
      accountType: ['Expense', [Validators.required]],
      balance: [0, [Validators.required]],
      status: ['Active', [Validators.required]]
    });

    // 5. Tax Form
    this.taxForm = this.fb.group({
      taxCode: ['', [Validators.required, Validators.maxLength(50)]],
      taxName: ['', [Validators.required, Validators.maxLength(250)]],
      taxRate: [12, [Validators.required, Validators.min(0), Validators.max(100)]],
      status: ['Active', [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Parse route params / data to determine default tab if navigated via URL
    this.route.data.subscribe(data => {
      if (data && data['defaultTab']) {
        this.activeTab.set(data['defaultTab'] as MainTab);
      }
      if (data && data['defaultSubTab']) {
        this.activeSubTab.set(data['defaultSubTab'] as SubTab);
      }
    });

    this.route.queryParamMap.subscribe(params => {
      const tabParam = params.get('tab');
      if (tabParam && ['header', 'details', 'notes', 'documents'].includes(tabParam)) {
        this.activeTab.set(tabParam as MainTab);
      }
      const subTabParam = params.get('subtab');
      if (subTabParam && ['items', 'accounts', 'taxes'].includes(subTabParam)) {
        this.activeSubTab.set(subTabParam as SubTab);
      }
    });

    this.loadLocationsFromDatabase();
    this.filteredItems.set(this.itemCatalogService.getAllowedItems());
    this.updateCalculatedTotals();

    // Subscribe to form value changes for live calculation updates
    this.billForm.valueChanges.subscribe(() => {
      this.updateCalculatedTotals();
    });

    // Handle item autocomplete input changes
    this.billForm.get('item')?.valueChanges.subscribe(value => {
      this.filteredItems.set(this.itemCatalogService.filterItems(value || ''));
    });

    // Load initial data for other modules
    this.loadHeaders();
    this.loadNotes();
    this.loadDocuments();
    this.loadAccounts();
    this.loadTaxes();
  }

  // ======================== Navigation Tab Actions ========================
  public setTab(tab: MainTab): void {
    this.activeTab.set(tab);
    this.clearAlerts();
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge'
    });
  }

  public setSubTab(subTab: SubTab): void {
    this.activeSubTab.set(subTab);
    this.clearAlerts();
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: 'details', subtab: subTab },
      queryParamsHandling: 'merge'
    });
  }

  // ======================== Top Toolbar Actions ========================
  public onTopBarNew(): void {
    const tab = this.activeTab();
    if (tab === 'header') {
      this.openCreateHeaderModal();
    } else if (tab === 'notes') {
      this.openCreateNoteModal();
    } else if (tab === 'documents') {
      const fileInput = document.getElementById('documentFileInput') as HTMLInputElement;
      if (fileInput) fileInput.click();
    } else if (tab === 'details') {
      const subTab = this.activeSubTab();
      if (subTab === 'items') {
        this.billForm.patchValue({
          item: '',
          quantity: 1,
          freeQty: 0
        });
        const itemInput = document.getElementById('itemInput') as HTMLInputElement;
        if (itemInput) itemInput.focus();
      } else if (subTab === 'accounts') {
        this.openCreateAccountModal();
      } else if (subTab === 'taxes') {
        this.openCreateTaxModal();
      }
    }
  }

  public onTopBarSearchToggle(): void {
    this.isSearchBarVisible.update(visible => !visible);
    if (this.isSearchBarVisible()) {
      setTimeout(() => {
        const searchInput = document.querySelector('.active-module-search-input') as HTMLInputElement;
        if (searchInput) searchInput.focus();
      }, 100);
    }
  }

  public onTopBarSettings(): void {
    this.isSettingsModalOpen.set(true);
  }

  public closeSettingsModal(): void {
    this.isSettingsModalOpen.set(false);
  }

  public toggleDarkMode(): void {
    this.isDarkMode.update(v => !v);
  }

  public toggleCompactDensity(): void {
    this.isCompactDensity.update(v => !v);
  }

  // ======================== Locations / Batches ========================
  public loadLocationsFromDatabase(): void {
    this.isLoadingLocations.set(true);
    this.locationLoadError.set('');

    this.locationService.getLocations().subscribe({
      next: (data) => {
        this.isLoadingLocations.set(false);
        this.locations.set(data);
        if (data.length > 0 && !this.billForm.get('batch')?.value) {
          this.billForm.patchValue({ batch: data[0].locationName });
        }
      },
      error: () => {
        this.isLoadingLocations.set(false);
        this.locationLoadError.set('Could not load batches from SQL Server database.');
      }
    });
  }

  // ======================== Purchase Bill Calculations & Rows ========================
  public updateCalculatedTotals(): void {
    const cost = Number(this.billForm.get('standardCost')?.value) || 0;
    const price = Number(this.billForm.get('standardPrice')?.value) || 0;
    const qty = Number(this.billForm.get('quantity')?.value) || 0;
    const discount = Number(this.billForm.get('discountPercentage')?.value) || 0;

    const result = this.calculationService.calculateRow(cost, price, qty, discount);
    this.calculatedPreview.set(result);
  }

  public onAddItem(): void {
    this.formSubmitAttempted = true;
    this.clearAlerts();

    if (this.billForm.invalid) {
      this.billForm.markAllAsTouched();
      return;
    }

    const formValues = this.billForm.value;
    const rawItem = formValues.item?.trim();

    if (!this.itemCatalogService.isValidItem(rawItem)) {
      this.billForm.get('item')?.setErrors({ invalidItem: true });
      return;
    }

    const canonicalItem = this.itemCatalogService.canonicalizeItem(rawItem);
    const selectedLocation = this.locations().find(loc => loc.locationName === formValues.batch);

    const cost = Number(formValues.standardCost);
    const price = Number(formValues.standardPrice);
    const qty = Number(formValues.quantity);
    const freeQty = Number(formValues.freeQty) || 0;
    const discount = Number(formValues.discountPercentage);

    const calc = this.calculationService.calculateRow(cost, price, qty, discount);
    const margin = price > 0 ? this.calculationService.round(((price - cost) / price) * 100) : 0;

    const newItem: PurchaseItem = {
      id: crypto.randomUUID(),
      item: canonicalItem,
      batch: formValues.batch,
      locationCode: selectedLocation?.locationCode,
      standardCost: cost,
      standardPrice: price,
      quantity: qty,
      freeQty: freeQty,
      discountPercentage: discount,
      discountAmount: calc.discountAmount,
      subtotal: calc.subtotal,
      totalCost: calc.totalCost,
      totalSelling: calc.totalSelling,
      margin: margin
    } as any;

    this.purchaseItems.update(items => [...items, newItem]);
    this.successMessage = `Item "${canonicalItem}" added successfully.`;

    this.formSubmitAttempted = false;
    this.billForm.patchValue({
      item: '',
      quantity: 1,
      freeQty: 0
    });
    this.billForm.get('item')?.markAsUntouched();
    this.billForm.get('quantity')?.markAsUntouched();
  }

  public onRemoveItem(id: string): void {
    this.purchaseItems.update(items => items.filter(item => item.id !== id));
    this.successMessage = 'Item removed.';
  }

  // ======================== Header Module ========================
  public loadHeaders(search?: string): void {
    this.isLoadingHeaders.set(true);
    this.modulesService.getHeaders(search).subscribe({
      next: (data) => {
        this.isLoadingHeaders.set(false);
        this.headers.set(data);
      },
      error: () => {
        this.isLoadingHeaders.set(false);
      }
    });
  }

  public onHeaderSearchChange(searchVal: string): void {
    this.headerSearch.set(searchVal);
    this.loadHeaders(searchVal);
  }

  public selectActiveHeader(header: BillHeader): void {
    this.selectedHeaderId.set(header.id);
    this.successMessage = `Active bill set to "${header.billNumber}".`;
  }

  public openCreateHeaderModal(): void {
    this.editingHeader.set(null);
    this.headerForm.reset({
      billNumber: `BILL-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
      billDate: new Date().toISOString().substring(0, 10),
      supplierName: '',
      referenceNumber: '',
      status: 'Draft',
      remarks: '',
      totalAmount: 0
    });
    this.isHeaderModalOpen.set(true);
  }

  public openEditHeaderModal(header: BillHeader): void {
    this.editingHeader.set(header);
    this.headerForm.patchValue({
      billNumber: header.billNumber,
      billDate: header.billDate.substring(0, 10),
      supplierName: header.supplierName,
      referenceNumber: header.referenceNumber || '',
      status: header.status,
      remarks: header.remarks || '',
      totalAmount: header.totalAmount
    });
    this.isHeaderModalOpen.set(true);
  }

  public openViewHeaderModal(header: BillHeader): void {
    this.selectedHeader.set(header);
    this.isViewHeaderModalOpen.set(true);
  }

  public closeHeaderModal(): void {
    this.isHeaderModalOpen.set(false);
    this.editingHeader.set(null);
  }

  public closeViewHeaderModal(): void {
    this.isViewHeaderModalOpen.set(false);
    this.selectedHeader.set(null);
  }

  public saveHeader(): void {
    if (this.headerForm.invalid) {
      this.headerForm.markAllAsTouched();
      return;
    }

    const payload: CreateBillHeaderDto = {
      ...this.headerForm.value,
      billDate: new Date(this.headerForm.value.billDate).toISOString()
    };

    const editItem = this.editingHeader();
    if (editItem) {
      this.modulesService.updateHeader(editItem.id, payload).subscribe({
        next: () => {
          this.closeHeaderModal();
          this.loadHeaders(this.headerSearch());
          this.successMessage = `Header #${payload.billNumber} updated successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to update header.';
        }
      });
    } else {
      this.modulesService.createHeader(payload).subscribe({
        next: () => {
          this.closeHeaderModal();
          this.loadHeaders(this.headerSearch());
          this.successMessage = `Header #${payload.billNumber} created successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to create header.';
        }
      });
    }
  }

  // ======================== Notes Module ========================
  public loadNotes(search?: string): void {
    this.isLoadingNotes.set(true);
    this.modulesService.getNotes(search).subscribe({
      next: (data) => {
        this.isLoadingNotes.set(false);
        this.notes.set(data);
      },
      error: () => {
        this.isLoadingNotes.set(false);
      }
    });
  }

  public onNoteSearchChange(searchVal: string): void {
    this.noteSearch.set(searchVal);
    this.loadNotes(searchVal);
  }

  public openCreateNoteModal(): void {
    this.editingNote.set(null);
    this.noteForm.reset({
      title: '',
      category: 'General',
      description: ''
    });
    this.isNoteModalOpen.set(true);
  }

  public openEditNoteModal(note: BillNote): void {
    this.editingNote.set(note);
    this.noteForm.patchValue({
      title: note.title,
      category: note.category,
      description: note.description
    });
    this.isNoteModalOpen.set(true);
  }

  public openViewNoteModal(note: BillNote): void {
    this.selectedNote.set(note);
    this.isViewNoteModalOpen.set(true);
  }

  public closeNoteModal(): void {
    this.isNoteModalOpen.set(false);
    this.editingNote.set(null);
  }

  public closeViewNoteModal(): void {
    this.isViewNoteModalOpen.set(false);
    this.selectedNote.set(null);
  }

  public saveNote(): void {
    if (this.noteForm.invalid) {
      this.noteForm.markAllAsTouched();
      return;
    }

    const payload: CreateBillNoteDto = this.noteForm.value;
    const editItem = this.editingNote();

    if (editItem) {
      this.modulesService.updateNote(editItem.id, payload).subscribe({
        next: () => {
          this.closeNoteModal();
          this.loadNotes(this.noteSearch());
          this.successMessage = `Note "${payload.title}" updated successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to update note.';
        }
      });
    } else {
      this.modulesService.createNote(payload).subscribe({
        next: () => {
          this.closeNoteModal();
          this.loadNotes(this.noteSearch());
          this.successMessage = `Note "${payload.title}" created successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to create note.';
        }
      });
    }
  }

  // ======================== Documents Module ========================
  public loadDocuments(search?: string): void {
    this.isLoadingDocuments.set(true);
    this.modulesService.getDocuments(search).subscribe({
      next: (data) => {
        this.isLoadingDocuments.set(false);
        this.documents.set(data);
      },
      error: () => {
        this.isLoadingDocuments.set(false);
      }
    });
  }

  public onDocumentSearchChange(searchVal: string): void {
    this.documentSearch.set(searchVal);
    this.loadDocuments(searchVal);
  }

  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.uploadError.set('');

    // Client-side file size validation (max 10MB)
    const maxSizeBytes = 10 * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      this.uploadError.set('File exceeds the 10 MB maximum size limit.');
      input.value = '';
      return;
    }

    // Client-side extension validation
    const allowedExtensions = ['.pdf', '.png', '.jpg', '.jpeg', '.docx', '.xlsx', '.txt', '.csv'];
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!allowedExtensions.includes(ext)) {
      this.uploadError.set(`File type '${ext}' is not permitted. Supported formats: ${allowedExtensions.join(', ')}`);
      input.value = '';
      return;
    }

    this.selectedUploadFile = file;
    this.isUploading.set(true);

    this.modulesService.uploadDocument(file).subscribe({
      next: (uploaded) => {
        this.isUploading.set(false);
        this.selectedUploadFile = null;
        input.value = '';
        this.loadDocuments(this.documentSearch());
        this.successMessage = `Document "${uploaded.fileName}" uploaded successfully.`;
      },
      error: (err) => {
        this.isUploading.set(false);
        input.value = '';
        this.uploadError.set(err.error?.message || 'Failed to upload document.');
      }
    });
  }

  public downloadDocument(doc: BillDocument): void {
    this.modulesService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = `Could not download "${doc.fileName}".`;
      }
    });
  }

  // ======================== Accounts Module ========================
  public loadAccounts(search?: string): void {
    this.isLoadingAccounts.set(true);
    this.modulesService.getAccounts(search).subscribe({
      next: (data) => {
        this.isLoadingAccounts.set(false);
        this.accounts.set(data);
      },
      error: () => {
        this.isLoadingAccounts.set(false);
      }
    });
  }

  public onAccountSearchChange(searchVal: string): void {
    this.accountSearch.set(searchVal);
    this.loadAccounts(searchVal);
  }

  public openCreateAccountModal(): void {
    this.editingAccount.set(null);
    this.accountForm.reset({
      accountCode: `ACC-${Math.floor(1000 + Math.random() * 9000)}`,
      accountName: '',
      accountType: 'Expense',
      balance: 0,
      status: 'Active'
    });
    this.isAccountModalOpen.set(true);
  }

  public openEditAccountModal(account: BillAccount): void {
    this.editingAccount.set(account);
    this.accountForm.patchValue({
      accountCode: account.accountCode,
      accountName: account.accountName,
      accountType: account.accountType,
      balance: account.balance,
      status: account.status
    });
    this.isAccountModalOpen.set(true);
  }

  public closeAccountModal(): void {
    this.isAccountModalOpen.set(false);
    this.editingAccount.set(null);
  }

  public saveAccount(): void {
    if (this.accountForm.invalid) {
      this.accountForm.markAllAsTouched();
      return;
    }

    const payload: CreateBillAccountDto = this.accountForm.value;
    const editItem = this.editingAccount();

    if (editItem) {
      this.modulesService.updateAccount(editItem.id, payload).subscribe({
        next: () => {
          this.closeAccountModal();
          this.loadAccounts(this.accountSearch());
          this.successMessage = `Account "${payload.accountName}" updated successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to update account.';
        }
      });
    } else {
      this.modulesService.createAccount(payload).subscribe({
        next: () => {
          this.closeAccountModal();
          this.loadAccounts(this.accountSearch());
          this.successMessage = `Account "${payload.accountName}" created successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to create account.';
        }
      });
    }
  }

  // ======================== Taxes Module ========================
  public loadTaxes(search?: string): void {
    this.isLoadingTaxes.set(true);
    this.modulesService.getTaxes(search).subscribe({
      next: (data) => {
        this.isLoadingTaxes.set(false);
        this.taxes.set(data);
      },
      error: () => {
        this.isLoadingTaxes.set(false);
      }
    });
  }

  public onTaxSearchChange(searchVal: string): void {
    this.taxSearch.set(searchVal);
    this.loadTaxes(searchVal);
  }

  public openCreateTaxModal(): void {
    this.editingTax.set(null);
    this.taxForm.reset({
      taxCode: `TAX-${Math.floor(100 + Math.random() * 900)}`,
      taxName: '',
      taxRate: 10,
      status: 'Active'
    });
    this.isTaxModalOpen.set(true);
  }

  public openEditTaxModal(tax: BillTax): void {
    this.editingTax.set(tax);
    this.taxForm.patchValue({
      taxCode: tax.taxCode,
      taxName: tax.taxName,
      taxRate: tax.taxRate,
      status: tax.status
    });
    this.isTaxModalOpen.set(true);
  }

  public closeTaxModal(): void {
    this.isTaxModalOpen.set(false);
    this.editingTax.set(null);
  }

  public saveTax(): void {
    if (this.taxForm.invalid) {
      this.taxForm.markAllAsTouched();
      return;
    }

    const payload: CreateBillTaxDto = this.taxForm.value;
    const editItem = this.editingTax();

    if (editItem) {
      this.modulesService.updateTax(editItem.id, payload).subscribe({
        next: () => {
          this.closeTaxModal();
          this.loadTaxes(this.taxSearch());
          this.successMessage = `Tax "${payload.taxName}" updated successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to update tax.';
        }
      });
    } else {
      this.modulesService.createTax(payload).subscribe({
        next: () => {
          this.closeTaxModal();
          this.loadTaxes(this.taxSearch());
          this.successMessage = `Tax "${payload.taxName}" created successfully.`;
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to create tax.';
        }
      });
    }
  }

  // ======================== Generic Delete Confirmation ========================
  public promptDelete(type: 'header' | 'note' | 'document' | 'account' | 'tax', id: number, title: string): void {
    this.deleteTarget.set({ type, id, title });
    this.isDeleteConfirmModalOpen.set(true);
  }

  public cancelDelete(): void {
    this.isDeleteConfirmModalOpen.set(false);
    this.deleteTarget.set(null);
  }

  public executeDelete(): void {
    const target = this.deleteTarget();
    if (!target) return;

    this.isDeleteConfirmModalOpen.set(false);

    if (target.type === 'header') {
      this.modulesService.deleteHeader(target.id).subscribe({
        next: () => {
          this.loadHeaders(this.headerSearch());
          this.successMessage = `Header "${target.title}" deleted.`;
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete header.'
      });
    } else if (target.type === 'note') {
      this.modulesService.deleteNote(target.id).subscribe({
        next: () => {
          this.loadNotes(this.noteSearch());
          this.successMessage = `Note "${target.title}" deleted.`;
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete note.'
      });
    } else if (target.type === 'document') {
      this.modulesService.deleteDocument(target.id).subscribe({
        next: () => {
          this.loadDocuments(this.documentSearch());
          this.successMessage = `Document "${target.title}" deleted.`;
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete document.'
      });
    } else if (target.type === 'account') {
      this.modulesService.deleteAccount(target.id).subscribe({
        next: () => {
          this.loadAccounts(this.accountSearch());
          this.successMessage = `Account "${target.title}" deleted.`;
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete account.'
      });
    } else if (target.type === 'tax') {
      this.modulesService.deleteTax(target.id).subscribe({
        next: () => {
          this.loadTaxes(this.taxSearch());
          this.successMessage = `Tax "${target.title}" deleted.`;
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete tax.'
      });
    }

    this.deleteTarget.set(null);
  }

  // ======================== Helpers ========================
  public formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  public clearAlerts(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }

  public onLogout(): void {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])
    });
  }
}
