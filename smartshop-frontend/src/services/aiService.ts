import api from './api';
import type { ApiResponse } from '../types/auth';
import type { ProductDto } from '../types/product';
import type { GenerateDescriptionRequest, SemanticSearchRequest, SemanticSearchResultDto } from '../types/ai';
import { getApiError } from '../utils/errorHandler';

export const aiService = {
  semanticSearch: async (request: SemanticSearchRequest): Promise<SemanticSearchResultDto[]> => {
    const { data } = await api.post<ApiResponse<SemanticSearchResultDto[]>>('/ai/search', request);
    return data.data ?? [];
  },

  getRecommendations: async (productId: string, count = 5): Promise<ProductDto[]> => {
    const { data } = await api.get<ApiResponse<ProductDto[]>>(
      `/ai/recommendations/${productId}`,
      { params: { count } }
    );
    return data.data ?? [];
  },

  generateDescription: async (request: GenerateDescriptionRequest): Promise<string> => {
    const { data } = await api.post<ApiResponse<string>>('/ai/generate-description', request);
    return data.data ?? '';
  },

  extractErrorMessage: (err: unknown, fallback: string) => getApiError(err, fallback),
};
