import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../../core/services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'isAuthenticated']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate', 'navigateByUrl']);
    authServiceSpy.isAuthenticated.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, NoopAnimationsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParams: {}
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create login component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize login form with empty values and invalid status', () => {
    expect(component.loginForm.valid).toBeFalse();
    expect(component.loginForm.get('email')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('should validate email format and requirement', () => {
    const emailControl = component.loginForm.get('email');

    emailControl?.setValue('');
    expect(emailControl?.hasError('required')).toBeTrue();

    emailControl?.setValue('not-an-email');
    expect(emailControl?.hasError('email')).toBeTrue();

    emailControl?.setValue('info@enhanzer.com');
    expect(emailControl?.valid).toBeTrue();
  });

  it('should validate password requirement', () => {
    const passwordControl = component.loginForm.get('password');

    passwordControl?.setValue('');
    expect(passwordControl?.hasError('required')).toBeTrue();

    passwordControl?.setValue('Welcome#5');
    expect(passwordControl?.valid).toBeTrue();
  });

  it('should toggle password visibility flag', () => {
    expect(component.hidePassword).toBeTrue();
    component.togglePasswordVisibility();
    expect(component.hidePassword).toBeFalse();
    component.togglePasswordVisibility();
    expect(component.hidePassword).toBeTrue();
  });

  it('should submit credentials and navigate to purchase bill on success', () => {
    component.loginForm.patchValue({
      email: 'info@enhanzer.com',
      password: 'Welcome#5'
    });

    authServiceSpy.login.and.returnValue(of({
      success: true,
      message: 'Login successful.',
      locationsCount: 5
    }));

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith({
      email: 'info@enhanzer.com',
      password: 'Welcome#5'
    });
    expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/purchase-bill');
  });

  it('should display error message on authentication failure', () => {
    component.loginForm.patchValue({
      email: 'info@enhanzer.com',
      password: 'WrongPassword'
    });

    authServiceSpy.login.and.returnValue(throwError(() => ({
      status: 401,
      error: { message: 'Invalid email or password.' }
    })));

    component.onSubmit();

    expect(component.errorMessage).toBe('Invalid email or password.');
    expect(component.isLoading).toBeFalse();
  });
});

