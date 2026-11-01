import { Routes } from '@angular/router';
import { LoginComponent } from './features/login/login.component';
import { PurchaseBillComponent } from './features/purchase-bill/purchase-bill.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'purchase-bill',
    component: PurchaseBillComponent,
    canActivate: [authGuard]
  },
  {
    path: 'header',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'header' }
  },
  {
    path: 'headers',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'header' }
  },
  {
    path: 'notes',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'notes' }
  },
  {
    path: 'documents',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'documents' }
  },
  {
    path: 'accounts',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'details', defaultSubTab: 'accounts' }
  },
  {
    path: 'taxes',
    component: PurchaseBillComponent,
    canActivate: [authGuard],
    data: { defaultTab: 'details', defaultSubTab: 'taxes' }
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
