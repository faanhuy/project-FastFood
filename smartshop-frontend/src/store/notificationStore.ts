import { create } from 'zustand';
import type { Notification } from '@/types/notification';
import { notificationService } from '@/services/notificationService';

interface NotificationState {
  notifications: Notification[];
  fetchNotifications: () => Promise<void>;
  addNotification: (n: Notification) => void;
  markOneRead: (id: string) => void;
  markAllRead: () => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],

  fetchNotifications: async () => {
    try {
      const data = await notificationService.getNotifications();
      set({ notifications: data });
    } catch {
      // silently ignore
    }
  },

  addNotification: (n) =>
    set((state) => ({ notifications: [n, ...state.notifications] })),

  markOneRead: (id) =>
    set((state) => ({
      notifications: state.notifications.map((n) =>
        n.id === id ? { ...n, isRead: true } : n
      ),
    })),

  markAllRead: () =>
    set((state) => ({
      notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
    })),
}));
