export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  orderId?: string;
  createdAt: string;
}
