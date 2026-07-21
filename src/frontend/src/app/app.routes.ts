import { Routes } from '@angular/router';
import { AuthenticatedLayoutComponent } from './layouts/authenticated-layout/authenticated-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
    // guestGuard sits on the parent so it covers every child at once. A signed-in user
    // reaching any of these is redirected home rather than offered a second session.
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/authentication/login/login.component'),
        title: 'CareerPilot AI - Sign In',
      },
      {
        path: 'register',
        loadComponent: () => import('./features/authentication/register/register.component'),
        title: 'CareerPilot AI - Create Account',
      },
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '',
    component: AuthenticatedLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/home.component'),
        title: 'CareerPilot AI - Home',
      },
    ],
  },
  {
    path: 'not-found',
    loadComponent: () => import('./features/not-found/not-found.component'),
    title: 'CareerPilot AI - Page Not Found',
  },
  {
    path: '**',
    redirectTo: 'not-found',
  },
];
