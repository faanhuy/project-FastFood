export interface AuthResponse {
  token: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}
