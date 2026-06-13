import { useState, useEffect } from 'react';
import { Link, Navigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/store/authStore';
import { useStoreSelectionStore } from '@/store/useStoreSelectionStore';
import { wishlistService } from '@/services/wishlistService';
import { cartService } from '@/services/cartService';
import { flashSaleService } from '@/services/flashSaleService';
import type { WishlistItem } from '@/types/wishlist';
import type { FlashSaleItemDto } from '@/types/flashSale';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import { FlashSaleBadge } from '@/components/FlashSaleBadge';
import { getImageUrl } from '../utils/imageUrl';
import { formatPrice } from '../utils/formatters';

export default function WishlistPage() {
  const { t } = useTranslation('product');
  const { t: tToast } = useTranslation('toast');
  const { isAuthenticated, refreshCartCount } = useAuthStore();
  const { selectedStore } = useStoreSelectionStore();
  const [items, setItems] = useState<WishlistItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [removingId, setRemovingId] = useState<string | null>(null);
  const [addingId, setAddingId] = useState<string | null>(null);
  const [flashSaleMap, setFlashSaleMap] = useState<
    Record<string, FlashSaleItemDto & { remainingSeconds: number }>
  >({});

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  useEffect(() => {
    flashSaleService
      .getActive(1, 100)
      .then((result) => {
        const map: Record<string, FlashSaleItemDto & { remainingSeconds: number }> = {};
        for (const fs of result.items) {
          for (const item of fs.items) {
            const existing = map[item.productId];
            if (!existing || item.salePrice < existing.salePrice) {
              map[item.productId] = { ...item, remainingSeconds: fs.remainingSeconds };
            }
          }
        }
        setFlashSaleMap(map);
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    wishlistService
      .getWishlist(selectedStore?.id)
      .then(setItems)
      .catch(() => toast.error(tToast('wishlistLoadFailed')))
      .finally(() => setLoading(false));
  }, [tToast, selectedStore?.id]);

  const handleRemove = async (productId: string) => {
    setRemovingId(productId);
    try {
      await wishlistService.removeFromWishlist(productId);
      setItems((prev) => prev.filter((i) => i.productId !== productId));
      toast.success(tToast('removeFromWishlistSuccess'));
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? tToast('genericError'));
    } finally {
      setRemovingId(null);
    }
  };

  const handleAddToCart = async (e: React.MouseEvent, item: WishlistItem) => {
    e.preventDefault();
    setAddingId(item.productId);
    try {
      await cartService.addToCart(item.productId, 1);
      refreshCartCount();
      toast.success(tToast('addToCartSuccess'));
    } catch (error: any) {
      toast.error(error.response?.data?.message ?? tToast('genericError'));
    } finally {
      setAddingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-6">{t('wishlist')}</h1>

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
            <p className="text-gray-500 text-lg font-medium mb-2">{t('emptyWishlist')}</p>
            <p className="text-gray-400 text-sm mb-6">{t('emptyWishlistSub')}</p>
            <Link
              to="/products"
              className="bg-rose-600 text-white px-6 py-2.5 rounded-lg hover:bg-rose-700 text-sm font-medium transition-colors"
            >
              {t('exploreProducts')}
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            {items.map((item) => {
              const flashSale = flashSaleMap[item.productId];
              const displayPrice = flashSale
                ? flashSale.salePrice
                : (item.effectivePrice ?? item.price);
              const hasDiscount = displayPrice < item.price;

              return (
                <Link
                  key={item.productId}
                  to={`/products/${item.slug}`}
                  className={`relative bg-white rounded-xl shadow-sm hover:shadow-lg hover:-translate-y-1 hover:border-rose-200 border border-transparent transition-all duration-200 p-3 flex flex-col group cursor-pointer ${
                    !item.isInStock ? 'opacity-75' : ''
                  } ${flashSale ? 'border-orange-200 ring-1 ring-orange-200' : ''}`}
                >
                  {/* Remove button */}
                  <button
                    onClick={(e) => { e.preventDefault(); handleRemove(item.productId); }}
                    disabled={removingId === item.productId}
                    className="absolute top-2 right-2 z-10 bg-white rounded-full p-1 shadow text-gray-400 hover:text-red-500 disabled:opacity-50 transition-colors"
                    title={t('removeFromWishlist')}
                  >
                    <svg className="w-4 h-4" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
                    </svg>
                  </button>

                  {!item.isInStock && (
                    <span className="absolute left-2 top-2 rounded-full bg-gray-900/80 px-2 py-0.5 text-[11px] font-medium text-white z-10">
                      {t('outOfStock')}
                    </span>
                  )}

                  {/* Image */}
                  <div className="bg-gray-100 rounded-lg h-36 flex items-center justify-center mb-3 overflow-hidden">
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

                  {/* Name */}
                  <p className="text-sm font-medium text-gray-800 line-clamp-2 flex-1">
                    {item.productName}
                  </p>

                  {/* Price */}
                  <div className="mt-2">
                    {flashSale && (
                      <div className="mb-1.5">
                        <FlashSaleBadge
                          item={flashSale}
                          onExpire={() =>
                            setFlashSaleMap((prev) => {
                              const n = { ...prev };
                              delete n[item.productId];
                              return n;
                            })
                          }
                        />
                      </div>
                    )}
                    <div className="flex items-center gap-1.5 flex-wrap">
                      <span className="text-rose-600 font-bold text-sm">
                        {formatPrice(displayPrice)}
                      </span>
                      {hasDiscount && (
                        <span className="rounded-full bg-red-100 text-red-600 px-1.5 py-0.5 text-[10px] font-bold">
                          -{Math.round((1 - displayPrice / item.price) * 100)}%
                        </span>
                      )}
                    </div>
                    {hasDiscount && (
                      <p className="text-gray-400 text-xs line-through">{formatPrice(item.price)}</p>
                    )}
                  </div>

                  {/* Add to cart / out of stock */}
                  {item.isInStock ? (
                    <button
                      onClick={(e) => handleAddToCart(e, item)}
                      disabled={addingId === item.productId}
                      className="mt-2 w-full text-xs bg-rose-600 text-white rounded-lg py-1.5 hover:bg-rose-700 disabled:opacity-50 transition-colors"
                    >
                      {addingId === item.productId ? t('adding') : `+ ${t('addToCart')}`}
                    </button>
                  ) : (
                    <p className="mt-2 text-xs text-center text-gray-400 py-1">
                      {t('temporarilyOutOfStock')}
                    </p>
                  )}
                </Link>
              );
            })}
          </div>
        )}
      </main>
      <Footer />
    </div>
  );
}
