import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { orderService } from '../services/orderService';
import type { OrderDto } from '../types/order';
import { FiArrowLeft } from 'react-icons/fi';

const STATUS_LABEL: Record<string, string> = {
  Pending: 'Chờ xác nhận',
  Confirmed: 'Đã xác nhận',
  Processing: 'Đang xử lý',
  Shipped: 'Đang giao',
  Delivered: 'Đã giao',
  Cancelled: 'Đã huỷ',
  Refunded: 'Đã hoàn tiền',
};

const STATUS_COLOR: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Processing: 'bg-purple-100 text-purple-700',
  Shipped: 'bg-indigo-100 text-indigo-700',
  Delivered: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
  Refunded: 'bg-gray-100 text-gray-700',
};

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    orderService.getOrderById(id)
      .then(setOrder)
      .catch(() => setError('Không tìm thấy đơn hàng.'))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="p-8 text-center">Đang tải...</div>;
  if (error || !order) return (
    <div className="p-8 text-center text-red-500">
      {error || 'Không tìm thấy đơn hàng.'}
    </div>
  );

  return (
    <div className="max-w-2xl mx-auto p-6">
      <button onClick={() => navigate('/orders')} className="text-blue-600 hover:text-blue-800 mb-4 block" title="Quay lại danh sách đơn hàng">
        <FiArrowLeft size={20} />
      </button>

      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">
          Đơn #{order.id.slice(0, 8).toUpperCase()}
        </h1>
        <span className={`text-sm px-3 py-1 rounded-full font-medium ${STATUS_COLOR[order.status] ?? 'bg-gray-100'}`}>
          {STATUS_LABEL[order.status] ?? order.status}
        </span>
      </div>

      <div className="bg-gray-50 rounded-lg p-4 mb-6 space-y-1 text-sm text-gray-600">
        <p><span className="font-medium text-gray-800">Ngày đặt:</span> {new Date(order.createdAt).toLocaleString('vi-VN')}</p>
        <p><span className="font-medium text-gray-800">Địa chỉ giao:</span> {order.shippingAddress}</p>
        {order.notes && <p><span className="font-medium text-gray-800">Ghi chú:</span> {order.notes}</p>}
      </div>

      <div className="space-y-3 mb-6">
        {order.items.map((item) => (
          <div key={item.productId} className="flex justify-between items-center border-b pb-3">
            <div>
              <p className="font-medium">{item.productName}</p>
              <p className="text-sm text-gray-500">
                {item.unitPrice.toLocaleString('vi-VN')}₫ × {item.quantity}
              </p>
            </div>
            <p className="font-semibold">{item.subTotal.toLocaleString('vi-VN')}₫</p>
          </div>
        ))}
      </div>

      <div className="text-right">
        <p className="text-xl font-bold text-blue-700">
          Tổng cộng: {order.totalAmount.toLocaleString('vi-VN')}₫
        </p>
      </div>
    </div>
  );
}
