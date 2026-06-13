import { useState, useEffect } from 'react';
import { flashSaleService } from '../services/flashSaleService';
import type { FlashSaleItemDto } from '../types/flashSale';

export type FlashSaleMapEntry = FlashSaleItemDto & { remainingSeconds: number };

export function useFlashSaleMap() {
  // map by productId only — dùng cho product cards (hiển thị giá thấp nhất)
  const [flashSaleMap, setFlashSaleMap] = useState<Record<string, FlashSaleMapEntry>>({});
  // map by "productId:sizeId" — dùng cho cart items (match chính xác theo size)
  const [flashSaleItemMap, setFlashSaleItemMap] = useState<Record<string, FlashSaleMapEntry>>({});

  useEffect(() => {
    flashSaleService
      .getActive(1, 100)
      .then((result) => {
        const byProduct: Record<string, FlashSaleMapEntry> = {};
        const byItem: Record<string, FlashSaleMapEntry> = {};
        for (const fs of result.items) {
          for (const item of fs.items) {
            const entry: FlashSaleMapEntry = { ...item, remainingSeconds: fs.remainingSeconds };

            // by productId — giữ giá thấp nhất
            const existing = byProduct[item.productId];
            if (!existing || item.salePrice < existing.salePrice) {
              byProduct[item.productId] = entry;
            }

            // by productId:sizeId — match chính xác
            const key = `${item.productId}:${item.sizeId ?? ''}`;
            const existingItem = byItem[key];
            if (!existingItem || item.salePrice < existingItem.salePrice) {
              byItem[key] = entry;
            }
          }
        }
        setFlashSaleMap(byProduct);
        setFlashSaleItemMap(byItem);
      })
      .catch(() => {});
  }, []);

  const expireItem = (productId: string) => {
    setFlashSaleMap((prev) => {
      const n = { ...prev };
      delete n[productId];
      return n;
    });
    setFlashSaleItemMap((prev) => {
      const n = { ...prev };
      Object.keys(n).forEach((k) => {
        if (k.startsWith(`${productId}:`)) delete n[k];
      });
      return n;
    });
  };

  return { flashSaleMap, flashSaleItemMap, expireItem };
}
