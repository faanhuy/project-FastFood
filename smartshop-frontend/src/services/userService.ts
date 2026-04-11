import api from './api';
import type { ApiResponse } from '../types/auth';

export interface UserProfileDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
}

export const userService = {
  getMyProfile: async (): Promise<UserProfileDto> => {
    const { data } = await api.get<ApiResponse<UserProfileDto>>('/users/me');
    return data.data;
  },

  updateMyProfile: async (payload: UpdateProfileRequest): Promise<UserProfileDto> => {
    const { data } = await api.put<ApiResponse<UserProfileDto>>('/users/me', payload);
    return data.data;
  },
};
