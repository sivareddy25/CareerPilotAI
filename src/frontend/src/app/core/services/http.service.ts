import { Injectable, inject } from '@angular/core';
import { ApiService, HttpOptions } from './api.service';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root',
})
export class HttpService {
  private readonly apiService = inject(ApiService);

  get<T>(endpoint: string, options?: HttpOptions): Observable<ApiResponse<T>> {
    return this.apiService.get<ApiResponse<T>>(endpoint, options);
  }

  post<T, U = unknown>(endpoint: string, body: U, options?: HttpOptions): Observable<ApiResponse<T>> {
    return this.apiService.post<ApiResponse<T>, U>(endpoint, body, options);
  }

  put<T, U = unknown>(endpoint: string, body: U, options?: HttpOptions): Observable<ApiResponse<T>> {
    return this.apiService.put<ApiResponse<T>, U>(endpoint, body, options);
  }

  delete<T>(endpoint: string, options?: HttpOptions): Observable<ApiResponse<T>> {
    return this.apiService.delete<ApiResponse<T>>(endpoint, options);
  }
}
