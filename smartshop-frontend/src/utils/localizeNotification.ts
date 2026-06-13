import type { TFunction } from 'i18next';

/** Shape tối thiểu để localize 1 notification (dùng chung cho DTO list + payload SignalR). */
export interface LocalizableNotification {
  titleKey?: string;
  messageKey?: string;
  params?: string;          // JSON string từ backend
  title?: string | null;    // legacy fallback (row cũ)
  message?: string | null;  // legacy fallback (row cũ)
}

function parseParams(raw?: string): Record<string, unknown> {
  if (!raw) return {};
  try {
    return (JSON.parse(raw) as Record<string, unknown>) ?? {};
  } catch {
    return {};
  }
}

/**
 * Dịch notification từ key + params sang text theo locale hiện tại.
 * `t` gắn namespace 'common' (chứa object "notification.*"),
 * `tOrder` gắn namespace 'order' (để dịch tham số status_*).
 * Fallback về title/message legacy nếu notification cũ không có key.
 */
export function localizeNotification(
  t: TFunction,
  tOrder: TFunction,
  n: LocalizableNotification
): { title: string; message: string } {
  const params = parseParams(n.params);
  const interp: Record<string, unknown> = { ...params };

  // Tham số status là enum trạng thái đơn → dịch qua namespace order.
  if (typeof params.status === 'string') {
    interp.status = tOrder(`status_${params.status}`, { defaultValue: params.status });
  }

  const title = n.titleKey ? t(n.titleKey, interp) : (n.title ?? '');
  const message = n.messageKey ? t(n.messageKey, interp) : (n.message ?? '');
  return { title, message };
}
