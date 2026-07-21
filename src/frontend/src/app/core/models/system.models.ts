export interface ComponentHealthDto {
  componentName: string;
  isHealthy: boolean;
  statusText: string;
  details: string;
  checkedAt: string;
}

export interface SystemHealthStatusDto {
  isOverallHealthy: boolean;
  postgresDb: ComponentHealthDto;
  redisCache: ComponentHealthDto;
  ollamaLlmEngine: ComponentHealthDto;
  playwrightBrowserDriver: ComponentHealthDto;
  oAuthIntegrations: ComponentHealthDto;
  checkedAt: string;
}

export interface OllamaModelDto {
  name: string;
  modelFamily: string;
  sizeBytes: number;
  formattedSize: string;
  digest: string;
  modifiedAt: string;
}

export interface SystemBackupDto {
  backupId: string;
  exportedAt: string;
  schemaVersion: string;
  totalResumes: number;
  totalJobs: number;
  totalApplications: number;
  backupJsonData: string;
}

export interface UpdateStatusDto {
  currentVersion: string;
  latestVersion: string;
  isUpdateAvailable: boolean;
  releaseNotes: string;
  downloadUrl: string;
  checkedAt: string;
}

export interface LogEntryDto {
  id: string;
  timestamp: string;
  logLevel: string;
  message: string;
  sourceCategory: string;
  exceptionDetails?: string;
}
