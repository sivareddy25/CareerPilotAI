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
