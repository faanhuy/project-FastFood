import api from './api';
import type { ApiResponse } from '../types/auth';
import type { GetCatalogResult } from '../types/catalog';

export const catalogService = {
  getCatalog: async (page = 1, pageSize = 20): Promise<GetCatalogResult> => {
    try {
      const { data } = await api.get<ApiResponse<GetCatalogResult>>('/catalog', {
        params: { page, pageSize },
      });
      return data.data ?? { products: [], combos: [], totalProducts: 0, totalCombos: 0 };
    } catch (error) {
      console.warn('Failed to fetch catalog:', error);
      return { products: [], combos: [], totalProducts: 0, totalCombos: 0 };
    }
  },
};
