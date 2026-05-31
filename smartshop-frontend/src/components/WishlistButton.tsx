import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { useAuthStore } from '@/store/authStore';
import { useWishlistStore } from '@/store/useWishlistStore';

interface WishlistButtonProps {
  productId: string;
  className?: string;
}

export default function WishlistButton({ productId, className = '' }: WishlistButtonProps) {
  const { t: tToast } = useTranslation('toast');
  const { isAuthenticated } = useAuthStore();
  const navigate = useNavigate();
  const { fetch, add, remove, reset, productIds, loading } = useWishlistStore();

  useEffect(() => {
    if (!isAuthenticated) {
      reset();
      return;
    }
    fetch();
  }, [isAuthenticated, fetch, reset]);

  const isInWishlist = productIds.has(productId);

  const handleToggle = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (!isAuthenticated) {
      toast.error(tToast('loginRequired'));
      navigate('/login');
      return;
    }

    try {
      if (isInWishlist) {
        await remove(productId);
        toast.success(tToast('removeFromWishlistSuccess'));
      } else {
        await add(productId);
        toast.success(tToast('addToWishlistSuccess'));
      }
    } catch {
      toast.error(tToast('genericError'));
    }
  };

  const { t } = useTranslation('common');

  return (
    <button
      onClick={handleToggle}
      disabled={loading}
      title={isInWishlist ? t('removeFromWishlist') : t('addToWishlist')}
      className={`p-1.5 rounded-full transition-colors disabled:opacity-50 ${
        isInWishlist
          ? 'text-rose-600 hover:text-rose-700'
          : 'text-gray-400 hover:text-rose-500'
      } ${className}`}
    >
      {isInWishlist ? (
        <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
          <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
        </svg>
      ) : (
        <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
          <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
        </svg>
      )}
    </button>
  );
}
