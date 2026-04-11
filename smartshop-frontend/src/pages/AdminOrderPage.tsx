import { Fragment, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { FiChevronDown, FiChevronRight } from 'react-icons/fi';
import AdminLayout from '../components/AdminLayout';
import { orderService } from '../services/orderService';
import type { OrderDto, OrderStatusValue } from '../types/order';
import { ORDER_STATUSES, resolveOrderStatus } from '../types/order';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

const formatDate = (iso: string) =>
  new Date(iso).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });

// Màu badge theo trạng thái
const STATUS_STYLES: Record<number, string> = {
  1: 'bg-yellow-100 text-yellow-700',
  2: 'bg-blue-100   text-blue-700',
  3: 'bg-purple-100 text-purple-700',
  4: 'bg-orange-100 text-orange-700',
  5: 'bg-green-100  text-green-700',
  6: 'bg-red-100    text-red-600',
  7: 'bg-gray-100   text-gray-600',
};


const PAGE_SIZE = 20;

export default function AdminOrderPage() {
  const [allOrders,   setAllOrders]   = useState<OrderDto[]>([]);
  const [totalCount,  setTotalCount]  = useState(0);
  const [loading,     setLoading]     = useState(true);
  const [page,        setPage]        = useState(1);
  const [statusFilter, setStatusFilter] = useState<number>(0); // 0 = tất cả
  const [updatingId,  setUpdatingId]  = useState<string | null>(null);
  const [expandedId,  setExpandedId]  = useState<string | null>(null);

  const loadOrders = async (p: number) => {
    setLoading(true);
    try {
      const result = await orderService.getAllOrders(p, PAGE_SIZE);
      setAllOrders(result.items);
      setTotalCount(result.totalCount);
    } catch {
      toast.error('Không thể tải danh sách đơn hàng.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadOrders(page); }, [page]);

  // Đổi filter → reset về trang 1
  const handleFilterChange = (value: number) => {
    setStatusFilter(value);
    setExpandedId(null);
  };

  // Client-side filter trong trang hiện tại
  const filtered = statusFilter === 0
    ? allOrders
    : allOrders.filter((o) => resolveOrderStatus(o.status) === statusFilter);

  const handleStatusChange = async (orderId: string, newStatus: OrderStatusValue) => {
    setUpdatingId(orderId);
    try {
      await orderService.updateOrderStatus(orderId, newStatus);
      await loadOrders(page); // re-fetch từ DB — đảm bảo đồng nhất
      toast.success('Đã cập nhật trạng thái.');
    } catch {
      toast.error('Cập nhật thất bại.');
    } finally {
      setUpdatingId(null);
    }
  };

  const totalPages = Math.ceil(totalCount / PAGE_SIZE) || 1;

  return (
    <AdminLayout title="Quản lý đơn hàng">
      {/* ── Bộ lọc trạng thái ── */}
      <div className="flex gap-1.5 flex-wrap mb-4">
        <button
          onClick={() => handleFilterChange(0)}
          className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors ${
            statusFilter === 0
              ? 'bg-gray-900 text-white border-gray-900'
              : 'bg-white text-gray-600 border-gray-200 hover:border-gray-400'
          }`}
        >
          Tất cả
        </button>
        {ORDER_STATUSES.map((s) => {
          const count = allOrders.filter((o) => resolveOrderStatus(o.status) === s.value).length;
          return (
            <button
              key={s.value}
              onClick={() => handleFilterChange(s.value)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors ${
                statusFilter === s.value
                  ? 'bg-gray-900 text-white border-gray-900'
                  : 'bg-white text-gray-600 border-gray-200 hover:border-gray-400'
              }`}
            >
              {s.label}
              {!loading && count > 0 && (
                <span className="ml-1.5 opacity-60">({count})</span>
              )}
            </button>
          );
        })}
      </div>

      <p className="text-xs text-gray-400 mb-3">
        Tổng {totalCount} đơn hàng
        {statusFilter !== 0 && ` — đang lọc: ${filtered.length} đơn trên trang này`}
      </p>

      {/* ── Bảng đơn hàng ── */}
      {loading ? (
        <div className="flex items-center justify-center h-64 text-gray-400">Đang tải...</div>
      ) : (
        <>
          <div className="bg-white rounded-xl border shadow-sm overflow-x-auto">
            {filtered.length === 0 ? (
              <p className="text-center text-gray-400 py-12">Không có đơn hàng nào.</p>
            ) : (
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-500 text-xs uppercase border-b">
                  <tr>
                    <th className="w-8 px-3 py-3" />
                    <th className="px-4 py-3 text-left">Mã đơn</th>
                    <th className="px-4 py-3 text-left">Người đặt</th>
                    <th className="px-4 py-3 text-left hidden sm:table-cell">Địa chỉ</th>
                    <th className="px-4 py-3 text-right">Tổng tiền</th>
                    <th className="px-4 py-3 text-left hidden md:table-cell">Ngày đặt</th>
                    <th className="px-4 py-3 text-left">Trạng thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {filtered.map((order) => {
                    const statusVal   = resolveOrderStatus(order.status);
                    const statusStyle = STATUS_STYLES[statusVal] ?? 'bg-gray-100 text-gray-600';
                    const statusLabel = ORDER_STATUSES.find((s) => s.value === statusVal)?.label ?? order.status;
                    const isExpanded  = expandedId === order.id;

                    return (
                      <Fragment key={order.id}>
                        {/* ── Row chính ── */}
                        <tr
                          className="hover:bg-gray-50 cursor-pointer transition-colors"
                          onClick={() => setExpandedId(isExpanded ? null : order.id)}
                        >
                          <td className="px-3 py-3 text-gray-400">
                            {isExpanded
                              ? <FiChevronDown size={14} />
                              : <FiChevronRight size={14} />}
                          </td>
                          <td className="px-4 py-3 font-mono text-xs text-gray-500">
                            {order.id.slice(0, 8)}…
                          </td>
                          <td className="px-4 py-3 text-xs text-gray-700">
                            {order.userId.slice(0, 8)}…
                          </td>
                          <td className="px-4 py-3 text-gray-500 max-w-xs truncate text-xs hidden sm:table-cell">
                            {order.shippingAddress}
                          </td>
                          <td className="px-4 py-3 text-right font-semibold text-blue-600 whitespace-nowrap">
                            {formatPrice(order.totalAmount)}
                          </td>
                          <td className="px-4 py-3 text-gray-400 whitespace-nowrap text-xs hidden md:table-cell">
                            {formatDate(order.createdAt)}
                          </td>
                          {/* Trạng thái: badge + dropdown đổi trạng thái */}
                          <td
                            className="px-4 py-3"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <div className="flex flex-col gap-1.5 min-w-[130px]">
                              <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusStyle}`}>
                                {statusLabel}
                              </span>
                              <select
                                value={statusVal}
                                onChange={(e) =>
                                  handleStatusChange(order.id, Number(e.target.value) as OrderStatusValue)
                                }
                                disabled={updatingId === order.id}
                                className="text-xs border border-gray-200 rounded-md px-1.5 py-1 focus:outline-none focus:ring-1 focus:ring-blue-300 disabled:opacity-50 bg-white text-gray-600 cursor-pointer"
                              >
                                {ORDER_STATUSES.map((s) => (
                                  <option key={s.value} value={s.value}>{s.label}</option>
                                ))}
                              </select>
                            </div>
                          </td>
                        </tr>

                        {/* ── Row mở rộng: chi tiết sản phẩm ── */}
                        {isExpanded && (
                          <tr className="bg-blue-50/40">
                            <td colSpan={7} className="px-8 py-4">
                              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
                                Chi tiết đơn · {order.items.length} sản phẩm
                              </p>
                              {order.notes && (
                                <p className="text-xs text-gray-500 mb-3 italic">
                                  Ghi chú: {order.notes}
                                </p>
                              )}
                              <table className="w-full text-xs">
                                <thead>
                                  <tr className="text-gray-400 text-left border-b border-blue-100">
                                    <th className="pb-1.5 font-medium pr-4">Sản phẩm</th>
                                    <th className="pb-1.5 font-medium text-right pr-4">Đơn giá</th>
                                    <th className="pb-1.5 font-medium text-center pr-4">SL</th>
                                    <th className="pb-1.5 font-medium text-right">Thành tiền</th>
                                  </tr>
                                </thead>
                                <tbody className="divide-y divide-blue-100/60">
                                  {order.items.map((item) => (
                                    <tr key={item.productId}>
                                      <td className="py-1.5 pr-4 text-gray-700 font-medium">
                                        {item.productName}
                                      </td>
                                      <td className="py-1.5 pr-4 text-right text-gray-500">
                                        {formatPrice(item.unitPrice)}
                                      </td>
                                      <td className="py-1.5 pr-4 text-center text-gray-600">
                                        ×{item.quantity}
                                      </td>
                                      <td className="py-1.5 text-right font-semibold text-blue-600">
                                        {formatPrice(item.subTotal)}
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </td>
                          </tr>
                        )}
                      </Fragment>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>

          {/* ── Phân trang ── */}
          {totalPages > 1 && (
            <div className="mt-4 flex justify-center gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
              >
                ← Trước
              </button>
              <span className="px-3 py-1.5 text-sm text-gray-500">
                Trang {page} / {totalPages}
              </span>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
              >
                Sau →
              </button>
            </div>
          )}
        </>
      )}
    </AdminLayout>
  );
}
