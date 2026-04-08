import api from './api';
import type { ApiResponse } from '../types/auth';
import type { CategoryDto, CreateProductRequest, PagedResult, ProductDto, UpdateProductRequest } from '../types/product';

export const productService = {
  getProducts: async (params: {
    page?: number;
    pageSize?: number;
    categoryId?: string;
    search?: string;
    sortBy?: number; // maps to ProductSortBy enum value on backend
  }) => {
    const { data } = await api.get<ApiResponse<PagedResult<ProductDto>>>('/products', { params });
    return data.data;
  },

  getProductById: async (id: string) => {
    const { data } = await api.get<ApiResponse<ProductDto>>(`/products/${id}`);
    return data.data;
  },

  getProductBySlug: async (slug: string) => {
    const { data } = await api.get<ApiResponse<ProductDto>>(`/products/${slug}`);
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
};

export const categoryService = {
  getCategories: async () => {
    const { data } = await api.get<ApiResponse<CategoryDto[]>>('/categories');
    return data.data;
  },
};
