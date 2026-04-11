import api from './api';
import type { ApiResponse } from '../types/auth';

interface UploadImageResponse {
  url: string; // relative: /images/products/{filename}
}

export const imageService = {
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
};
