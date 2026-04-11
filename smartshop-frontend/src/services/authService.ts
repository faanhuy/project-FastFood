import api from './api';
import type { ApiResponse, AuthResponse, LoginRequest, RegisterRequest } from '../types/auth';

export const authService = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const { data: res } = await api.post<ApiResponse<AuthResponse>>('/auth/login', data);
    return res.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const { data: res } = await api.post<ApiResponse<AuthResponse>>('/auth/register', data);
    return res.data;
  },
};
