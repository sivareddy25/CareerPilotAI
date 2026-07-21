import { ApplicationConfig } from '@angular/core';
import { provideCoreInfrastructure } from './core/providers/app-providers';

export const appConfig: ApplicationConfig = {
  providers: [provideCoreInfrastructure()],
};
