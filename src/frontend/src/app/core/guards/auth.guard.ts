import { CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  // Placeholder guard - allow navigation without authentication for now
  return true;
};
