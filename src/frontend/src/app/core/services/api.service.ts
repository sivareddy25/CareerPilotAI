import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from '../config/app-config.service';

export interface HttpOptions {
  headers?: HttpHeaders | { [header: string]: string | string[] };
  params?: HttpParams | { [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean> };
  reportProgress?: boolean;
  withCredentials?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  protected readonly http = inject(HttpClient);
  protected readonly configService = inject(AppConfigService);

  protected get baseUrl(): string {
    return this.configService.apiBaseUrl;
  }

  get<T>(endpoint: string, options?: HttpOptions): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, options);
  }

  post<T, U = unknown>(endpoint: string, body: U, options?: HttpOptions): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  put<T, U = unknown>(endpoint: string, body: U, options?: HttpOptions): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  patch<T, U = unknown>(endpoint: string, body: U, options?: HttpOptions): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}${endpoint}`, body, options);
  }

  delete<T>(endpoint: string, options?: HttpOptions): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`, options);
  }

  /**
   * Escape hatch for verbs the shorthands cannot express.
   *
   * Needed for DELETE with a request body — account deletion carries a password, which
   * must not go in a query string where it would be captured by server logs, browser
   * history and referrer headers. `HttpClient.delete` accepts a body only through the
   * generic request form.
   */
  request<T>(
    method: string,
    endpoint: string,
    options?: HttpOptions & {
      body?: unknown;
      /** `blob` for file downloads; `json` otherwise. */
      responseType?: 'json' | 'blob' | 'text' | 'arraybuffer';
      /** `response` when the caller needs headers, such as Content-Disposition. */
      observe?: 'body' | 'response' | 'events';
    },
  ): Observable<T> {
    // HttpClient's overloads narrow the return type from the literal values of
    // responseType and observe, which cannot be expressed through this wrapper's
    // generic. The cast is confined to this one line; callers stay fully typed via T.
    return this.http.request(
      method,
      `${this.baseUrl}${endpoint}`,
      options as object,
    ) as unknown as Observable<T>;
  }
}
