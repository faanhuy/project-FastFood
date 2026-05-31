import api from './api';
import type { ProductImageDto } from '../types/product';
import type { ApiResponse } from '../types/auth';

export const productImageService = {
  getImages: async (productId: string): Promise<ProductImageDto[]> => {
    const { data } = await api.get<ApiResponse<ProductImageDto[]>>(`/products/${productId}/images`);
    return data.data;
  },

  addImage: async (
    productId: string,
    imageUrl: string,
    isPrimary = false,
    sortOrder = 0
  ): Promise<ProductImageDto> => {
    const { data } = await api.post<ApiResponse<ProductImageDto>>(
      `/admin/products/${productId}/images`,
      {
        imageUrl,
        isPrimary,
        sortOrder,
      }
    );
    return data.data;
  },

  deleteImage: async (productId: string, imageId: string): Promise<void> => {
    await api.delete(`/admin/products/${productId}/images/${imageId}`);
  },

  setPrimary: async (productId: string, imageId: string): Promise<void> => {
    await api.patch(`/admin/products/${productId}/images/${imageId}/primary`);
  },
};
