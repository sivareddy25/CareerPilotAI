/**
 * Wire contracts for the profile endpoints.
 *
 * The enum values are numeric to match the server, which persists them as integers.
 * They are mirrored here rather than sent as strings so that renaming a member on
 * either side becomes a compile error instead of a value that silently stops matching.
 */

export enum ThemePreference {
  System = 0,
  Light = 1,
  Dark = 2,
  HighContrast = 3,
}

export enum DateFormatPreference {
  IsoYearMonthDay = 0,
  DayMonthYear = 1,
  MonthDayYear = 2,
  DayMonthNameYear = 3,
}

export enum TimeFormatPreference {
  TwentyFourHour = 0,
  TwelveHour = 1,
}

export interface Preferences {
  readonly theme: ThemePreference;
  readonly dateFormat: DateFormatPreference;
  readonly timeFormat: TimeFormatPreference;
  readonly emailNotifications: boolean;
  readonly inAppNotifications: boolean;
  readonly marketingEmails: boolean;
  readonly weeklySummaryEmails: boolean;
}

export interface Profile {
  readonly userId: string;
  /** Read-only. Changing it requires a verification workflow the server does not expose. */
  readonly email: string;
  readonly emailConfirmed: boolean;
  readonly firstName: string | null;
  readonly lastName: string | null;
  readonly displayName: string | null;
  readonly phoneNumber: string | null;
  readonly country: string | null;
  readonly state: string | null;
  readonly city: string | null;
  readonly timeZone: string | null;
  readonly preferredLanguage: string | null;
  readonly profilePictureUrl: string | null;
  readonly bio: string | null;
  readonly linkedInUrl: string | null;
  readonly gitHubUrl: string | null;
  readonly portfolioUrl: string | null;
  readonly preferences: Preferences;
}

/** Body of `PUT /profile`. Email is absent by design — the server refuses to change it. */
export interface UpdateProfileRequest {
  readonly firstName: string | null;
  readonly lastName: string | null;
  readonly displayName: string | null;
  readonly phoneNumber: string | null;
  readonly country: string | null;
  readonly state: string | null;
  readonly city: string | null;
  readonly timeZone: string | null;
  readonly preferredLanguage: string | null;
  readonly bio: string | null;
  readonly linkedInUrl: string | null;
  readonly gitHubUrl: string | null;
  readonly portfolioUrl: string | null;
}

export interface ChangePasswordRequest {
  readonly currentPassword: string;
  readonly newPassword: string;
}

export interface DeactivateAccountRequest {
  readonly currentPassword: string;
}

export interface DeleteAccountRequest {
  readonly currentPassword: string;
  /** Must be the literal string `DELETE`; the server compares it case-sensitively. */
  readonly confirmation: string;
}

export interface ProfileImage {
  readonly profilePictureUrl: string;
}
