import api from './api';
import type { OrderDto, OrderStatusValue, PlaceOrderRequest } from '../types/order';
import type { ApiResponse } from '../types/auth';
import type { PagedResult } from '../types/product';

export const orderService = {
  placeOrder: async (request: PlaceOrderRequest): Promise<OrderDto> => {
    const { data } = await api.post<ApiResponse<OrderDto>>('/orders', request);
    return data.data;
  },

  getMyOrders: async (page = 1, pageSize = 10): Promise<PagedResult<OrderDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<OrderDto>>>('/orders', {
      params: { page, pageSize },
    });
    return data.data;
  },

  getOrderById: async (id: string): Promise<OrderDto> => {
    const { data } = await api.get<ApiResponse<OrderDto>>(`/orders/${id}`);
    return data.data;
  },

  // Admin
  getAllOrders: async (page = 1, pageSize = 20, status?: number): Promise<PagedResult<OrderDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<OrderDto>>>('/orders/admin/all', {
      params: { page, pageSize, ...(status ? { status } : {}) },
    });
    return data.data;
  },

  updateOrderStatus: async (id: string, status: OrderStatusValue): Promise<OrderDto> => {
    const { data } = await api.patch<ApiResponse<OrderDto>>(`/orders/${id}/status`, { status });
    return data.data;
  },

  cancelOrder: async (id: string): Promise<void> => {
    await api.post(`/orders/${id}/cancel`);
  },
};
