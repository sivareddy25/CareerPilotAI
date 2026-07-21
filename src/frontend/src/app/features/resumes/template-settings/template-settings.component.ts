import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  CardComponent,
  SelectComponent,
  SelectOption,
  PageHeaderComponent,
  ToggleComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-template-settings',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    SelectComponent,
    PageHeaderComponent,
    ToggleComponent,
  ],
  template: `
    <div class="settings-container">
      <app-page-header
        title="Template Customization Settings"
        subtitle="Configure font preferences, accent colors, and layout section order."
      />

      <div class="settings-grid">
        <app-card title="Typography & Styling">
          <div class="form-stack">
            <app-select label="Heading Font" [options]="fontOptions" value="Georgia" />
            <app-select label="Body Font" [options]="fontOptions" value="Calibri" />
            <app-select label="Base Font Size" [options]="fontSizeOptions" value="10.5" />
          </div>
        </app-card>

        <app-card title="Section Visibility">
          <div class="form-stack">
            <app-toggle label="Show Professional Summary" [checked]="true" />
            <app-toggle label="Show Projects Section" [checked]="true" />
            <app-toggle label="Show Certifications Section" [checked]="true" />
          </div>
        </app-card>
      </div>
    </div>
  `,
  styles: [`
    .settings-container {
      padding: var(--space-6);
      max-width: 1000px;
      margin: 0 auto;
    }
    .settings-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-6);
      margin-top: var(--space-6);
    }
    .form-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TemplateSettingsComponent {
  protected readonly fontOptions: SelectOption[] = [
    { label: 'Arial', value: 'Arial' },
    { label: 'Calibri', value: 'Calibri' },
    { label: 'Georgia', value: 'Georgia' },
    { label: 'Helvetica', value: 'Helvetica' },
    { label: 'Times New Roman', value: 'Times New Roman' },
    { label: 'Verdana', value: 'Verdana' },
  ];

  protected readonly fontSizeOptions: SelectOption[] = [
    { label: '10 pt (Compact)', value: '10' },
    { label: '10.5 pt (Standard)', value: '10.5' },
    { label: '11 pt (Generous)', value: '11' },
  ];
}
