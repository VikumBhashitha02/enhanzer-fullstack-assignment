import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { signal } from '@angular/core';

describe('authGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    routerSpy = jasmine.createSpyObj('Router', ['createUrlTree']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
  });

  it('should allow access when user is authenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);

    const dummyRoute = {} as ActivatedRouteSnapshot;
    const dummyState = { url: '/purchase-bill' } as RouterStateSnapshot;

    const result = TestBed.runInInjectionContext(() => authGuard(dummyRoute, dummyState));

    expect(result).toBeTrue();
  });

  it('should redirect to /login with returnUrl when user is not authenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);
    const mockTree = {} as any;
    routerSpy.createUrlTree.and.returnValue(mockTree);

    const dummyRoute = {} as ActivatedRouteSnapshot;
    const dummyState = { url: '/purchase-bill' } as RouterStateSnapshot;

    const result = TestBed.runInInjectionContext(() => authGuard(dummyRoute, dummyState));

    expect(routerSpy.createUrlTree).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/purchase-bill' } });
    expect(result).toBe(mockTree);
  });
});

