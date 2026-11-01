export interface BillHeader {
  id: number;
  companyCode: string;
  billNumber: string;
  billDate: string;
  supplierName: string;
  referenceNumber?: string;
  status: string;
  remarks?: string;
  totalAmount: number;
  createdBy?: string;
  createdAtUtc: string;
}

export interface CreateBillHeaderDto {
  billNumber: string;
  billDate: string;
  supplierName: string;
  referenceNumber?: string;
  status: string;
  remarks?: string;
  totalAmount: number;
}

export interface BillNote {
  id: number;
  companyCode: string;
  title: string;
  description: string;
  category: string;
  createdBy?: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateBillNoteDto {
  title: string;
  description: string;
  category: string;
}

export interface BillDocument {
  id: number;
  companyCode: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedBy?: string;
  uploadedAtUtc: string;
}

export interface BillAccount {
  id: number;
  companyCode: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  balance: number;
  status: string;
  createdBy?: string;
  createdAtUtc: string;
}

export interface CreateBillAccountDto {
  accountCode: string;
  accountName: string;
  accountType: string;
  balance: number;
  status: string;
}

export interface BillTax {
  id: number;
  companyCode: string;
  taxCode: string;
  taxName: string;
  taxRate: number;
  status: string;
  createdBy?: string;
  createdAtUtc: string;
}

export interface CreateBillTaxDto {
  taxCode: string;
  taxName: string;
  taxRate: number;
  status: string;
}

