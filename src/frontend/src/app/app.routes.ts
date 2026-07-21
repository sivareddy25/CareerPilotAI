import { Routes } from '@angular/router';
import { AuthenticatedLayoutComponent } from './layouts/authenticated-layout/authenticated-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
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
      {
        path: 'design-system',
        loadComponent: () => import('./features/design-system/design-system.component'),
        title: 'CareerPilot AI - Design System Infrastructure',
      },
      {
        // The shell is loaded once and hosts every section, so switching tabs swaps
        // only the child chunk rather than re-rendering the header and tab strip.
        path: 'profile',
        loadComponent: () => import('./features/profile/profile-shell.component'),
        children: [
          {
            path: 'overview',
            loadComponent: () => import('./features/profile/overview/profile-overview.component'),
            title: 'CareerPilot AI - Profile',
          },
          {
            path: 'edit',
            loadComponent: () => import('./features/profile/edit/profile-edit.component'),
            title: 'CareerPilot AI - Edit Profile',
          },
          {
            path: 'account',
            loadComponent: () => import('./features/profile/account/account-settings.component'),
            title: 'CareerPilot AI - Account Settings',
          },
          {
            path: 'security',
            loadComponent: () => import('./features/profile/security/security-settings.component'),
            title: 'CareerPilot AI - Security Settings',
          },
          {
            path: 'notifications',
            loadComponent: () =>
              import('./features/profile/notifications/notification-settings.component'),
            title: 'CareerPilot AI - Notification Settings',
          },
          {
            path: 'appearance',
            loadComponent: () =>
              import('./features/profile/appearance/appearance-settings.component'),
            title: 'CareerPilot AI - Appearance Settings',
          },
          {
            path: '',
            redirectTo: 'overview',
            pathMatch: 'full',
          },
        ],
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
