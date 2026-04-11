import api from './api';
import type { ApiResponse } from '../types/auth';
import type { ReviewDto, AddReviewRequest } from '../types/review';
import type { PagedResult } from '../types/product';

export const reviewService = {
  getProductReviews: async (productId: string, page = 1, pageSize = 10): Promise<PagedResult<ReviewDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<ReviewDto>>>(
      `/reviews/product/${productId}`,
      { params: { page, pageSize } }
    );
    return data.data;
  },

  addReview: async (request: AddReviewRequest): Promise<ReviewDto> => {
    const { data } = await api.post<ApiResponse<ReviewDto>>('/reviews', request);
    return data.data;
  },

  deleteReview: async (id: string): Promise<void> => {
    await api.delete(`/reviews/${id}`);
  },
};
