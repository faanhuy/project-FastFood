import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { FiArrowLeft } from 'react-icons/fi';
import { orderService } from '../services/orderService';
import type { OrderDto } from '../types/order';
import { ORDER_STATUSES } from '../types/order';
import { getApiError } from '../utils/errorHandler';
import { formatPrice, formatDateTime } from '../utils/formatters';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [cancelling, setCancelling] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    orderService.getOrderById(id)
      .then(setOrder)
      .catch(() => setError('Không tìm thấy đơn hàng.'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleCancel = async () => {
    if (!order) return;
    if (!confirm('Bạn có chắc muốn huỷ đơn hàng này?')) return;
    setCancelling(true);
    try {
      await orderService.cancelOrder(order.id);
      setOrder({ ...order, status: 'Cancelled' });
      toast.success('Đã huỷ đơn hàng.');
    } catch (err) {
      toast.error(getApiError(err, 'Huỷ đơn hàng thất bại.'));
    } finally {
      setCancelling(false);
    }
  };

  if (loading) return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="p-8 text-center text-gray-400">Đang tải...</div>
    </div>
  );
  if (error || !order) return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="p-8 text-center text-red-500">{error || 'Không tìm thấy đơn hàng.'}</div>
    </div>
  );

  const statusInfo = ORDER_STATUSES.find((s) => s.key === order.status);

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="max-w-2xl mx-auto p-6">
        <button onClick={() => navigate('/orders')} className="text-blue-600 hover:text-blue-800 mb-4 flex items-center gap-1.5 text-sm" title="Quay lại danh sách đơn hàng">
          <FiArrowLeft size={16} /> Đơn hàng của tôi
        </button>

        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold">
            Đơn #{order.id.slice(0, 8).toUpperCase()}
          </h1>
          <span className={`text-sm px-3 py-1 rounded-full font-medium ${statusInfo?.color ?? 'bg-gray-100 text-gray-700'}`}>
            {statusInfo?.label ?? order.status}
          </span>
        </div>

        <div className="bg-gray-50 rounded-lg p-4 mb-6 space-y-1 text-sm text-gray-600">
          <p><span className="font-medium text-gray-800">Ngày đặt:</span> {formatDateTime(order.createdAt)}</p>
          <p><span className="font-medium text-gray-800">Địa chỉ giao:</span> {order.shippingAddress}</p>
          {order.notes && <p><span className="font-medium text-gray-800">Ghi chú:</span> {order.notes}</p>}
        </div>

        <div className="space-y-3 mb-6">
          {order.items.map((item) => (
            <div key={item.productId} className="flex justify-between items-center border-b pb-3">
              <div>
                <p className="font-medium">{item.productName}</p>
                <p className="text-sm text-gray-500">
                  {formatPrice(item.unitPrice)} × {item.quantity}
                </p>
              </div>
              <p className="font-semibold">{formatPrice(item.subTotal)}</p>
            </div>
          ))}
        </div>

        <div className="flex items-center justify-between">
          {order.status === 'Pending' ? (
            <button
              onClick={handleCancel}
              disabled={cancelling}
              className="text-sm text-red-500 border border-red-300 px-4 py-2 rounded-lg hover:bg-red-50 disabled:opacity-50 transition-colors"
            >
              {cancelling ? 'Đang huỷ...' : 'Huỷ đơn hàng'}
            </button>
          ) : <div />}
          <p className="text-xl font-bold text-blue-700">
            Tổng cộng: {formatPrice(order.totalAmount)}
          </p>
        </div>
      </div>
      <Footer />
    </div>
  );
}
