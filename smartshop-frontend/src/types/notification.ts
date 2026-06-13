export interface Notification {
  id: string;
  titleKey?: string;
  messageKey?: string;
  params?: string;          // JSON string từ backend
  title?: string | null;    // legacy fallback
  message?: string | null;  // legacy fallback
  isRead: boolean;
  orderId?: string;
  createdAt: string;
}
