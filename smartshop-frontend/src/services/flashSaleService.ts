import api from './api';
import type { FlashSaleDto, CreateFlashSaleRequest, UpdateFlashSaleRequest } from '../types/flashSale';
import type { ApiResponse } from '../types/auth';
import type { PagedResult } from '../types/product';

export const flashSaleService = {
  getActive: async (page = 1, pageSize = 20): Promise<PagedResult<FlashSaleDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<FlashSaleDto>>>('/flash-sales', { params: { page, pageSize } });
    return data.data;
  },

  adminGetAll: async (page = 1, pageSize = 20, status?: string): Promise<PagedResult<FlashSaleDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<FlashSaleDto>>>('/admin/flash-sales', { params: { page, pageSize, status } });
    return data.data;
  },

  create: async (request: CreateFlashSaleRequest): Promise<FlashSaleDto> => {
    const { data } = await api.post<ApiResponse<FlashSaleDto>>('/admin/flash-sales', request);
    return data.data;
  },

  update: async (id: string, request: UpdateFlashSaleRequest): Promise<FlashSaleDto> => {
    const { data } = await api.put<ApiResponse<FlashSaleDto>>(`/admin/flash-sales/${id}`, request);
    return data.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/admin/flash-sales/${id}`);
  },

  submitForApproval: async (id: string): Promise<FlashSaleDto> => {
    const { data } = await api.post<ApiResponse<FlashSaleDto>>(`/admin/flash-sales/${id}/submit`);
    return data.data;
  },

  approve: async (id: string): Promise<FlashSaleDto> => {
    const { data } = await api.post<ApiResponse<FlashSaleDto>>(`/admin/flash-sales/${id}/approve`);
    return data.data;
  },

  reject: async (id: string, reason: string): Promise<FlashSaleDto> => {
    const { data } = await api.post<ApiResponse<FlashSaleDto>>(`/admin/flash-sales/${id}/reject`, { reason });
    return data.data;
  },
};
