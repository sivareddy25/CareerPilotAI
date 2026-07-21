import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppShellComponent } from '../../shell/app-shell.component';

@Component({
  selector: 'app-authenticated-layout',
  standalone: true,
  imports: [AppShellComponent],
  template: `
    <app-shell />
  `,
  styleUrl: './authenticated-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthenticatedLayoutComponent {}
