import api from './api';
import type { OrderDto, OrderStatusValue, PlaceOrderRequest, BulkActionResult, OrderTimelineEventDto, AdminRefundOrderRequest, AdminPartialRefundOrderRequest } from '../types/order';
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
  getAllOrders: async (params?: {
    page?: number;
    pageSize?: number;
    statusFilter?: number;
    search?: string;
    createdAfter?: string;
    createdBefore?: string;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<PagedResult<OrderDto>> => {
    const queryParams = params || {};
    const { data } = await api.get<ApiResponse<PagedResult<OrderDto>>>('/orders/admin/all', {
      params: queryParams,
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

  bulkUpdateOrders: async (orderIds: string[], action: 'confirm' | 'ship' | 'deliver' | 'cancel'): Promise<BulkActionResult> => {
    const { data } = await api.post<ApiResponse<BulkActionResult>>('/orders/admin/bulk-actions', { orderIds, action });
    return data.data;
  },

  getOrderTimeline: async (orderId: string): Promise<OrderTimelineEventDto[]> => {
    const { data } = await api.get<ApiResponse<OrderTimelineEventDto[]>>(`/orders/${orderId}/timeline`);
    return data.data;
  },

  refundOrder: async (orderId: string, request: AdminRefundOrderRequest): Promise<OrderDto> => {
    const { data } = await api.post<ApiResponse<OrderDto>>(`/orders/admin/${orderId}/refund`, request);
    return data.data;
  },

  partialRefundOrder: async (orderId: string, request: AdminPartialRefundOrderRequest): Promise<OrderDto> => {
    const { data } = await api.post<ApiResponse<OrderDto>>(`/orders/admin/${orderId}/partial-refund`, request);
    return data.data;
  },
};
