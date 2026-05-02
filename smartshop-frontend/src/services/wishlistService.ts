import api from '@/services/api';
import type { WishlistItem } from '@/types/wishlist';
import type { ApiResponse } from '@/types/auth';

export const wishlistService = {
  getWishlist: async (): Promise<WishlistItem[]> => {
    const { data } = await api.get<ApiResponse<WishlistItem[]>>('/wishlist');
    return data.data ?? [];
  },

  addToWishlist: async (productId: string): Promise<void> => {
    await api.post('/wishlist', { productId });
  },

  removeFromWishlist: async (productId: string): Promise<void> => {
    await api.delete(`/wishlist/${productId}`);
  },
};
