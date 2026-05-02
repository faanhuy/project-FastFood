import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useAuthStore } from '@/store/authStore';
import { wishlistService } from '@/services/wishlistService';
import { cartService } from '@/services/cartService';
import type { WishlistItem } from '@/types/wishlist';
import Navbar from '@/components/Navbar';
import { getImageUrl } from '../utils/imageUrl';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

export default function WishlistPage() {
  const { isAuthenticated, refreshCartCount } = useAuthStore();
  const navigate = useNavigate();
  const [items, setItems] = useState<WishlistItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [removingId, setRemovingId] = useState<string | null>(null);
  const [addingId, setAddingId] = useState<string | null>(null);

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  useEffect(() => {
    wishlistService
      .getWishlist()
      .then(setItems)
      .catch(() => toast.error('Không thể tải danh sách yêu thích'))
      .finally(() => setLoading(false));
  }, []);

  const handleRemove = async (productId: string) => {
    setRemovingId(productId);
    try {
      await wishlistService.removeFromWishlist(productId);
      setItems((prev) => prev.filter((i) => i.productId !== productId));
      toast.success('Đã xóa khỏi danh sách yêu thích');
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? 'Có lỗi xảy ra');
    } finally {
      setRemovingId(null);
    }
  };

  const handleAddToCart = async (productId: string) => {
    setAddingId(productId);
    try {
      await cartService.addToCart(productId, 1);
      refreshCartCount();
      toast.success('Đã thêm vào giỏ hàng');
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? 'Có lỗi xảy ra');
    } finally {
      setAddingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-6">Danh sách yêu thích </h1>

        {loading ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            {Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="bg-white rounded-xl shadow-sm p-3 animate-pulse">
                <div className="bg-gray-200 rounded-lg h-36 mb-3" />
                <div className="bg-gray-200 h-4 rounded mb-2 w-3/4" />
                <div className="bg-gray-200 h-4 rounded w-1/2" />
                <div className="bg-gray-200 h-8 rounded mt-3" />
              </div>
            ))}
          </div>
        ) : items.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-24 text-center">
            <svg
              className="w-16 h-16 text-gray-300 mb-4"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth={1.5}
            >
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
            </svg>
            <p className="text-gray-500 text-lg font-medium mb-2">Chưa có sản phẩm nào yêu thích</p>
            <p className="text-gray-400 text-sm mb-6">Thêm sản phẩm vào danh sách để xem lại sau</p>
            <Link
              to="/products"
              className="bg-rose-600 text-white px-6 py-2.5 rounded-lg hover:bg-rose-700 text-sm font-medium transition-colors"
            >
              Khám phá sản phẩm
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            {items.map((item) => (
              <div
                key={item.productId}
                className="bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow p-3 flex flex-col"
              >
                <div className="relative">
                  <div className="bg-gray-100 rounded-lg h-36 flex items-center justify-center overflow-hidden mb-3">
                    {item.imageUrl ? (
                      <img
                        src={getImageUrl(item.imageUrl)}
                        alt={item.productName}
                        className="h-full w-full object-contain"
                      />
                    ) : (
                      <span className="text-gray-300 text-4xl">🍔</span>
                    )}
                  </div>
                  {!item.isInStock && (
                    <span className="absolute top-2 left-2 bg-gray-600 text-white text-[10px] font-semibold px-2 py-0.5 rounded-full">
                      Het hang
                    </span>
                  )}
                  <button
                    onClick={() => handleRemove(item.productId)}
                    disabled={removingId === item.productId}
                    className="absolute top-2 right-2 bg-white rounded-full p-1 shadow text-gray-400 hover:text-red-500 disabled:opacity-50 transition-colors"
                    title="Xoa khoi yeu thich"
                  >
                    <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6l-1 14H6L5 6" />
                      <path d="M10 11v6M14 11v6" />
                      <path d="M9 6V4h6v2" />
                    </svg>
                  </button>
                </div>

                <p className="text-sm font-medium text-gray-800 line-clamp-2 flex-1">
                  {item.productName}
                </p>
                <p className="text-rose-600 font-bold text-sm mt-1">{formatPrice(item.price)}</p>

                {item.isInStock ? (
                  <button
                    onClick={() => handleAddToCart(item.productId)}
                    disabled={addingId === item.productId}
                    className="mt-2 w-full text-xs bg-rose-600 text-white rounded-lg py-1.5 hover:bg-rose-700 disabled:opacity-50 transition-colors"
                  >
                    {addingId === item.productId ? 'Đang thêm...' : '+ Thêm vào giỏ hàng'}
                  </button>
                ) : (
                  <p className="mt-2 text-xs text-center text-gray-400 py-1">Tạm hết hàng</p>
                )}
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
