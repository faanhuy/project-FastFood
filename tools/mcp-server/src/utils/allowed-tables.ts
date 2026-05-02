export const ALLOWED_TABLES = new Set([
  'Products', 'Categories', 'Orders', 'OrderItems',
  'Carts', 'CartItems', 'Users', 'Reviews',
  'Coupons', 'CouponUsages', 'AppSettings',
  'ProductEmbeddings', '__EFMigrationsHistory',
]);

const SENSITIVE_COLUMNS = new Set(['PasswordHash', 'RefreshToken', 'RefreshTokenExpiry']);

export function isSensitiveColumn(column: string): boolean {
  return SENSITIVE_COLUMNS.has(column);
}
