import { create } from 'zustand';
import { flashSaleService } from '../services/flashSaleService';
import type { FlashSaleItemDto } from '../types/flashSale';

// Shared types for both maps
export type FlashSaleMapEntry = FlashSaleItemDto & {
  flashSaleName?: string; // for product list (by productId)
  remainingSeconds: number;
};

type FlashSaleProductMap = Record<string, FlashSaleMapEntry>;
// key = `${productId}:${sizeId ?? ''}` (for cart/checkout use)
type FlashSaleItemMap = Record<string, FlashSaleItemDto & { remainingSeconds: number }>;

interface FlashSaleState {
  flashSaleMap: FlashSaleProductMap;
  flashSaleItemMap: FlashSaleItemMap;
  lastFetchMs: number | null;
  isFetching: boolean;
  fetchIfStale: () => Promise<void>;
  expireItem: (productId: string, sizeId?: string) => void;
}

const CACHE_TTL_MS = 30_000;

export const useFlashSaleStore = create<FlashSaleState>((set, get) => ({
  flashSaleMap: {},
  flashSaleItemMap: {},
  lastFetchMs: null,
  isFetching: false,

  fetchIfStale: async () => {
    const { lastFetchMs, isFetching } = get();
    if (isFetching) return;
    if (lastFetchMs && Date.now() - lastFetchMs < CACHE_TTL_MS) return;

    set({ isFetching: true });
    try {
      const result = await flashSaleService.getActive(1, 100);
      const byProduct: FlashSaleProductMap = {};
      const byItem: FlashSaleItemMap = {};

      for (const fs of result.items) {
        for (const item of fs.items) {
          // byProduct: giữ giá thấp nhất theo productId, thêm flashSaleName
          const existing = byProduct[item.productId];
          if (!existing || item.salePrice < existing.salePrice) {
            byProduct[item.productId] = {
              ...item,
              flashSaleName: fs.name,
              remainingSeconds: fs.remainingSeconds,
            };
          }

          // byItem: key = productId:sizeId (dùng cho cart/checkout)
          const key = `${item.productId}:${item.sizeId ?? ''}`;
          const existingItem = byItem[key];
          if (!existingItem || item.salePrice < existingItem.salePrice) {
            byItem[key] = { ...item, remainingSeconds: fs.remainingSeconds };
          }
        }
      }

      set({
        flashSaleMap: byProduct,
        flashSaleItemMap: byItem,
        lastFetchMs: Date.now(),
      });
    } catch {
      // silent fail — pages render normally without flash sale data
    } finally {
      set({ isFetching: false });
    }
  },

  expireItem: (productId: string, sizeId?: string) => {
    set((state) => {
      const newProductMap = { ...state.flashSaleMap };
      delete newProductMap[productId];

      const newItemMap = { ...state.flashSaleItemMap };
      if (sizeId !== undefined) {
        delete newItemMap[`${productId}:${sizeId}`];
      } else {
        // xóa tất cả entries của productId
        Object.keys(newItemMap).forEach((k) => {
          if (k.startsWith(`${productId}:`)) delete newItemMap[k];
        });
      }

      return { flashSaleMap: newProductMap, flashSaleItemMap: newItemMap };
    });
  },
}));
