import { Environment } from './environment.model';

export const environment: Environment = {
  production: false,
  environmentName: 'development',
  apiBaseUrl: 'http://localhost:5080/api/v1',
  logging: {
    enabled: true,
    level: 'debug',
  },
  featureFlags: {
    enableAnalytics: false,
    enableNewDashboard: true,
  },
};
