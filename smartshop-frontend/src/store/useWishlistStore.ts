import { create } from 'zustand';
import { wishlistService } from '@/services/wishlistService';

interface WishlistStore {
  productIds: Set<string>;
  loading: boolean;
  loaded: boolean;
  fetch: () => Promise<void>;
  add: (productId: string) => Promise<void>;
  remove: (productId: string) => Promise<void>;
  isWishlisted: (productId: string) => boolean;
  reset: () => void;
}

export const useWishlistStore = create<WishlistStore>((set, get) => ({
  productIds: new Set(),
  loading: false,
  loaded: false,

  fetch: async () => {
    if (get().loading || get().loaded) return;
    set({ loading: true });
    try {
      const items = await wishlistService.getWishlist();
      set({ productIds: new Set(items.map((i) => i.productId)), loaded: true });
    } catch {
      // silently fail — buttons will show un-wishlisted state
    } finally {
      set({ loading: false });
    }
  },

  add: async (productId: string) => {
    set((s) => ({ productIds: new Set([...s.productIds, productId]) }));
    try {
      await wishlistService.addToWishlist(productId);
    } catch (err) {
      set((s) => {
        const next = new Set(s.productIds);
        next.delete(productId);
        return { productIds: next };
      });
      throw err;
    }
  },

  remove: async (productId: string) => {
    set((s) => {
      const next = new Set(s.productIds);
      next.delete(productId);
      return { productIds: next };
    });
    try {
      await wishlistService.removeFromWishlist(productId);
    } catch (err) {
      set((s) => ({ productIds: new Set([...s.productIds, productId]) }));
      throw err;
    }
  },

  isWishlisted: (productId: string) => get().productIds.has(productId),

  reset: () => set({ productIds: new Set(), loading: false, loaded: false }),
}));
