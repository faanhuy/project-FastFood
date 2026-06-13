import api from './api';
import type { ApiResponse } from '../types/auth';
import type { PagedResult } from '../types/product';
import type { LoyaltyAccountDto, PointTransactionDto } from '../types/loyalty';

export const loyaltyService = {
  getAccount: async (): Promise<LoyaltyAccountDto> => {
    const { data } = await api.get<ApiResponse<LoyaltyAccountDto>>('/loyalty');
    return data.data;
  },

  getTransactions: async (page = 1, pageSize = 20): Promise<PagedResult<PointTransactionDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<PointTransactionDto>>>('/loyalty/transactions', {
      params: { page, pageSize },
    });
    return data.data;
  },
};
