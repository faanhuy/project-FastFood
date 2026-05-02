import api from '@/services/api';
import type { Notification } from '@/types/notification';
import type { ApiResponse } from '@/types/auth';

export const notificationService = {
  getNotifications: async (): Promise<Notification[]> => {
    const { data } = await api.get<ApiResponse<Notification[]>>('/notifications');
    return data.data ?? [];
  },

  markAsRead: async (id: string): Promise<void> => {
    await api.patch(`/notifications/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await api.patch('/notifications/read-all');
  },

  deleteOne: async (id: string): Promise<void> => {
    await api.delete(`/notifications/${id}`);
  },

  deleteAll: async (): Promise<void> => {
    await api.delete('/notifications');
  },
};
