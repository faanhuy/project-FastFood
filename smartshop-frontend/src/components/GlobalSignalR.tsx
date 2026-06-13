import { useEffect } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/store/authStore';
import { useNotificationStore } from '@/store/notificationStore';
import { useSignalR } from '@/hooks/useSignalR';
import type { SignalRNotification } from '@/hooks/useSignalR';
import { localizeNotification } from '@/utils/localizeNotification';

export default function GlobalSignalR() {
  const { isAuthenticated } = useAuthStore();
  const { fetchNotifications } = useNotificationStore();
  const { t } = useTranslation('common');
  const { t: tOrder } = useTranslation('order');

  useEffect(() => {
    if (isAuthenticated) fetchNotifications();
  }, [isAuthenticated, fetchNotifications]);

  const handleMessage = (data: SignalRNotification) => {
    const { message } = localizeNotification(t, tOrder, data);
    if (message) toast.success(message, { duration: 5000 });
    fetchNotifications();
  };

  useSignalR(isAuthenticated ? handleMessage : () => {});

  return null;
}
