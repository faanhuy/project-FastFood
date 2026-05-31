import api from './api';
import type { ApiResponse } from '../types/auth';

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Customer' | 'Admin';
  isBanned: boolean;
  bannedAt?: string;
  createdAt: string;
  orderCount: number;
}

export interface UsersPagedResult {
  items: UserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UpdateRoleRequest {
  newRole: string;
}

export const adminUserService = {
  getUsers: async (
    page = 1,
    pageSize = 20,
    roleFilter?: string,
    bannedFilter?: boolean,
    searchEmail?: string
  ): Promise<UsersPagedResult> => {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
    };
    if (roleFilter) params.roleFilter = roleFilter;
    if (bannedFilter !== undefined) params.bannedFilter = bannedFilter.toString();
    if (searchEmail) params.searchEmail = searchEmail;

    const { data } = await api.get<ApiResponse<UsersPagedResult>>('/admin/users', { params });
    return data.data;
  },

  getUserDetail: async (userId: string): Promise<UserDto> => {
    const { data } = await api.get<ApiResponse<UserDto>>(`/admin/users/${userId}`);
    return data.data;
  },

  banUser: async (userId: string): Promise<UserDto> => {
    const { data } = await api.patch<ApiResponse<UserDto>>(`/admin/users/${userId}/ban`);
    return data.data;
  },

  unbanUser: async (userId: string): Promise<UserDto> => {
    const { data } = await api.patch<ApiResponse<UserDto>>(`/admin/users/${userId}/unban`);
    return data.data;
  },

  updateUserRole: async (userId: string, newRole: string): Promise<UserDto> => {
    const { data } = await api.patch<ApiResponse<UserDto>>(`/admin/users/${userId}/role`, {
      newRole,
    } satisfies UpdateRoleRequest);
    return data.data;
  },
};
