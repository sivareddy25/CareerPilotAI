import { Injectable } from '@angular/core';
import { STORAGE_KEYS } from '../constants/storage.constants';
import { StorageUtils } from '../../shared/utils/storage.utils';

/**
 * Holds the token pair, splitting the two by how long each needs to survive.
 *
 * The access token is kept **in memory only**. It never reaches `localStorage`, so it
 * is not readable by an XSS payload that runs after the fact, it is not left behind on
 * a shared machine, and it disappears when the tab closes.
 *
 * The refresh token has to outlive a page reload — that is what makes session
 * restoration possible at all — so it is persisted. This is the residual risk of any
 * browser SPA: script running on the origin can read it. It is mitigated rather than
 * eliminated: rotation means a stolen token is single-use and its theft is detectable
 * (the API revokes the whole chain on reuse), and the backend can be switched to
 * HttpOnly cookie delivery via `Authentication:RefreshTokenCookie` for deployments
 * where XSS resistance matters more than statelessness.
 */
@Injectable({
  providedIn: 'root',
})
export class TokenStorageService {
  private accessToken: string | null = null;
  private accessTokenExpiresAt: number | null = null;

  getAccessToken(): string | null {
    return this.accessToken;
  }

  /** Epoch milliseconds, or null when no access token is held. */
  getAccessTokenExpiry(): number | null {
    return this.accessTokenExpiresAt;
  }

  setAccessToken(token: string, expiresAtIso: string): void {
    this.accessToken = token;
    this.accessTokenExpiresAt = Date.parse(expiresAtIso);
  }

  getRefreshToken(): string | null {
    return StorageUtils.getItem<string>(STORAGE_KEYS.REFRESH_TOKEN);
  }

  setRefreshToken(token: string): void {
    StorageUtils.setItem(STORAGE_KEYS.REFRESH_TOKEN, token);
  }

  /**
   * True when the access token is missing or within `skewSeconds` of expiry.
   *
   * The skew exists so a request is not sent with a token that will lapse while it is
   * in flight — the server enforces expiry with zero clock tolerance, so a token valid
   * at send time can still be rejected on arrival.
   */
  isAccessTokenExpired(skewSeconds = 30): boolean {
    if (!this.accessToken || this.accessTokenExpiresAt === null) {
      return true;
    }

    return Date.now() >= this.accessTokenExpiresAt - skewSeconds * 1000;
  }

  hasRefreshToken(): boolean {
    return !!this.getRefreshToken();
  }

  clear(): void {
    this.accessToken = null;
    this.accessTokenExpiresAt = null;

    StorageUtils.removeItem(STORAGE_KEYS.REFRESH_TOKEN);

    // Removed too: an earlier build persisted the access token under this key, and a
    // stale entry left behind would be a credential sitting in storage indefinitely.
    StorageUtils.removeItem(STORAGE_KEYS.AUTH_TOKEN);
  }
}
