import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderService } from '../services/orderService';
import { FiArrowLeft } from 'react-icons/fi';

export default function CheckoutPage() {
  const [shippingAddress, setShippingAddress] = useState('');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!shippingAddress.trim()) {
      setError('Vui lòng nhập địa chỉ giao hàng.');
      return;
    }
    setLoading(true);
    setError('');
    try {
      const order = await orderService.placeOrder({
        shippingAddress: shippingAddress.trim(),
        notes: notes.trim() || undefined,
      });
      navigate(`/orders/${order.id}`, { replace: true });
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors?.[0];
      setError(msg ?? 'Đặt hàng thất bại, vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-lg mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Xác nhận đơn hàng</h1>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Địa chỉ giao hàng <span className="text-red-500">*</span>
          </label>
          <textarea
            value={shippingAddress}
            onChange={(e) => setShippingAddress(e.target.value)}
            rows={3}
            className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Số nhà, đường, phường/xã, quận/huyện, tỉnh/thành phố"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Ghi chú (tuỳ chọn)
          </label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={2}
            className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Yêu cầu đặc biệt, giờ giao hàng..."
          />
        </div>

        <div className="flex gap-3 pt-2">
          <button
            type="button"
            onClick={() => navigate('/cart')}
            className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 flex items-center justify-center gap-2"
            title="Quay lại giỏ hàng"
          >
            <FiArrowLeft size={16} />
            Giỏ hàng
          </button>
          <button
            type="submit"
            disabled={loading}
            className="flex-1 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-semibold"
          >
            {loading ? 'Đang xử lý...' : 'Xác nhận đặt hàng'}
          </button>
        </div>
      </form>
    </div>
  );
}
