export interface OrderItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface OrderDto {
  id: string;
  userId: string;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  notes: string | null;
  items: OrderItemDto[];
  createdAt: string;
}

export interface PlaceOrderRequest {
  shippingAddress: string;
  notes?: string;
}

export const ORDER_STATUSES = [
  { value: 1, label: 'Chờ xác nhận', key: 'Pending'   },
  { value: 2, label: 'Đã xác nhận',  key: 'Confirmed' },
  { value: 3, label: 'Đang xử lý',   key: 'Processing'},
  { value: 4, label: 'Đang giao',    key: 'Shipped'   },
  { value: 5, label: 'Đã giao',      key: 'Delivered' },
  { value: 6, label: 'Đã hủy',       key: 'Cancelled' },
  { value: 7, label: 'Hoàn tiền',    key: 'Refunded'  },
] as const;

export type OrderStatusValue = (typeof ORDER_STATUSES)[number]['value'];

/** Chuẩn hoá status từ bất kỳ format nào backend trả về:
 *  - C# enum name : "Pending", "Confirmed", ...
 *  - Số nguyên    : 1, 2, ...
 *  - Numeric string: "1", "2", ...
 *  - Label tiếng Việt: "Chờ xác nhận", ...
 */
export function resolveOrderStatus(status: string | number): OrderStatusValue {
  if (typeof status === 'number') return status as OrderStatusValue;
  const n = Number(status);
  return (
    ORDER_STATUSES.find((s) => s.key   === status)?.value ??
    ORDER_STATUSES.find((s) => s.label === status)?.value ??
    (!Number.isNaN(n) ? ORDER_STATUSES.find((s) => s.value === n)?.value : undefined) ??
    1
  ) as OrderStatusValue;
}
