import i18n from '../i18n';

export const formatPrice = (price: number): string => {
  const locale = i18n.language === 'en' ? 'en-US' : 'vi-VN';
  return new Intl.NumberFormat(locale, { style: 'currency', currency: 'VND' }).format(price);
};

export const formatDate = (iso: string): string => {
  const locale = i18n.language === 'en' ? 'en-US' : 'vi-VN';
  return new Date(iso).toLocaleDateString(locale, { dateStyle: 'medium' });
};

export const formatDateTime = (iso: string): string => {
  const date = new Date(iso);

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');

  return `${day}/${month}/${year} ${hours}:${minutes}`;
};
