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
  totalSpent: number;
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

export interface BulkActionResult {
  succeeded: number;
  failed: number;
  errors: { itemId: string; message: string }[];
}

export const adminUserService = {
  getUsers: async (
    page = 1,
    pageSize = 20,
    roleFilter?: string,
    bannedFilter?: boolean,
    searchEmail?: string,
    sortBy = 'createdAt',
    sortDirection = 'desc'
  ): Promise<UsersPagedResult> => {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortBy,
      sortDirection,
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

  bulkUpdateUsers: async (userIds: string[], action: 'ban' | 'unban' | 'setRole', roleValue?: string): Promise<BulkActionResult> => {
    const { data } = await api.post<ApiResponse<BulkActionResult>>('/admin/users/bulk-actions', { userIds, action, roleValue });
    return data.data;
  },

  forceLogout: async (userId: string): Promise<{ message: string; revokedTokenCount: number }> => {
    const { data } = await api.post<ApiResponse<{ message: string; revokedTokenCount: number }>>(`/admin/users/${userId}/force-logout`);
    return data.data;
  },

  resetPassword: async (userId: string): Promise<{ message: string }> => {
    const { data } = await api.post<ApiResponse<{ message: string }>>(`/admin/users/${userId}/reset-password`);
    return data.data;
  },
};
