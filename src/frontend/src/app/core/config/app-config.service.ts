import { Injectable, inject } from '@angular/core';
import { APP_CONFIG } from '../tokens/app-config.token';
import { AppConfig } from './app-config.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private readonly config: AppConfig = inject(APP_CONFIG, {
    optional: true,
  }) ?? {
    appName: 'CareerPilot AI',
    version: '1.0.0',
    apiBaseUrl: environment.apiBaseUrl,
    environmentName: environment.environmentName,
    defaultTheme: 'system',
    featureFlags: environment.featureFlags,
  };

  get appName(): string {
    return this.config.appName;
  }

  get version(): string {
    return this.config.version;
  }

  get apiBaseUrl(): string {
    return this.config.apiBaseUrl;
  }

  get environmentName(): string {
    return this.config.environmentName;
  }

  isFeatureEnabled(flag: string): boolean {
    return !!this.config.featureFlags[flag];
  }
}
