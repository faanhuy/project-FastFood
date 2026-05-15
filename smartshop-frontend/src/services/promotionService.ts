import api from './api';
import type { ApiResponse } from '../types/auth';
import type { PagedResult } from '../types/product';
import type {
  CreatePriceCampaignRequest,
  PriceCampaignDto,
  PriceCampaignSummaryDto,
  ComboDto,
  ComboSummaryDto,
  CreateComboRequest,
} from '../types/promotion';

export const promotionService = {
  // Admin — Price Campaigns
  getPriceCampaigns: async (): Promise<PagedResult<PriceCampaignSummaryDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<PriceCampaignSummaryDto>>>(
      '/admin/price-campaigns',
    );
    return data.data ?? { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 };
  },

  getPriceCampaignById: async (id: string): Promise<PriceCampaignDto> => {
    const { data } = await api.get<ApiResponse<PriceCampaignDto>>(`/admin/price-campaigns/${id}`);
    return data.data!;
  },

  createPriceCampaign: async (body: CreatePriceCampaignRequest): Promise<PriceCampaignDto> => {
    const { data } = await api.post<ApiResponse<PriceCampaignDto>>('/admin/price-campaigns', body);
    return data.data!;
  },

  updatePriceCampaign: async (
    id: string,
    body: CreatePriceCampaignRequest,
  ): Promise<PriceCampaignDto> => {
    const { data } = await api.put<ApiResponse<PriceCampaignDto>>(
      `/admin/price-campaigns/${id}`,
      body,
    );
    return data.data!;
  },

  deletePriceCampaign: async (id: string): Promise<void> => {
    await api.delete(`/admin/price-campaigns/${id}`);
  },

  // Admin — Combos
  getCombos: async (page = 1, pageSize = 20): Promise<PagedResult<ComboSummaryDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<ComboSummaryDto>>>(
      `/admin/combos?page=${page}&pageSize=${pageSize}`,
    );
    return data.data ?? { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 };
  },

  getComboById: async (id: string): Promise<ComboDto> => {
    const { data } = await api.get<ApiResponse<ComboDto>>(`/admin/combos/${id}`);
    return data.data!;
  },

  createCombo: async (body: CreateComboRequest): Promise<ComboDto> => {
    const { data } = await api.post<ApiResponse<ComboDto>>('/admin/combos', body);
    return data.data!;
  },

  updateCombo: async (id: string, body: CreateComboRequest): Promise<ComboDto> => {
    const { data } = await api.put<ApiResponse<ComboDto>>(`/admin/combos/${id}`, body);
    return data.data!;
  },

  deleteCombo: async (id: string): Promise<void> => {
    await api.delete(`/admin/combos/${id}`);
  },
};
