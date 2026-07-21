import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ExecutiveDashboardComponent } from '../dashboard/executive-dashboard.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ExecutiveDashboardComponent],
  template: `
    <app-executive-dashboard />
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class HomeComponent {}
