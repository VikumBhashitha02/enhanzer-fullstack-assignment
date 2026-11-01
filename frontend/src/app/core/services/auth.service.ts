import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, UserInfo } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;

  // Modern Angular signal for reactive auth state
  private currentUserSignal = signal<UserInfo | null>(null);
  public currentUser = this.currentUserSignal.asReadonly();
  public isAuthenticated = computed(() => this.currentUserSignal() !== null);

  constructor(private http: HttpClient) {
    this.restoreSessionFromStorage();
  }

  public login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials).pipe(
      tap(response => {
        if (response.success && response.user) {
          this.currentUserSignal.set(response.user);
          // Only safe display metadata in session storage for refresh persistence
          sessionStorage.setItem('enhanzer_user', JSON.stringify(response.user));
        }
      })
    );
  }

  public logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/logout`, {}).pipe(
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return of(null);
      })
    );
  }

  public checkSession(): Observable<UserInfo | null> {
    return this.http.get<UserInfo>(`${this.apiUrl}/auth/me`).pipe(
      tap(user => {
        this.currentUserSignal.set(user);
        sessionStorage.setItem('enhanzer_user', JSON.stringify(user));
      }),
      catchError(() => {
        this.clearSession();
        return of(null);
      })
    );
  }

  public clearSession(): void {
    this.currentUserSignal.set(null);
    sessionStorage.removeItem('enhanzer_user');
  }

  private restoreSessionFromStorage(): void {
    try {
      const stored = sessionStorage.getItem('enhanzer_user');
      if (stored) {
        const user: UserInfo = JSON.parse(stored);
        this.currentUserSignal.set(user);
      }
    } catch {
      this.clearSession();
    }
  }
}

