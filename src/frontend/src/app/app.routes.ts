import { Routes } from '@angular/router';
import { AuthenticatedLayoutComponent } from './layouts/authenticated-layout/authenticated-layout.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
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
