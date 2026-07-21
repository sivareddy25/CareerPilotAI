import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ButtonComponent,
  IconButtonComponent,
  IconComponent,
  CardComponent,
  AvatarComponent,
  BadgeComponent,
  ChipComponent,
  TabsComponent,
  TabItem,
  AccordionComponent,
  AccordionItem,
  BreadcrumbComponent,
  PaginationComponent,
  ProgressBarComponent,
  SpinnerComponent,
  SkeletonComponent,
  EmptyStateComponent,
  InputComponent,
  TextareaComponent,
  SelectComponent,
  CheckboxComponent,
  RadioComponent,
  ToggleComponent,
  SearchBarComponent,
  DatePickerComponent,
  FileUploadComponent,
  DataTableComponent,
  ColumnDef,
  DialogComponent,
  DrawerComponent,
  DropdownComponent,
  DropdownItem,
  ContextMenuComponent,
  ContextMenuItem,
  CommandBarComponent,
  PageHeaderComponent,
} from '../../shared/components';
import { TooltipDirective } from '../../shared/directives/tooltip.directive';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-design-system',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    IconButtonComponent,
    IconComponent,
    CardComponent,
    AvatarComponent,
    BadgeComponent,
    ChipComponent,
    TabsComponent,
    AccordionComponent,
    BreadcrumbComponent,
    PaginationComponent,
    ProgressBarComponent,
    SpinnerComponent,
    SkeletonComponent,
    EmptyStateComponent,
    InputComponent,
    TextareaComponent,
    SelectComponent,
    CheckboxComponent,
    RadioComponent,
    ToggleComponent,
    SearchBarComponent,
    DatePickerComponent,
    FileUploadComponent,
    DataTableComponent,
    DialogComponent,
    DrawerComponent,
    DropdownComponent,
    ContextMenuComponent,
    CommandBarComponent,
    PageHeaderComponent,
    TooltipDirective,
  ],
  template: `
    <div class="ds-page">
      <app-breadcrumb [items]="breadcrumbItems" />

      <app-page-header
        title="CareerPilot AI Design System"
        subtitle="Production-grade UI infrastructure inspired by Microsoft Fluent 2 aesthetics."
      >
        <div header-actions class="ds-header-actions">
          <app-button variant="outline" (btnClick)="themeService.setTheme('light')">☀️ Light</app-button>
          <app-button variant="outline" (btnClick)="themeService.setTheme('dark')">🌙 Dark</app-button>
          <app-button variant="outline" (btnClick)="themeService.setTheme('high-contrast')">👁️ High Contrast</app-button>
        </div>
      </app-page-header>

      <app-tabs
        [items]="categoryTabs"
        [activeId]="activeCategory()"
        (tabChange)="activeCategory.set($event)"
      />

      <!-- TAB 1: DESIGN TOKENS -->
      @if (activeCategory() === 'tokens') {
        <div class="ds-section">
          <h2 class="section-title">Color Palette Tokens</h2>
          <div class="color-grid">
            <div class="color-card brand-bg">
              <span class="color-name">Brand Primary</span>
              <span class="color-code">var(--brand-primary)</span>
            </div>
            <div class="color-card success-bg">
              <span class="color-name">Success</span>
              <span class="color-code">var(--success)</span>
            </div>
            <div class="color-card warning-bg">
              <span class="color-name">Warning</span>
              <span class="color-code">var(--warning)</span>
            </div>
            <div class="color-card danger-bg">
              <span class="color-name">Danger</span>
              <span class="color-code">var(--danger)</span>
            </div>
            <div class="color-card info-bg">
              <span class="color-name">Info</span>
              <span class="color-code">var(--info)</span>
            </div>
          </div>

          <h2 class="section-title">Typography Scale</h2>
          <app-card>
            <div class="typo-list">
              <div class="typo-item"><span class="typo-label">Display</span><span class="typo-display">Display Header 40px</span></div>
              <div class="typo-item"><span class="typo-label">Heading 1</span><span class="typo-h1">H1 Heading 32px</span></div>
              <div class="typo-item"><span class="typo-label">Heading 2</span><span class="typo-h2">H2 Heading 24px</span></div>
              <div class="typo-item"><span class="typo-label">Heading 3</span><span class="typo-h3">H3 Heading 20px</span></div>
              <div class="typo-item"><span class="typo-label">Body Medium</span><span class="typo-body">Standard Body Text 16px</span></div>
              <div class="typo-item"><span class="typo-label">Caption</span><span class="typo-caption">Caption Text 12px</span></div>
            </div>
          </app-card>
        </div>
      }

      <!-- TAB 2: BUTTONS & ACTIONS -->
      @if (activeCategory() === 'buttons') {
        <div class="ds-section">
          <h2 class="section-title">Button Variants</h2>
          <div class="component-row">
            <app-button variant="primary" appTooltip="Primary action button">Primary Button</app-button>
            <app-button variant="secondary">Secondary Button</app-button>
            <app-button variant="outline">Outline Button</app-button>
            <app-button variant="ghost">Ghost Button</app-button>
            <app-button variant="danger">Danger Button</app-button>
            <app-button variant="primary" [loading]="true">Loading State</app-button>
            <app-button variant="primary" [disabled]="true">Disabled</app-button>
          </div>

          <h2 class="section-title">Button Sizes</h2>
          <div class="component-row">
            <app-button variant="primary" size="sm">Small (32px)</app-button>
            <app-button variant="primary" size="md">Medium (40px)</app-button>
            <app-button variant="primary" size="lg">Large (48px)</app-button>
          </div>

          <h2 class="section-title">Icon Buttons</h2>
          <div class="component-row">
            <app-icon-button icon="search" ariaLabel="Search" />
            <app-icon-button icon="bell" ariaLabel="Notifications" variant="primary" />
            <app-icon-button icon="settings" ariaLabel="Settings" variant="outline" />
            <app-icon-button icon="trash" ariaLabel="Delete" />
          </div>

          <h2 class="section-title">Dropdown & Context Menu Triggers</h2>
          <div class="component-row">
            <app-dropdown [items]="dropdownItems">
              <app-button trigger variant="outline">
                Dropdown Menu <app-icon name="chevron-down" size="xs" />
              </app-button>
            </app-dropdown>

            <app-context-menu [items]="contextMenuItems">
              <div class="context-demo-box">
                Right-Click inside this container to trigger Context Menu
              </div>
            </app-context-menu>
          </div>
        </div>
      }

      <!-- TAB 3: FORM CONTROLS -->
      @if (activeCategory() === 'forms') {
        <div class="ds-section">
          <div class="grid-2">
            <app-card title="Text Inputs & Controls">
              <div class="form-stack">
                <app-input label="Standard Input" placeholder="Enter text..." leadingIcon="user" />
                <app-input label="Input with Error" value="Invalid entry" error="Please enter a valid email address." leadingIcon="mail" />
                <app-textarea label="Multiline Textarea" placeholder="Enter long description..." [maxLength]="200" />
                <app-select label="Select Dropdown" [options]="selectOptions" />
              </div>
            </app-card>

            <app-card title="Checkboxes, Radios & Toggles">
              <div class="form-stack">
                <app-checkbox label="I accept the Terms and Conditions" [checked]="true" />
                <app-checkbox label="Indeterminate State" [indeterminate]="true" />
                <app-toggle label="Enable Real-Time Notifications" [checked]="isToggleActive()" (checkedChange)="isToggleActive.set($event)" />
                <app-radio label="Notification Frequency" [options]="radioOptions" value="daily" />
                <app-search-bar placeholder="Filter items..." shortcutKey="⌘K" />
              </div>
            </app-card>
          </div>

          <div class="grid-2" style="margin-top: var(--space-6)">
            <app-card title="Date Picker (Placeholder Component)">
              <app-date-picker label="Target Completion Date" />
            </app-card>

            <app-card title="File Upload (Placeholder Component)">
              <app-file-upload label="Upload Resume / Portfolio" />
            </app-card>
          </div>
        </div>
      }

      <!-- TAB 4: SURFACES & DISPLAY -->
      @if (activeCategory() === 'display') {
        <div class="ds-section">
          <h2 class="section-title">Cards & Glassmorphism Surfaces</h2>
          <div class="grid-3">
            <app-card title="Elevated Card" subtitle="Default shadow & border" variant="elevated">
              Card body with Fluent 2 shadow elevation.
            </app-card>
            <app-card title="Glass Card" subtitle="Backdrop blur tint" variant="glass">
              Modern glassmorphism translucent container.
            </app-card>
            <app-card title="Hoverable Card" subtitle="Interactive hover float" [hoverable]="true">
              Slight translateY float on mouse hover.
            </app-card>
          </div>

          <h2 class="section-title">Accordion Collapsible Panels</h2>
          <app-accordion [items]="accordionItems" />

          <h2 class="section-title">Badges, Chips & Avatars</h2>
          <div class="component-row">
            <app-badge variant="primary">Primary Soft</app-badge>
            <app-badge variant="success" styleMode="solid">Success Solid</app-badge>
            <app-badge variant="warning" styleMode="outline">Warning Outline</app-badge>
            <app-badge variant="danger">Danger Soft</app-badge>
            <app-badge variant="info">Info Soft</app-badge>
          </div>

          <div class="component-row" style="margin-top: var(--space-4)">
            <app-chip icon="sparkles" [clickable]="true">Interactive Chip</app-chip>
            <app-chip icon="check" [selected]="true">Selected Tag</app-chip>
            <app-chip [removable]="true">Removable Filter</app-chip>
          </div>

          <div class="component-row" style="margin-top: var(--space-4)">
            <app-avatar name="Sarah Jenkins" size="xs" />
            <app-avatar name="David Miller" size="sm" status="online" />
            <app-avatar name="CareerPilot Admin" size="md" status="busy" />
            <app-avatar name="Elena Rostova" size="lg" status="away" />
            <app-avatar name="Principal UI Architect" size="xl" status="online" />
          </div>
        </div>
      }

      <!-- TAB 5: DATA PRESENTATION -->
      @if (activeCategory() === 'data') {
        <div class="ds-section">
          <h2 class="section-title">Data Table & Pagination Infrastructure</h2>
          <app-data-table [columns]="tableColumns" [data]="tableData" [selectable]="true" />
          <app-pagination [currentPage]="1" [pageSize]="10" [totalItems]="48" />
        </div>
      }

      <!-- TAB 6: OVERLAYS & FEEDBACK -->
      @if (activeCategory() === 'overlays') {
        <div class="ds-section">
          <h2 class="section-title">Modals, Drawers & Popups</h2>
          <div class="component-row">
            <app-button variant="primary" (btnClick)="isDialogOpen.set(true)">Open Dialog</app-button>
            <app-button variant="secondary" (btnClick)="isDrawerOpen.set(true)">Open Drawer</app-button>
            <app-button variant="outline" (btnClick)="isCommandBarOpen.set(true)">Open Cmd+K Bar</app-button>
            <app-button variant="success" (btnClick)="triggerToast()">Trigger Toast Notification</app-button>
          </div>

          <h2 class="section-title">Progress & Loading Loaders</h2>
          <div class="form-stack">
            <app-progress-bar [value]="65" variant="primary" />
            <app-progress-bar [indeterminate]="true" variant="success" />

            <div class="component-row" style="margin-top: var(--space-4)">
              <app-spinner size="sm" />
              <app-spinner size="md" />
              <app-spinner size="lg" />
            </div>

            <div style="margin-top: var(--space-4)">
              <app-skeleton type="text" width="60%" />
              <app-skeleton type="rect" height="60px" />
            </div>
          </div>

          <h2 class="section-title">Empty State Placeholder</h2>
          <app-empty-state title="No resumes uploaded" description="Get started by uploading your first master resume document.">
            <div actions>
              <app-button variant="primary">Upload Resume</app-button>
            </div>
          </app-empty-state>
        </div>
      }
    </div>

    <!-- Dialog Modal Container -->
    <app-dialog [isOpen]="isDialogOpen()" title="Fluent 2 Confirmation Dialog" (closed)="isDialogOpen.set(false)">
      <p>This is a production-grade accessible modal container with focus trapping, backdrop click close, and Esc key bindings.</p>
      <div dialog-footer>
        <app-button variant="secondary" (btnClick)="isDialogOpen.set(false)">Cancel</app-button>
        <app-button variant="primary" (btnClick)="isDialogOpen.set(false)">Confirm Action</app-button>
      </div>
    </app-dialog>

    <!-- Drawer Container -->
    <app-drawer [isOpen]="isDrawerOpen()" title="Side Panel Filter Drawer" (closed)="isDrawerOpen.set(false)">
      <p>Accessible side drawer container for slide-in filters and detailed inspection panels.</p>
    </app-drawer>

    <!-- Command Bar -->
    <app-command-bar [isOpen]="isCommandBarOpen()" (closed)="isCommandBarOpen.set(false)" />
  `,
  styles: [`
    .ds-page {
      padding: var(--space-6);
      max-width: 1280px;
      margin: 0 auto;
    }
    .ds-header-actions {
      display: flex;
      gap: var(--space-2);
    }
    .ds-section {
      margin-top: var(--space-6);
      animation: fadeIn var(--duration-fast) var(--ease-fluent);
    }
    .section-title {
      font-size: var(--text-h3);
      font-weight: 600;
      color: var(--text-primary);
      margin: var(--space-6) 0 var(--space-4) 0;

      &:first-child { margin-top: 0; }
    }
    .context-demo-box {
      padding: var(--space-4) var(--space-6);
      border: 1px dashed var(--border-strong);
      border-radius: var(--radius-lg);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      background-color: var(--bg-tertiary);
      cursor: context-menu;
    }
    .color-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: var(--space-4);
    }
    .color-card {
      padding: var(--space-6) var(--space-4);
      border-radius: var(--radius-lg);
      color: #ffffff;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    .brand-bg { background-color: var(--brand-primary); }
    .success-bg { background-color: var(--success); }
    .warning-bg { background-color: var(--warning); }
    .danger-bg { background-color: var(--danger); }
    .info-bg { background-color: var(--info); }
    .color-name { font-weight: 600; font-size: var(--text-body-sm); }
    .color-code { font-size: var(--text-caption); opacity: 0.85; font-family: var(--font-mono); }

    .typo-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .typo-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: var(--space-3);
    }
    .typo-label { font-size: var(--text-caption); color: var(--text-muted); width: 120px; }
    .typo-display { font-size: var(--text-display); font-weight: 700; }
    .typo-h1 { font-size: var(--text-h1); font-weight: 700; }
    .typo-h2 { font-size: var(--text-h2); font-weight: 600; }
    .typo-h3 { font-size: var(--text-h3); font-weight: 600; }
    .typo-body { font-size: var(--text-body-md); }
    .typo-caption { font-size: var(--text-caption); color: var(--text-muted); }

    .component-row {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      flex-wrap: wrap;
    }
    .grid-2 {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-6);
      @media (max-width: 768px) { grid-template-columns: 1fr; }
    }
    .grid-3 {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--space-6);
      @media (max-width: 960px) { grid-template-columns: 1fr; }
    }
    .form-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DesignSystemComponent {
  protected readonly themeService = inject(ThemeService);
  private readonly notificationService = inject(NotificationService);

  protected activeCategory = signal<string>('tokens');
  protected isDialogOpen = signal<boolean>(false);
  protected isDrawerOpen = signal<boolean>(false);
  protected isCommandBarOpen = signal<boolean>(false);
  protected isToggleActive = signal<boolean>(true);

  protected readonly breadcrumbItems = [
    { label: 'Home', url: '/' },
    { label: 'Design System' },
  ];

  protected readonly categoryTabs: TabItem[] = [
    { id: 'tokens', label: 'Design Tokens', icon: 'sparkles' },
    { id: 'buttons', label: 'Buttons & Actions', icon: 'grid' },
    { id: 'forms', label: 'Form Controls', icon: 'edit' },
    { id: 'display', label: 'Surfaces & Display', icon: 'user' },
    { id: 'data', label: 'Data Presentation', icon: 'list' },
    { id: 'overlays', label: 'Overlays & Feedback', icon: 'bell' },
  ];

  protected readonly selectOptions = [
    { label: 'Full Time Engineering', value: 'ft' },
    { label: 'Contract Architecture', value: 'contract' },
    { label: 'Consulting Advisory', value: 'consulting' },
  ];

  protected readonly radioOptions = [
    { label: 'Real-time Instant', value: 'instant' },
    { label: 'Daily Summary Digest', value: 'daily' },
    { label: 'Weekly Overview', value: 'weekly' },
  ];

  protected readonly dropdownItems: DropdownItem[] = [
    { id: 'edit', label: 'Edit Profile', icon: 'edit' },
    { id: 'share', label: 'Share Link', icon: 'share' },
    { id: 'delete', label: 'Delete Item', icon: 'trash', danger: true },
  ];

  protected readonly contextMenuItems: ContextMenuItem[] = [
    { id: 'copy', label: 'Copy Symbol', icon: 'copy' },
    { id: 'edit', label: 'Inspect Component', icon: 'search' },
    { id: 'delete', label: 'Remove Node', icon: 'trash', danger: true },
  ];

  protected readonly accordionItems: AccordionItem[] = [
    { id: 'a1', title: 'What is Microsoft Fluent 2 inspiration?', subtitle: 'Design Philosophy', content: 'Fluent 2 delivers a clean, modern SaaS visual language focusing on soft shadows, subtle borders, high contrast accessibility, and responsive layouts.' },
    { id: 'a2', title: 'How does Theme Engine work?', subtitle: 'CSS Custom Variables', content: 'Theme states (light, dark, system, high-contrast) set the data-theme attribute on document root, driving CSS Custom Properties.' },
  ];

  protected readonly tableColumns: ColumnDef[] = [
    { key: 'name', header: 'Component Name', sortable: true },
    { key: 'category', header: 'Category', sortable: true },
    { key: 'status', header: 'Production Status', sortable: true, type: 'badge' },
    { key: 'version', header: 'Version' },
  ];

  protected readonly tableData = [
    { id: 1, name: 'ButtonComponent', category: 'Actions', status: 'Hired & Active', version: 'v1.0' },
    { id: 2, name: 'DataTableComponent', category: 'Presentation', status: 'Hired & Active', version: 'v1.0' },
    { id: 3, name: 'DialogComponent', category: 'Overlays', status: 'Pending Review', version: 'v1.0' },
    { id: 4, name: 'CommandBarComponent', category: 'Navigation', status: 'Hired & Active', version: 'v1.0' },
  ];

  protected triggerToast(): void {
    this.notificationService.success('Theme Token Applied', 'Microsoft Fluent 2 design tokens active.');
  }
}

export default DesignSystemComponent;
