import api from './api';
import type { ApiResponse } from '../types/auth';
import type { BulkImportResult, BulkActionResult, CategoryDto, CreateProductRequest, PagedResult, ProductDetailDto, ProductDto, UpdateProductRequest } from '../types/product';

export const productService = {
  getProducts: async (params: {
    page?: number;
    pageSize?: number;
    categoryId?: string;
    search?: string;
    sortBy?: number; // maps to ProductSortBy enum value on backend
    storeId?: string;
  }) => {
    const { data } = await api.get<ApiResponse<PagedResult<ProductDto>>>('/products', { params });
    return data.data;
  },

  getProductById: async (id: string) => {
    const { data } = await api.get<ApiResponse<ProductDto>>(`/products/${id}`);
    return data.data;
  },

  getProductBySlug: async (slug: string, storeId?: string) => {
    const { data } = await api.get<ApiResponse<ProductDetailDto>>(`/products/${slug}`, {
      params: storeId ? { storeId } : undefined,
    });
    return data.data;
  },

  createProduct: async (payload: CreateProductRequest) => {
    const { data } = await api.post<ApiResponse<ProductDto>>('/products', payload);
    return data.data;
  },

  updateProduct: async (id: string, payload: UpdateProductRequest) => {
    const { data } = await api.put<ApiResponse<ProductDto>>(`/products/${id}`, payload);
    return data.data;
  },

  deleteProduct: async (id: string) => {
    await api.delete(`/products/${id}`);
  },

  bulkImportProducts: async (file: File): Promise<BulkImportResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const { data } = await api.post<ApiResponse<BulkImportResult>>(
      '/products/import',
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data;
  },

  getAdminProducts: async (params: {
    page?: number;
    pageSize?: number;
    categoryId?: string;
    search?: string;
    sortBy?: string;
    isActiveFilter?: boolean;
    priceMin?: number;
    priceMax?: number;
  }): Promise<PagedResult<ProductDto>> => {
    const { data } = await api.get<ApiResponse<PagedResult<ProductDto>>>('/products/admin', { params });
    return data.data;
  },

  bulkUpdateProducts: async (productIds: string[], action: 'activate' | 'deactivate' | 'delete'): Promise<BulkActionResult> => {
    const { data } = await api.post<ApiResponse<BulkActionResult>>('/products/bulk-actions', { productIds, action });
    return data.data;
  },
};

export const categoryService = {
  getCategories: async () => {
    const { data } = await api.get<ApiResponse<CategoryDto[]>>('/categories');
    return data.data;
  },
};
