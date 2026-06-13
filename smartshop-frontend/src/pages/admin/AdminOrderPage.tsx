import { Fragment, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { FiChevronDown, FiChevronRight } from 'react-icons/fi';
import AdminLayout from '../../components/AdminLayout';
import { AdminRefundModal } from '../../components/admin/AdminRefundModal';
import { OrderTimeline } from '../../components/OrderTimeline';
import { orderService } from '../../services/orderService';
import type { OrderDto, OrderStatusValue, PaymentMethod, PaymentStatus } from '../../types/order';
import { ORDER_STATUSES, resolveOrderStatus } from '../../types/order';
import { formatPrice, formatDateTime } from '../../utils/formatters';
import { getImageUrl } from '../../utils/imageUrl';
import { getApiError } from '../../utils/errorHandler';
import Pagination from '../../components/common/Pagination';
import { BulkActionToolbar, AdminFilterPanel, AdminTableCheckbox } from '../../components/admin';

const PAGE_SIZE = 20;

const PAYMENT_METHOD_LABEL: Record<string, string> = {
  COD: 'COD',
  VNPay: 'VNPay',
  BankTransfer: 'Bank Transfer',
};

// Payment status labels will be dynamically loaded via t() in the component since we need the i18n instance
const PAYMENT_STATUS_CONFIG: Record<string, { cls: string }> = {
  Pending:   { cls: 'bg-yellow-100 text-yellow-700' },
  Paid:      { cls: 'bg-green-100 text-green-700'   },
  Failed:    { cls: 'bg-red-100 text-red-700'       },
  Refunded:  { cls: 'bg-gray-100 text-gray-600'     },
};

function PaymentCell({ method, status }: { method?: PaymentMethod; status?: PaymentStatus }) {
  const { t } = useTranslation('admin');
  const methodLabel = method ? (PAYMENT_METHOD_LABEL[method] ?? method) : '—';
  const st = status ? PAYMENT_STATUS_CONFIG[status] : null;
  const statusLabel = status === 'Pending' ? t('paymentStatusPending') : status === 'Paid' ? t('paymentStatusPaid') : status === 'Failed' ? t('paymentStatusFailed') : t('paymentStatusRefunded');
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs font-medium text-gray-700">{methodLabel}</span>
      {st && (
        <span className={`inline-flex items-center px-1.5 py-0.5 rounded-full text-[10px] font-medium w-fit ${st.cls}`}>
          {statusLabel}
        </span>
      )}
    </div>
  );
}

function PaymentDetail({
  method, status, paidAt, transactionId,
}: {
  method?: PaymentMethod;
  status?: PaymentStatus;
  paidAt?: string | null;
  transactionId?: string | null;
}) {
  const { t } = useTranslation('admin');
  if (!method) return null;
  const methodLabel = PAYMENT_METHOD_LABEL[method] ?? method;
  const st = status ? PAYMENT_STATUS_CONFIG[status] : null;
  const statusLabel = status === 'Pending' ? t('paymentStatusPending') : status === 'Paid' ? t('paymentStatusPaid') : status === 'Failed' ? t('paymentStatusFailed') : t('paymentStatusRefunded');
  return (
    <div className="mb-3 inline-flex flex-wrap gap-x-6 gap-y-1 rounded-lg border border-rose-100 bg-white px-4 py-2.5 text-xs">
      <div className="flex items-center gap-1.5 text-gray-500">
        <span className="font-medium text-gray-700">{t('methodLabel')}</span>
        <span>{methodLabel}</span>
      </div>
      {st && (
        <div className="flex items-center gap-1.5 text-gray-500">
          <span className="font-medium text-gray-700">{t('paymentLabel')}</span>
          <span className={`px-1.5 py-0.5 rounded-full text-[10px] font-medium ${st.cls}`}>{statusLabel}</span>
        </div>
      )}
      {paidAt && (
        <div className="flex items-center gap-1.5 text-gray-500">
          <span className="font-medium text-gray-700">{t('paymentTimeLabel')}</span>
          <span>{new Date(paidAt).toLocaleString('vi-VN')}</span>
        </div>
      )}
      {transactionId && (
        <div className="flex items-center gap-1.5 text-gray-500">
          <span className="font-medium text-gray-700">{t('transactionIdLabel')}</span>
          <span className="font-mono">{transactionId}</span>
        </div>
      )}
    </div>
  );
}

export default function AdminOrderPage() {
  const { t } = useTranslation('admin');
  const [allOrders,    setAllOrders]    = useState<OrderDto[]>([]);
  const [totalCount,   setTotalCount]   = useState(0);
  const [loading,      setLoading]      = useState(true);
  const [page,         setPage]         = useState(1);
  const [statusFilter, setStatusFilter] = useState<number>(0);
  const [updatingIds,  setUpdatingIds]  = useState<Set<string>>(new Set());
  const [expandedId,   setExpandedId]   = useState<string | null>(null);

  // Bulk actions state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [bulkLoading, setBulkLoading] = useState(false);
  // Filter state
  const [filterValues, setFilterValues] = useState<Record<string, string>>({});
  const [sortBy] = useState('createdAt');
  const [sortDirection] = useState<'asc' | 'desc'>('desc');
  // Refund modal state
  const [showRefundModal, setShowRefundModal] = useState(false);
  const [selectedOrderForRefund, setSelectedOrderForRefund] = useState<OrderDto | null>(null);
  // Timeline modal state
  const [showTimelineModal, setShowTimelineModal] = useState(false);
  const [selectedOrderForTimeline, setSelectedOrderForTimeline] = useState<OrderDto | null>(null);

  const isBusy = updatingIds.size > 0;

  const loadOrders = async (p: number, sf: number, showLoading = true) => {
    if (showLoading) setLoading(true);
    try {
      const result = await orderService.getAllOrders({
        page: p,
        pageSize: PAGE_SIZE,
        statusFilter: sf || undefined,
        search: filterValues.search || undefined,
        createdAfter: filterValues.createdAfter || undefined,
        createdBefore: filterValues.createdBefore || undefined,
        sortBy,
        sortDirection,
      });
      setAllOrders(result.items);
      setTotalCount(result.totalCount);
    } catch {
      toast.error(t('error', { ns: 'common' }));
    } finally {
      if (showLoading) setLoading(false);
    }
  };

  useEffect(() => { loadOrders(page, statusFilter); }, [page, statusFilter, filterValues, sortBy, sortDirection]);

  const handleFilterChange = (value: number) => {
    setStatusFilter(value);
    setPage(1);
    setExpandedId(null);
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedIds(new Set(allOrders.map((o) => o.id)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectOne = (id: string, checked: boolean) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (checked) next.add(id);
      else next.delete(id);
      return next;
    });
  };

  const handleBulkAction = async (actionId: string) => {
    const ids = Array.from(selectedIds);
    if (!confirm(t('bulkConfirmMessage', { count: ids.length }))) return;

    setBulkLoading(true);
    try {
      const result = await orderService.bulkUpdateOrders(ids, actionId as any);
      toast.success(t('bulkResultSuccess', { count: result.succeeded }));
      if (result.failed > 0) {
        toast.error(t('bulkResultFailed', { count: result.failed }));
      }
      setSelectedIds(new Set());
      await loadOrders(page, statusFilter);
    } catch (err) {
      toast.error(getApiError(err, t('error', { ns: 'common' })));
    } finally {
      setBulkLoading(false);
    }
  };

  const bulkActions = [
    { id: 'confirm', label: t('bulkConfirm'), variant: 'default' as const },
    { id: 'cancel', label: t('bulkCancel'), variant: 'danger' as const },
  ];

  const filterFields = [
    { key: 'search', type: 'text' as const, label: t('filterSearch'), placeholder: t('filterSearch') },
    { key: 'createdAfter', type: 'date' as const, label: t('filterDateFrom') },
    { key: 'createdBefore', type: 'date' as const, label: t('filterDateTo') },
  ];

  const handleStatusChange = async (orderId: string, newStatus: OrderStatusValue) => {
    if (updatingIds.has(orderId)) return;

    const optimisticStatus =
      ORDER_STATUSES.find((s) => s.value === newStatus)?.key ?? String(newStatus);

    setUpdatingIds((prev) => {
      const next = new Set(prev);
      next.add(orderId);
      return next;
    });

    // Optimistic update so admin sees change immediately.
    setAllOrders((prev) =>
      prev.map((order) =>
        order.id === orderId ? { ...order, status: optimisticStatus } : order
      )
    );

    try {
      await orderService.updateOrderStatus(orderId, newStatus);
      await loadOrders(page, statusFilter, false);
      toast.success(t('orderStatusUpdated'));
    } catch {
      toast.error(t('error', { ns: 'common' }));
      await loadOrders(page, statusFilter, false);
    } finally {
      setUpdatingIds((prev) => {
        const next = new Set(prev);
        next.delete(orderId);
        return next;
      });
    }
  };

  const totalPages = Math.ceil(totalCount / PAGE_SIZE) || 1;

  return (
    <AdminLayout title={t('ordersTitle')}>
      {/* Filter Panel */}
      <div className="mb-4">
        <AdminFilterPanel
          fields={filterFields}
          values={filterValues}
          onChange={(key, value) => {
            setFilterValues((prev) => ({ ...prev, [key]: value }));
            setPage(1);
          }}
          onApply={() => loadOrders(1, statusFilter)}
          onReset={() => {
            setFilterValues({});
            setPage(1);
          }}
          isLoading={loading}
        />
      </div>

      {/* Status filter buttons */}
      <div className="flex gap-1.5 flex-wrap mb-4">
        <button
          onClick={() => handleFilterChange(0)}
          disabled={isBusy}
          className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors ${
            statusFilter === 0
              ? 'bg-gray-900 text-white border-gray-900'
              : 'bg-white text-gray-600 border-gray-200 hover:border-gray-400'
          } disabled:opacity-50 disabled:cursor-not-allowed`}
        >
          {t('all', { ns: 'common' })}
        </button>
        {ORDER_STATUSES.map((s) => (
          <button
            key={s.value}
            onClick={() => handleFilterChange(s.value)}
            disabled={isBusy}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors ${
              statusFilter === s.value
                ? 'bg-gray-900 text-white border-gray-900'
                : 'bg-white text-gray-600 border-gray-200 hover:border-gray-400'
            } disabled:opacity-50 disabled:cursor-not-allowed`}
          >
            {s.label}
            {statusFilter === s.value && !loading && (
              <span className="ml-1.5 opacity-60">({totalCount})</span>
            )}
          </button>
        ))}
      </div>

      {isBusy && (
        <div className="mb-3 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-xs text-amber-800 flex items-center gap-2">
          <svg className="h-3.5 w-3.5 animate-spin text-amber-700" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
          </svg>
          {t('adminOrderProcessingMsg', { count: updatingIds.size })}
        </div>
      )}

      <p className="text-xs text-gray-400 mb-3">{t('adminOrderTotal')} {totalCount} {t('statOrders')}</p>

      {/* Orders table */}
      {loading ? (
        <div className="flex items-center justify-center h-64 text-gray-400">{t('loading', { ns: 'common' })}</div>
      ) : (
        <>
          <div className="bg-white rounded-xl border shadow-sm overflow-x-auto">
            {allOrders.length === 0 ? (
              <p className="text-center text-gray-400 py-12">{t('noData', { ns: 'common' })}</p>
            ) : (
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-500 text-xs uppercase border-b">
                  <tr>
                    <th className="w-8 px-3 py-3">
                      <AdminTableCheckbox
                        checked={selectedIds.size > 0 && selectedIds.size === allOrders.length}
                        indeterminate={selectedIds.size > 0 && selectedIds.size < allOrders.length}
                        onChange={(checked) => handleSelectAll(checked)}
                      />
                    </th>
                    <th className="w-8 px-3 py-3" />
                    <th className="px-4 py-3 text-left">{t('orderCode')}</th>
                    <th className="px-4 py-3 text-left">{t('customer')}</th>
                    <th className="px-4 py-3 text-left hidden sm:table-cell">{t('address')}</th>
                    <th className="px-4 py-3 text-right">{t('total')}</th>
                    <th className="px-4 py-3 text-left hidden lg:table-cell">{t('payment')}</th>
                    <th className="px-4 py-3 text-left hidden md:table-cell">{t('createdAt')}</th>
                    <th className="px-4 py-3 text-left">{t('status')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {allOrders.map((order) => {
                    const statusVal  = resolveOrderStatus(order.status);
                    const statusInfo = ORDER_STATUSES.find((s) => s.value === statusVal);
                    const isExpanded = expandedId === order.id;

                    return (
                      <Fragment key={order.id}>
                        <tr
                          className="hover:bg-gray-50 cursor-pointer transition-colors"
                          onClick={() => setExpandedId(isExpanded ? null : order.id)}
                        >
                          <td className="px-3 py-3 text-gray-400" onClick={(e) => e.stopPropagation()}>
                            <AdminTableCheckbox
                              checked={selectedIds.has(order.id)}
                              onChange={(checked) => handleSelectOne(order.id, checked)}
                            />
                          </td>
                          <td className="px-3 py-3 text-gray-400">
                            {isExpanded ? <FiChevronDown size={14} /> : <FiChevronRight size={14} />}
                          </td>
                          <td className="px-4 py-3 font-mono text-xs text-gray-500">
                            {order.id.slice(0, 8)}…
                          </td>
                          <td className="px-4 py-3 text-xs text-gray-700">
                            {order.userName || order.userId.slice(0, 8) + '…'}
                          </td>
                          <td className="px-4 py-3 text-gray-500 max-w-xs truncate text-xs hidden sm:table-cell">
                            {order.shippingAddress}
                          </td>
                          <td className="px-4 py-3 text-right font-semibold text-rose-600 whitespace-nowrap">
                            {formatPrice(order.totalAmount)}
                          </td>
                          <td className="px-4 py-3 hidden lg:table-cell">
                            <PaymentCell method={order.paymentMethod} status={order.paymentStatus} />
                          </td>
                          <td className="px-4 py-3 text-gray-400 whitespace-nowrap text-xs hidden md:table-cell">
                            {formatDateTime(order.createdAt)}
                          </td>
                          <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                            <div className="flex flex-col gap-1.5 min-w-[130px]">
                              <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusInfo?.color ?? 'bg-gray-100 text-gray-700'}`}>
                                {statusInfo?.label ?? order.status}
                              </span>
                              <div className="relative">
                                <select
                                  value={statusVal}
                                  onChange={(e) =>
                                    handleStatusChange(order.id, Number(e.target.value) as OrderStatusValue)
                                  }
                                  disabled={isBusy}
                                  className="text-xs border border-gray-200 rounded-md px-1.5 py-1 focus:outline-none focus:ring-1 focus:ring-rose-300 disabled:opacity-50 disabled:cursor-not-allowed bg-white text-gray-600 cursor-pointer w-full"
                                >
                                  {ORDER_STATUSES.map((s) => (
                                    <option key={s.value} value={s.value}>{s.label}</option>
                                  ))}
                                </select>
                                {updatingIds.has(order.id) && (
                                  <div className="absolute inset-0 flex items-center justify-center bg-white/80 rounded-md">
                                    <svg className="w-3.5 h-3.5 animate-spin text-rose-500" viewBox="0 0 24 24" fill="none">
                                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                                    </svg>
                                    <span className="ml-1 text-[10px] text-rose-500 font-medium">{t('updating')}</span>
                                  </div>
                                )}
                              </div>
                            </div>
                          </td>
                        </tr>

                        {isExpanded && (
                          <tr className="bg-rose-50/40">
                            <td colSpan={9} className="px-8 py-4">
                              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
                                {t('adminOrderDetails', { count: order.items.length })}
                              </p>
                              {order.notes && (
                                <p className="text-xs text-gray-500 mb-2 italic">{t('adminOrderNotes')} {order.notes}</p>
                              )}
                              <div className="mb-4 flex gap-2">
                                <button
                                  onClick={() => {
                                    setSelectedOrderForRefund(order);
                                    setShowRefundModal(true);
                                  }}
                                  className="px-3 py-1.5 rounded-lg text-xs font-medium bg-green-50 text-green-700 border border-green-200 hover:bg-green-100 transition-colors"
                                >
                                  {t('refundOrderBtn')}
                                </button>
                                <button
                                  onClick={() => {
                                    setSelectedOrderForTimeline(order);
                                    setShowTimelineModal(true);
                                  }}
                                  className="px-3 py-1.5 rounded-lg text-xs font-medium bg-blue-50 text-blue-700 border border-blue-200 hover:bg-blue-100 transition-colors"
                                >
                                  {t('viewTimeline')}
                                </button>
                              </div>
                              <PaymentDetail
                                method={order.paymentMethod}
                                status={order.paymentStatus}
                                paidAt={order.paidAt}
                                transactionId={order.vnpayTransactionId}
                              />
                              <table className="w-full text-xs">
                                <thead>
                                  <tr className="text-gray-400 text-left border-b border-rose-100">
                                    <th className="pb-1.5 font-medium pr-4">{t('adminOrderImage')}</th>
                                    <th className="pb-1.5 font-medium pr-4">{t('adminOrderProduct')}</th>
                                    <th className="pb-1.5 font-medium text-right pr-4">{t('adminOrderPrice')}</th>
                                    <th className="pb-1.5 font-medium text-center pr-4">{t('adminOrderQty')}</th>
                                    <th className="pb-1.5 font-medium text-right">{t('adminOrderSubtotal')}</th>
                                  </tr>
                                </thead>
                                <tbody className="divide-y divide-rose-100/60">
                                  {order.items.map((item) => (
                                    <tr key={item.productId}>
                                      <td className="py-1.5 pr-4">
                                        <div className="h-10 w-10 overflow-hidden rounded-md border border-rose-100 bg-white">
                                          {item.productImageUrl ? (
                                            <img
                                              src={getImageUrl(item.productImageUrl)}
                                              alt={item.productName}
                                              className="h-full w-full object-cover"
                                            />
                                          ) : (
                                            <div className="flex h-full w-full items-center justify-center text-sm">🍔</div>
                                          )}
                                        </div>
                                      </td>
                                      <td className="py-1.5 pr-4 text-gray-700 font-medium">{item.productName}</td>
                                      <td className="py-1.5 pr-4 text-right text-gray-500">{formatPrice(item.unitPrice)}</td>
                                      <td className="py-1.5 pr-4 text-center text-gray-600">×{item.quantity}</td>
                                      <td className="py-1.5 text-right font-semibold text-rose-600">{formatPrice(item.subTotal)}</td>
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

          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} disabled={loading || isBusy} />

          {/* Bulk Action Toolbar */}
          {selectedIds.size > 0 && (
            <BulkActionToolbar
              selectedCount={selectedIds.size}
              actions={bulkActions}
              onAction={handleBulkAction}
              isLoading={bulkLoading}
            />
          )}
        </>
      )}

      {/* Refund Modal */}
      {showRefundModal && selectedOrderForRefund && (
        <AdminRefundModal
          orderId={selectedOrderForRefund.id}
          orderItems={selectedOrderForRefund.items}
          totalAmount={selectedOrderForRefund.totalAmount}
          onSuccess={() => loadOrders(page, statusFilter, false)}
          onClose={() => {
            setShowRefundModal(false);
            setSelectedOrderForRefund(null);
          }}
        />
      )}

      {/* Timeline Modal */}
      {showTimelineModal && selectedOrderForTimeline && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex min-h-screen items-center justify-center bg-black bg-opacity-50 p-4">
            <div className="relative w-full max-w-2xl rounded-lg bg-white shadow-xl">
              <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
                <h3 className="text-lg font-semibold text-gray-900">{t('viewTimeline')}</h3>
                <button
                  onClick={() => {
                    setShowTimelineModal(false);
                    setSelectedOrderForTimeline(null);
                  }}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <span className="text-2xl leading-none">&times;</span>
                </button>
              </div>
              <div className="px-6 py-4">
                <OrderTimeline orderId={selectedOrderForTimeline.id} />
              </div>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}
