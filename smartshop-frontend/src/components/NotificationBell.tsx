import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { notificationService } from '@/services/notificationService';
import type { Notification } from '@/types/notification';
import { useNotificationStore } from '@/store/notificationStore';

const STATUS_MAP: Record<string, string> = {
  Pending: 'Chờ xác nhận',
  Confirmed: 'Đã xác nhận',
  Shipped: 'Đang giao hàng',
  Delivered: 'Đã giao hàng',
  Cancelled: 'Đã hủy',
};

function localizeMessage(message: string): string {
  return Object.entries(STATUS_MAP).reduce(
    (msg, [en, vi]) => msg.replace(new RegExp(en, 'g'), vi),
    message
  );
}

function formatRelativeTime(isoString: string): string {
  const normalized = isoString.endsWith('Z') ? isoString : isoString + 'Z';
  const diff = Date.now() - new Date(normalized).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'Vừa xong';
  if (minutes < 60) return `${minutes} phút trước`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} giờ trước`;
  return `${Math.floor(hours / 24)} ngày trước`;
}

export default function NotificationBell() {
  const navigate = useNavigate();
  const { notifications, markOneRead, markAllRead } = useNotificationStore();
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
        toast.error('Có lỗi xảy ra');
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
      toast.error(error.response?.data?.message ?? 'Có lỗi xảy ra');
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setOpen((o) => !o)}
        className="relative text-gray-500 hover:text-rose-600 p-1"
        title="Thông báo"
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
            <span className="font-semibold text-sm text-gray-800">Thông báo</span>
            {unreadCount > 0 && (
              <span className="text-xs text-rose-600 font-medium">{unreadCount} chưa đọc</span>
            )}
          </div>

          <div className="overflow-y-auto flex-1">
            {notifications.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-gray-400">
                Chưa có thông báo nào
              </div>
            ) : (
              notifications.map((n) => (
                <button
                  key={n.id}
                  onClick={() => handleNotificationClick(n)}
                  className={`w-full text-left px-4 py-3 border-b border-gray-50 hover:bg-gray-50 transition-colors ${
                    !n.isRead ? 'bg-rose-50' : ''
                  }`}
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
                      <p className="text-[11px] text-gray-400 mt-1">{formatRelativeTime(n.createdAt)}</p>
                    </div>
                  </div>
                </button>
              ))
            )}
          </div>

          {notifications.length > 0 && (
            <div className="px-4 py-2.5 border-t border-gray-100">
              <button
                onClick={handleMarkAllRead}
                className="w-full text-xs text-center text-rose-600 hover:text-rose-700 font-medium py-1"
              >
                Đọc tất cả
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
