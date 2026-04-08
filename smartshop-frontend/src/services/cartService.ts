import api from './api';
import type { CartDto } from '../types/cart';
import type { ApiResponse } from '../types/auth';

export const cartService = {
  getCart: async (): Promise<CartDto> => {
    const { data } = await api.get<ApiResponse<CartDto>>('/cart');
    return data.data;
  },

  addToCart: async (productId: string, quantity: number): Promise<CartDto> => {
    const { data } = await api.post<ApiResponse<CartDto>>('/cart/items', { productId, quantity });
    return data.data;
  },

  updateItem: async (productId: string, quantity: number): Promise<CartDto> => {
    const { data } = await api.put<ApiResponse<CartDto>>(`/cart/items/${productId}`, { quantity });
    return data.data;
  },

  removeItem: async (productId: string): Promise<CartDto> => {
    const { data } = await api.delete<ApiResponse<CartDto>>(`/cart/items/${productId}`);
    return data.data;
  },

  clearCart: async (): Promise<void> => {
    await api.delete('/cart');
  },
};
