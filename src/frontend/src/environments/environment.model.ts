/**
 * Shape of the build-time environment config.
 *
 * Kept in its own file, separate from the concrete `environment.ts`, because angular.json's
 * fileReplacements swaps `environment.ts` for `environment.development.ts` at build time. If the
 * interface lived in `environment.ts`, the development file's import of it would resolve to its
 * own replacement and the type would vanish. A dedicated model file is imported by both and is
 * never replaced.
 */
export interface Environment {
  production: boolean;
  environmentName: string;
  apiBaseUrl: string;
  logging: {
    enabled: boolean;
    level: 'debug' | 'info' | 'warn' | 'error';
  };
  featureFlags: {
    enableAnalytics: boolean;
    enableNewDashboard: boolean;
  };
}
