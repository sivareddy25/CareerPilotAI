import { Environment } from './environment.model';

export const environment: Environment = {
  production: true,
  environmentName: 'production',
  apiBaseUrl: '/api/v1',
  logging: {
    enabled: false,
    level: 'error',
  },
  featureFlags: {
    enableAnalytics: true,
    enableNewDashboard: true,
  },
};
