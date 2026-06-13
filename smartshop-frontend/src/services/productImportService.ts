import api from './api';
import type { ApiResponse } from '../types/auth';

export interface CsvPreviewRow {
  rowNumber: number;
  name: string;
  price: number;
  categoryId: string;
  isValid: boolean;
}

export interface ImportRowError {
  rowNumber: number;
  field: string;
  message: string;
}

export interface CsvPreviewResult {
  totalRows: number;
  validRows: number;
  invalidRows: number;
  errors: ImportRowError[];
  preview: CsvPreviewRow[];
}

export interface BulkImportResult {
  created: number;
  failed: number;
  errors: ImportRowError[];
}

export const productImportService = {
  previewCsv: async (file: File): Promise<CsvPreviewResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const { data } = await api.post<ApiResponse<CsvPreviewResult>>('/products/import/preview', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data.data;
  },

  importCsv: async (file: File): Promise<BulkImportResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const { data } = await api.post<ApiResponse<BulkImportResult>>('/products/import/csv', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data.data;
  },
};
