import api from './api';
import type { ApiResponse } from '../types/auth';

interface UploadImageResponse {
  url: string; // relative: /images/products/{filename}
}

export const imageService = {
  // Legacy — giữ lại để không break code cũ
  upload: async (file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<ApiResponse<UploadImageResponse>>(
      '/images/upload',
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data.url; // relative path
  },

  // Mới — dùng IFileStorageService với ImageSharp resize
  uploadProductImage: async (file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<ApiResponse<UploadImageResponse>>(
      '/files/upload/product-image',
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data.url;
  },

  uploadComboImage: async (file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<ApiResponse<UploadImageResponse>>(
      '/files/upload/combo-image',
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data.url;
  },

  uploadAvatar: async (file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<ApiResponse<UploadImageResponse>>(
      '/files/upload/avatar',
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data.url;
  },
};
