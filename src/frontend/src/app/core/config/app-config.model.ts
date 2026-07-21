export interface AppConfig {
  appName: string;
  version: string;
  apiBaseUrl: string;
  environmentName: string;
  defaultTheme: 'light' | 'dark' | 'system';
  featureFlags: Record<string, boolean>;
}
