import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SystemService } from '../../core/services/system.service';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  IconComponent,
  PageHeaderComponent,
  SpinnerComponent,
} from '../../shared/components';

@Component({
  selector: 'app-local-system-settings',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent,
    PageHeaderComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="system-container">
      <app-page-header
        title="Local System & Desktop Management"
        subtitle="Health probes, automatic migrations, 1-click database backup/restore, local Ollama models, and GitHub release checker."
      >
        <div header-actions class="header-actions">
          <app-button variant="outline" (btnClick)="onExportBackup()">
            <app-icon name="download" size="sm" /> Download Backup JSON
          </app-button>
          <app-button variant="primary" (btnClick)="onCheckHealth()">
            <app-icon name="refresh-cw" size="sm" /> Run Health Probes
          </app-button>
        </div>
      </app-page-header>

      @if (isLoading()) {
        <div class="loading-state">
          <app-spinner size="lg" message="Running local system health probes & model diagnostics..." />
        </div>
      } @else if (health(); as h) {
        <!-- System Health Cards -->
        <h3 class="section-title">Component Health Probes</h3>
        <div class="health-grid">
          <app-card class="probe-card">
            <div class="probe-header">
              <span class="probe-title">{{ h.postgresDb.componentName }}</span>
              <app-badge [variant]="h.postgresDb.isHealthy ? 'success' : 'danger'">
                {{ h.postgresDb.isHealthy ? 'Healthy' : 'Error' }}
              </app-badge>
            </div>
            <p class="probe-status">{{ h.postgresDb.statusText }}</p>
            <span class="probe-details">{{ h.postgresDb.details }}</span>
          </app-card>

          <app-card class="probe-card">
            <div class="probe-header">
              <span class="probe-title">{{ h.redisCache.componentName }}</span>
              <app-badge [variant]="h.redisCache.isHealthy ? 'success' : 'danger'">
                {{ h.redisCache.isHealthy ? 'Healthy' : 'Error' }}
              </app-badge>
            </div>
            <p class="probe-status">{{ h.redisCache.statusText }}</p>
            <span class="probe-details">{{ h.redisCache.details }}</span>
          </app-card>

          <app-card class="probe-card">
            <div class="probe-header">
              <span class="probe-title">{{ h.ollamaLlmEngine.componentName }}</span>
              <app-badge [variant]="h.ollamaLlmEngine.isHealthy ? 'success' : 'danger'">
                {{ h.ollamaLlmEngine.isHealthy ? 'Healthy' : 'Error' }}
              </app-badge>
            </div>
            <p class="probe-status">{{ h.ollamaLlmEngine.statusText }}</p>
            <span class="probe-details">{{ h.ollamaLlmEngine.details }}</span>
          </app-card>

          <app-card class="probe-card">
            <div class="probe-header">
              <span class="probe-title">{{ h.playwrightBrowserDriver.componentName }}</span>
              <app-badge [variant]="h.playwrightBrowserDriver.isHealthy ? 'success' : 'danger'">
                {{ h.playwrightBrowserDriver.isHealthy ? 'Healthy' : 'Error' }}
              </app-badge>
            </div>
            <p class="probe-status">{{ h.playwrightBrowserDriver.statusText }}</p>
            <span class="probe-details">{{ h.playwrightBrowserDriver.details }}</span>
          </app-card>
        </div>

        <!-- 2 Column Body: Ollama Models & Updates -->
        <div class="management-grid">
          <app-card title="Local Ollama LLM Models Manager">
            <div class="models-table">
              @for (m of ollamaModels(); track m.name) {
                <div class="model-row">
                  <div>
                    <strong class="model-name">{{ m.name }}</strong>
                    <span class="model-family">({{ m.modelFamily }})</span>
                  </div>
                  <app-badge variant="secondary">{{ m.formattedSize }}</app-badge>
                </div>
              }
            </div>
          </app-card>

          <app-card title="Software Updates & Versioning">
            @if (updateStatus(); as u) {
              <div class="update-box">
                <div class="version-row">
                  <span>Current Version: <strong>{{ u.currentVersion }}</strong></span>
                  <app-badge [variant]="u.isUpdateAvailable ? 'warning' : 'success'">
                    {{ u.isUpdateAvailable ? 'Update Available' : 'Up To Date' }}
                  </app-badge>
                </div>
                <p class="release-notes">{{ u.releaseNotes }}</p>
                <app-button variant="outline" [fullWidth]="true" (btnClick)="onCheckUpdates()">
                  <app-icon name="refresh-cw" size="sm" /> Check GitHub Releases
                </app-button>
              </div>
            }
          </app-card>
        </div>

        <!-- Diagnostic Logs Stream -->
        <app-card title="Diagnostic Logs & Telemetry Stream" class="logs-card">
          <div class="logs-stream">
            @for (log of logs(); track log.id) {
              <div class="log-row">
                <span class="log-time">{{ log.timestamp | date:'mediumTime' }}</span>
                <app-badge variant="info" styleMode="soft">{{ log.sourceCategory }}</app-badge>
                <span class="log-msg">{{ log.message }}</span>
              </div>
            }
          </div>
        </app-card>
      }
    </div>
  `,
  styles: [`
    .system-container {
      padding: var(--space-6);
      max-width: 1400px;
      margin: 0 auto;
    }
    .header-actions {
      display: flex;
      gap: var(--space-3);
    }
    .loading-state { display: flex; justify-content: center; padding: var(--space-12) 0; }
    .section-title {
      font-size: var(--text-h3);
      font-weight: 700;
      margin: var(--space-6) 0 var(--space-3) 0;
    }
    .health-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: var(--space-4);
    }
    .probe-card { padding: var(--space-4); }
    .probe-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-2);
    }
    .probe-title { font-weight: 600; font-size: var(--text-body-sm); }
    .probe-status { font-size: var(--text-body-sm); margin: 0 0 var(--space-1) 0; color: var(--text-primary); }
    .probe-details { font-size: var(--text-caption); color: var(--text-muted); }

    .management-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-6);
      margin: var(--space-6) 0;
    }
    @media (max-width: 992px) {
      .management-grid { grid-template-columns: 1fr; }
    }
    .models-table { display: flex; flex-direction: column; gap: var(--space-3); }
    .model-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-md);
      background-color: var(--bg-tertiary);
    }
    .model-name { font-size: var(--text-body-sm); margin-right: var(--space-2); }
    .model-family { font-size: var(--text-caption); color: var(--text-secondary); }

    .version-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-3); }
    .release-notes { font-size: var(--text-body-sm); color: var(--text-secondary); margin-bottom: var(--space-4); }

    .logs-card { margin-top: var(--space-6); }
    .logs-stream { display: flex; flex-direction: column; gap: var(--space-2); font-family: monospace; font-size: 12px; }
    .log-row {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2);
      border-bottom: 1px solid var(--border-subtle);
    }
    .log-time { color: var(--text-muted); }
    .log-msg { color: var(--text-primary); flex: 1; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LocalSystemSettingsComponent implements OnInit {
  private readonly systemService = inject(SystemService);

  protected readonly health = this.systemService.healthStatus;
  protected readonly ollamaModels = this.systemService.ollamaModels;
  protected readonly updateStatus = this.systemService.updateStatus;
  protected readonly logs = this.systemService.logs;
  protected readonly isLoading = this.systemService.isLoading;

  ngOnInit(): void {
    this.onCheckHealth();
    this.systemService.loadOllamaModels().subscribe();
    this.systemService.checkUpdates().subscribe();
    this.systemService.loadLogs().subscribe();
  }

  protected onCheckHealth(): void {
    this.systemService.checkHealth().subscribe();
  }

  protected onExportBackup(): void {
    this.systemService.exportBackup().subscribe((backup) => {
      const blob = new Blob([backup.backupJsonData], { type: 'application/json' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `careerpilot-backup-${new Date().toISOString().slice(0, 10)}.json`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  protected onCheckUpdates(): void {
    this.systemService.checkUpdates().subscribe();
  }
}
