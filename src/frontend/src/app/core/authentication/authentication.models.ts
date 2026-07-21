/**
 * Wire contracts for the authentication endpoints. These mirror the API's response
 * shapes exactly — the auth endpoints return their payload directly rather than
 * wrapped in `ApiResponse<T>`.
 */

export interface UserProfile {
  readonly id: string;
  readonly email: string;
  readonly firstName: string | null;
  readonly lastName: string | null;
  readonly emailConfirmed: boolean;
  readonly roles: readonly string[];
  readonly permissions: readonly string[];
}

export interface AuthenticationResult {
  readonly accessToken: string;
  readonly refreshToken: string;
  /** ISO 8601, UTC. */
  readonly accessTokenExpiresAt: string;
  readonly refreshTokenExpiresAt: string;
  readonly tokenType: string;
  readonly user: UserProfile;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

export interface RegisterRequest {
  readonly email: string;
  readonly password: string;
  readonly firstName: string | null;
  readonly lastName: string | null;
}

export interface RefreshTokenRequest {
  readonly refreshToken: string | null;
}
