import { Environment } from './environment.model';

// Default / production environment. A same-origin relative base — the deployed API is served
// from the same host as the app, and the api-base-url interceptor leaves same-origin paths
// untouched. Local development replaces this file with environment.development.ts (see the
// fileReplacements in angular.json), which points at the standalone backend on :5080.
// The previous value (http://localhost:5000) was a dead port — macOS AirPlay listens there — so
// every API call failed and the UI silently showed mock data.
export const environment: Environment = {
  production: true,
  environmentName: 'production',
  apiBaseUrl: '/api/v1',
  logging: {
    enabled: false,
    level: 'error',
  },
  featureFlags: {
    enableAnalytics: false,
    enableNewDashboard: true,
  },
};
