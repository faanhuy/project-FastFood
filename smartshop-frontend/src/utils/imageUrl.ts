/**
 * Chuyển đổi image path thành URL đầy đủ.
 * - Nếu path đã là http/https → giữ nguyên (external URL)
 * - Nếu path là relative (/images/...) → prepend API host
 * - Nếu null/undefined → trả về ''
 */
const API_BASE = (import.meta.env.VITE_API_URL ?? 'http://localhost:5284/api')
  .replace(/\/api$/, ''); // "http://localhost:5284"

export function getImageUrl(path?: string | null): string {
  if (!path) return '/images/products/default.png';
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  // Đảm bảo path bắt đầu bằng /
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE}${normalized}`;
}
