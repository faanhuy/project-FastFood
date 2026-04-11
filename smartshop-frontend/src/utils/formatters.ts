export const formatPrice = (price: number): string =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

export const formatDate = (iso: string): string =>
  new Date(iso).toLocaleDateString('vi-VN', { dateStyle: 'medium' });

export const formatDateTime = (iso: string): string =>
  new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
