export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string | null;
  statusCode: number;
  timestamp: string;
}
