import { useEffect } from 'react';
import toast from 'react-hot-toast';
import { useAuthStore } from '@/store/authStore';
import { useNotificationStore } from '@/store/notificationStore';
import { useSignalR } from '@/hooks/useSignalR';
import type { SignalRNotification } from '@/hooks/useSignalR';

export default function GlobalSignalR() {
  const { isAuthenticated } = useAuthStore();
  const { fetchNotifications } = useNotificationStore();

  useEffect(() => {
    if (isAuthenticated) fetchNotifications();
  }, [isAuthenticated, fetchNotifications]);

  const handleMessage = (data: SignalRNotification) => {
    toast.success(data.message, { duration: 5000 });
    fetchNotifications();
  };

  useSignalR(isAuthenticated ? handleMessage : () => {});

  return null;
}
