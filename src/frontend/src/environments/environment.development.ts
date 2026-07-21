import { Environment } from './environment';

export const environment: Environment = {
  production: false,
  environmentName: 'development',
  apiBaseUrl: 'http://localhost:5000/api/v1',
  logging: {
    enabled: true,
    level: 'debug',
  },
  featureFlags: {
    enableAnalytics: false,
    enableNewDashboard: true,
  },
};
