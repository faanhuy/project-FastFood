import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { cartService } from '../services/cartService';
import type { CartDto } from '../types/cart';
import { FiHome, FiMinus, FiPlus, FiX, FiTrash2, FiShoppingBag, FiShoppingCart } from 'react-icons/fi';

export default function CartPage() {
  const [cart, setCart] = useState<CartDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const loadCart = async () => {
    try {
      const data = await cartService.getCart();
      setCart(data);
    } catch {
      setError('Không thể tải giỏ hàng.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCart();
  }, []);

  const handleUpdateQuantity = async (productId: string, quantity: number) => {
    if (quantity <= 0) return handleRemove(productId);
    try {
      const updated = await cartService.updateItem(productId, quantity);
      setCart(updated);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors?.[0];
      alert(msg ?? 'Cập nhật thất bại.');
    }
  };

  const handleRemove = async (productId: string) => {
    try {
      const updated = await cartService.removeItem(productId);
      setCart(updated);
    } catch {
      alert('Xoá sản phẩm thất bại.');
    }
  };

  const handleClear = async () => {
    if (!confirm('Xoá toàn bộ giỏ hàng?')) return;
    try {
      await cartService.clearCart();
      setCart(null);
    } catch {
      alert('Xoá giỏ hàng thất bại.');
    }
  };

  if (loading) return <div className="p-8 text-center">Đang tải...</div>;

  return (
    <div className="max-w-3xl mx-auto p-6">
      <div className="flex items-center gap-4 mb-6">
        <button
          onClick={() => navigate('/')}
          className="text-gray-500 hover:text-gray-700"
          title="Trang chủ"
        >
          <FiHome size={20} />
        </button>
        <h1 className="text-2xl font-bold">Giỏ hàng</h1>
      </div>

      {error && <p className="text-red-500 mb-4">{error}</p>}

      {!cart || cart.items.length === 0 ? (
        <div className="text-center py-16 text-gray-500">
          <p className="text-lg mb-4">Giỏ hàng trống</p>
          <button
            onClick={() => navigate('/products')}
            className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700 flex items-center gap-2 mx-auto"
            title="Tiếp tục mua sắm"
          >
            <FiShoppingCart size={18} />
            Tiếp tục mua sắm
          </button>
        </div>
      ) : (
        <>
          <div className="space-y-4 mb-6">
            {cart.items.map((item) => (
              <div key={item.productId} className="flex items-center gap-4 border rounded-lg p-4">
                <div className="w-16 h-16 bg-gray-100 rounded flex-shrink-0 overflow-hidden">
                  {item.productImageUrl ? (
                    <img src={item.productImageUrl} alt={item.productName} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">No img</div>
                  )}
                </div>
                <div className="flex-1">
                  <p className="font-medium">{item.productName}</p>
                  <p className="text-sm text-gray-500">{item.unitPrice.toLocaleString('vi-VN')}₫ / sản phẩm</p>
                </div>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleUpdateQuantity(item.productId, item.quantity - 1)}
                    className="w-8 h-8 rounded-full border flex items-center justify-center hover:bg-gray-100"
                  >
                    <FiMinus size={14} />
                  </button>
                  <span className="w-8 text-center">{item.quantity}</span>
                  <button
                    onClick={() => handleUpdateQuantity(item.productId, item.quantity + 1)}
                    className="w-8 h-8 rounded-full border flex items-center justify-center hover:bg-gray-100"
                  >
                    <FiPlus size={14} />
                  </button>
                </div>
                <p className="w-28 text-right font-semibold">
                  {item.subTotal.toLocaleString('vi-VN')}₫
                </p>
                <button
                  onClick={() => handleRemove(item.productId)}
                  className="text-red-400 hover:text-red-600 ml-2"
                  title="Xoá sản phẩm"
                >
                  <FiX size={18} />
                </button>
              </div>
            ))}
          </div>

          <div className="border-t pt-4 flex items-center justify-between">
            <button onClick={handleClear} className="text-red-500 hover:text-red-700" title="Xoá giỏ hàng">
              <FiTrash2 size={20} />
            </button>
            <div className="text-right">
              <p className="text-lg font-bold mb-3">
                Tổng: {cart.totalAmount.toLocaleString('vi-VN')}₫
              </p>
              <button
                onClick={() => navigate('/checkout')}
                className="bg-blue-600 text-white px-8 py-2 rounded-lg hover:bg-blue-700 font-semibold flex items-center gap-2"
                title="Đặt hàng"
              >
                <FiShoppingBag size={18} />
                Đặt hàng
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
