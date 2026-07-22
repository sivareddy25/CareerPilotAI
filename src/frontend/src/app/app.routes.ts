import { Routes } from '@angular/router';
import { AuthenticatedLayoutComponent } from './layouts/authenticated-layout/authenticated-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { onboardingGuard } from './core/guards/onboarding.guard';

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
    path: 'onboarding',
    loadComponent: () =>
      import('./features/onboarding/onboarding-wizard.component').then((m) => m.OnboardingWizardComponent),
    title: 'CareerPilot AI - First-Time Candidate Setup',
  },
  {
    path: '',
    component: AuthenticatedLayoutComponent,
    // onboardingGuard redirects first-run users (no completed profile) to the wizard. It sits
    // after authGuard so the check only runs for an authenticated user, and it guards the whole
    // authenticated area — the /onboarding route lives outside this layout, so there is no loop.
    canActivate: [authGuard, onboardingGuard],
    children: [
      {
        path: '',
        redirectTo: 'onboarding',
        pathMatch: 'full',
      },
      {
        path: 'design-system',
        loadComponent: () => import('./features/design-system/design-system.component'),
        title: 'CareerPilot AI - Design System Infrastructure',
      },
      {
        path: 'communication',
        loadComponent: () =>
          import('./features/communication/communication-hub.component').then((m) => m.CommunicationHubComponent),
        title: 'CareerPilot AI - Smart Communication Hub',
      },
      {
        path: 'system/settings',
        loadComponent: () =>
          import('./features/system/local-system-settings.component').then((m) => m.LocalSystemSettingsComponent),
        title: 'CareerPilot AI - System Management & Health',
      },
      {
        path: 'jobs',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/jobs/browser/job-browser.component').then(m => m.JobBrowserComponent),
            title: 'CareerPilot AI - Job Ingestion Engine',
          },
          {
            path: 'companies',
            loadComponent: () => import('./features/jobs/company/company-details.component').then(m => m.CompanyDetailsComponent),
            title: 'CareerPilot AI - Hiring Companies',
          },
          {
            path: 'saved',
            loadComponent: () => import('./features/jobs/saved/saved-jobs-placeholder.component').then(m => m.SavedJobsPlaceholderComponent),
            title: 'CareerPilot AI - Saved Jobs',
          },
          {
            path: ':id',
            loadComponent: () => import('./features/jobs/details/job-details.component').then(m => m.JobDetailsComponent),
            title: 'CareerPilot AI - Job Details',
          },
        ],
      },
      {
        path: 'resumes',
        children: [
          {
            path: 'templates',
            loadComponent: () => import('./features/resumes/template-gallery/template-gallery.component').then(m => m.TemplateGalleryComponent),
            title: 'CareerPilot AI - Resume Templates',
          },
          {
            path: 'import',
            loadComponent: () => import('./features/resumes/import-wizard/import-wizard.component').then(m => m.ImportWizardComponent),
            title: 'CareerPilot AI - Import Resumes',
          },
          {
            path: 'preview',
            loadComponent: () => import('./features/resumes/resume-preview/resume-preview.component').then(m => m.ResumePreviewComponent),
            title: 'CareerPilot AI - Live Resume Preview',
          },
          {
            path: 'settings',
            loadComponent: () => import('./features/resumes/template-settings/template-settings.component').then(m => m.TemplateSettingsComponent),
            title: 'CareerPilot AI - Template Settings',
          },
          {
            path: 'print',
            loadComponent: () => import('./features/resumes/print-preview/print-preview.component').then(m => m.PrintPreviewComponent),
            title: 'CareerPilot AI - Print Preview',
          },
          {
            path: '',
            redirectTo: 'templates',
            pathMatch: 'full',
          },
        ],
      },
      {
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
            path: 'career',
            loadComponent: () =>
              import('./features/profile/career/career-profile.component').then((m) => m.CareerProfileComponent),
            title: 'CareerPilot AI - Career Profile',
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
