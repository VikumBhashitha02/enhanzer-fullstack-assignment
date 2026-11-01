import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
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
} from '../models/modules.models';

@Injectable({
  providedIn: 'root'
})
export class ModulesService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ======================== Headers ========================
  public getHeaders(search?: string): Observable<BillHeader[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<BillHeader[]>(`${this.apiUrl}/headers`, { params });
  }

  public getHeader(id: number): Observable<BillHeader> {
    return this.http.get<BillHeader>(`${this.apiUrl}/headers/${id}`);
  }

  public createHeader(dto: CreateBillHeaderDto): Observable<BillHeader> {
    return this.http.post<BillHeader>(`${this.apiUrl}/headers`, dto);
  }

  public updateHeader(id: number, dto: CreateBillHeaderDto): Observable<BillHeader> {
    return this.http.put<BillHeader>(`${this.apiUrl}/headers/${id}`, dto);
  }

  public deleteHeader(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/headers/${id}`);
  }

  // ======================== Notes ========================
  public getNotes(search?: string): Observable<BillNote[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<BillNote[]>(`${this.apiUrl}/notes`, { params });
  }

  public getNote(id: number): Observable<BillNote> {
    return this.http.get<BillNote>(`${this.apiUrl}/notes/${id}`);
  }

  public createNote(dto: CreateBillNoteDto): Observable<BillNote> {
    return this.http.post<BillNote>(`${this.apiUrl}/notes`, dto);
  }

  public updateNote(id: number, dto: CreateBillNoteDto): Observable<BillNote> {
    return this.http.put<BillNote>(`${this.apiUrl}/notes/${id}`, dto);
  }

  public deleteNote(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/notes/${id}`);
  }

  // ======================== Documents ========================
  public getDocuments(search?: string): Observable<BillDocument[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<BillDocument[]>(`${this.apiUrl}/documents`, { params });
  }

  public uploadDocument(file: File): Observable<BillDocument> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<BillDocument>(`${this.apiUrl}/documents/upload`, formData);
  }

  public downloadDocument(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/documents/${id}/download`, {
      responseType: 'blob'
    });
  }

  public deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/documents/${id}`);
  }

  // ======================== Accounts ========================
  public getAccounts(search?: string): Observable<BillAccount[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<BillAccount[]>(`${this.apiUrl}/accounts`, { params });
  }

  public getAccount(id: number): Observable<BillAccount> {
    return this.http.get<BillAccount>(`${this.apiUrl}/accounts/${id}`);
  }

  public createAccount(dto: CreateBillAccountDto): Observable<BillAccount> {
    return this.http.post<BillAccount>(`${this.apiUrl}/accounts`, dto);
  }

  public updateAccount(id: number, dto: CreateBillAccountDto): Observable<BillAccount> {
    return this.http.put<BillAccount>(`${this.apiUrl}/accounts/${id}`, dto);
  }

  public deleteAccount(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/accounts/${id}`);
  }

  // ======================== Taxes ========================
  public getTaxes(search?: string): Observable<BillTax[]> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<BillTax[]>(`${this.apiUrl}/taxes`, { params });
  }

  public getTax(id: number): Observable<BillTax> {
    return this.http.get<BillTax>(`${this.apiUrl}/taxes/${id}`);
  }

  public createTax(dto: CreateBillTaxDto): Observable<BillTax> {
    return this.http.post<BillTax>(`${this.apiUrl}/taxes`, dto);
  }

  public updateTax(id: number, dto: CreateBillTaxDto): Observable<BillTax> {
    return this.http.put<BillTax>(`${this.apiUrl}/taxes/${id}`, dto);
  }

  public deleteTax(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/taxes/${id}`);
  }
}

