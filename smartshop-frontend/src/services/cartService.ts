import api from './api';
import type { CartDto } from '../types/cart';
import type { ApiResponse } from '../types/auth';

export const cartService = {
  getCart: async (): Promise<CartDto> => {
    const { data } = await api.get<ApiResponse<CartDto>>('/cart');
    return data.data;
  },

  addToCart: async (productId: string, quantity: number, sizeId?: string): Promise<CartDto> => {
    const { data } = await api.post<ApiResponse<CartDto>>('/cart/items', {
      productId,
      quantity,
      ...(sizeId ? { sizeId } : {}),
    });
    return data.data;
  },

  addComboToCart: async (comboId: string, quantity: number = 1): Promise<CartDto> => {
    const { data } = await api.post<ApiResponse<CartDto>>('/cart/combo-items', { comboId, quantity });
    return data.data;
  },

  updateItemByLineId: async (cartItemId: string, quantity: number): Promise<CartDto> => {
    const { data } = await api.put<ApiResponse<CartDto>>(`/cart/items/line/${cartItemId}`, { quantity });
    return data.data;
  },

  removeItemByLineId: async (cartItemId: string): Promise<CartDto> => {
    const { data } = await api.delete<ApiResponse<CartDto>>(`/cart/items/line/${cartItemId}`);
    return data.data;
  },

  updateItem: async (productId: string, quantity: number, sizeId?: string): Promise<CartDto> => {
    const { data } = await api.put<ApiResponse<CartDto>>(`/cart/items/${productId}`, {
      quantity,
      ...(sizeId ? { sizeId } : {}),
    });
    return data.data;
  },

  removeItem: async (productId: string, sizeId?: string): Promise<CartDto> => {
    const { data } = await api.delete<ApiResponse<CartDto>>(`/cart/items/${productId}`, {
      params: sizeId ? { sizeId } : undefined,
    });
    return data.data;
  },

  clearCart: async (): Promise<void> => {
    await api.delete('/cart');
  },

  addFromOrder: async (orderId: string): Promise<CartDto> => {
    const { data } = await api.post<ApiResponse<CartDto>>(`/cart/from-order/${orderId}`);
    return data.data;
  },
};
