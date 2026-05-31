import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { notificationService } from '@/services/notificationService';
import type { Notification } from '@/types/notification';
import { useNotificationStore } from '@/store/notificationStore';

const STATUS_MAP_EN_VI: Record<string, string> = {
  Pending: 'Pending',
  Confirmed: 'Confirmed',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
  Processing: 'Processing',
  Refunded: 'Refunded'
};

function localizeMessage(message: string): string {
  return message;
}

function formatRelativeTime(isoString: string, t: (key: string) => string): string {
  const normalized = isoString.endsWith('Z') ? isoString : isoString + 'Z';
  const diff = Date.now() - new Date(normalized).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes} minutes ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hours ago`;
  return `${Math.floor(hours / 24)} days ago`;
}

export default function NotificationBell() {
  const { t } = useTranslation('common');
  const { t: tToast } = useTranslation('toast');
  const navigate = useNavigate();
  const { notifications, markOneRead, markAllRead, removeOne, removeAll } = useNotificationStore();
  const [open, setOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node))
        setOpen(false);
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const handleNotificationClick = async (notification: Notification) => {
    if (!notification.isRead) {
      try {
        await notificationService.markAsRead(notification.id);
        markOneRead(notification.id);
      } catch {
        toast.error(tToast('genericError'));
      }
    }
    setOpen(false);
    if (notification.orderId) navigate(`/orders/${notification.orderId}`);
  };

  const handleMarkAllRead = async () => {
    try {
      await notificationService.markAllAsRead();
      markAllRead();
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? tToast('genericError'));
    }
  };

  const handleDeleteOne = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    try {
      await notificationService.deleteOne(id);
      removeOne(id);
    } catch {
      toast.error(tToast('genericError'));
    }
  };

  const handleMarkOneRead = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    try {
      await notificationService.markAsRead(id);
      markOneRead(id);
    } catch {
      toast.error(tToast('genericError'));
    }
  };

  const handleDeleteAll = async () => {
    try {
      await notificationService.deleteAll();
      removeAll();
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? tToast('genericError'));
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setOpen((o) => !o)}
        className="relative text-gray-500 hover:text-rose-600 p-1"
        title={t('notifications')}
      >
        <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute -top-1.5 -right-1.5 bg-red-500 text-white text-[10px] font-bold min-w-4 h-4 px-0.5 rounded-full flex items-center justify-center leading-none">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-xl shadow-lg border border-gray-100 z-[999] flex flex-col max-h-[420px]">
          <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
            <span className="font-semibold text-sm text-gray-800">{t('notifications')}</span>
            {unreadCount > 0 && (
              <span className="text-xs text-rose-600 font-medium">{unreadCount} {t('unread')}</span>
            )}
          </div>

          <div className="overflow-y-auto flex-1">
            {notifications.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-gray-400">
                {t('noNotifications')}
              </div>
            ) : (
              notifications.map((n) => (
                <div
                  key={n.id}
                  className={`group relative border-b border-gray-50 transition-colors ${
                    !n.isRead ? 'bg-rose-50 hover:bg-rose-100/60' : 'hover:bg-gray-50'
                  }`}
                >
                  <button
                    onClick={() => handleNotificationClick(n)}
                    className="w-full text-left px-4 py-3 pr-20"
                  >
                    <div className="flex items-start gap-2">
                      {!n.isRead && (
                        <span className="mt-1.5 w-2 h-2 rounded-full bg-rose-500 shrink-0" />
                      )}
                      <div className={!n.isRead ? '' : 'pl-4'}>
                        <p className="text-sm font-medium text-gray-800 line-clamp-1">{n.title}</p>
                        <p className="text-xs text-gray-500 line-clamp-2 mt-0.5">
                          {localizeMessage(n.message)}
                        </p>
                        <p className="text-[11px] text-gray-400 mt-1">{formatRelativeTime(n.createdAt, t)}</p>
                      </div>
                    </div>
                  </button>

                  {/* Action buttons — always visible for unread, hover-only for delete */}
                  <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-0.5">
                    {!n.isRead && (
                      <button
                        onClick={(e) => handleMarkOneRead(e, n.id)}
                        title={t('confirm', { ns: 'common' })}
                        className="p-1.5 rounded-md text-green-500 hover:text-green-700 hover:bg-green-50 transition-colors"
                      >
                        <svg className="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5}>
                          <polyline points="20 6 9 17 4 12" />
                        </svg>
                      </button>
                    )}
                    <button
                      onClick={(e) => handleDeleteOne(e, n.id)}
                      title={t('deleteNotification')}
                      className="p-1.5 rounded-md text-gray-300 hover:text-red-600 hover:bg-red-50 transition-colors opacity-0 group-hover:opacity-100"
                    >
                      <svg className="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                        <polyline points="3 6 5 6 21 6" />
                        <path d="M19 6l-1 14H6L5 6" />
                        <path d="M10 11v6M14 11v6" />
                        <path d="M9 6V4h6v2" />
                      </svg>
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          {notifications.length > 0 && (
            <div className="px-4 py-2.5 border-t border-gray-100 flex items-center gap-2">
              <button
                onClick={handleMarkAllRead}
                className="flex-1 text-xs text-center text-rose-600 hover:text-rose-700 font-medium py-1 rounded-md hover:bg-rose-50 transition-colors"
              >
                {t('markAllRead')}
              </button>
              <button
                onClick={handleDeleteAll}
                className="flex-1 text-xs text-center text-gray-500 hover:text-gray-700 font-medium py-1 rounded-md hover:bg-gray-50 transition-colors"
              >
                {t('delete', { ns: 'common' })}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
