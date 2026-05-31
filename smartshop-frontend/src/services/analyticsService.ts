import api from './api';
import type { ApiResponse } from '../types/auth';

export interface RevenueSummaryDto {
  totalRevenue: number;
  totalOrders: number;
  newCustomers: number;
  averageOrderValue: number;
  revenueGrowthPercent: number;
}

export interface RevenueByDateDto {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface TopProductDto {
  productId: string;
  productName: string;
  totalQuantity: number;
  totalRevenue: number;
}

export interface OrderStatusBreakdownDto {
  status: string;
  count: number;
}

function formatDateForApi(date: Date): string {
  return date.toISOString().slice(0, 10);
}

function formatDateForFilename(date: Date): string {
  return date.toISOString().slice(0, 10).replace(/-/g, '');
}

export const analyticsService = {
  getSummary: async (from: string, to: string): Promise<RevenueSummaryDto> => {
    const { data } = await api.get<ApiResponse<RevenueSummaryDto>>('/admin/analytics/summary', {
      params: { from, to },
    });
    return data.data;
  },

  getRevenueByDate: async (from: string, to: string): Promise<RevenueByDateDto[]> => {
    const { data } = await api.get<ApiResponse<RevenueByDateDto[]>>('/admin/analytics/revenue', {
      params: { from, to },
    });
    return data.data;
  },

  getTopProducts: async (from: string, to: string, limit = 5): Promise<TopProductDto[]> => {
    const { data } = await api.get<ApiResponse<TopProductDto[]>>('/admin/analytics/top-products', {
      params: { from, to, limit },
    });
    return data.data;
  },

  getOrderStatusBreakdown: async (): Promise<OrderStatusBreakdownDto[]> => {
    const { data } = await api.get<ApiResponse<OrderStatusBreakdownDto[]>>(
      '/admin/analytics/order-status',
    );
    return data.data;
  },

  exportCsv: async (from: Date, to: Date): Promise<void> => {
    const response = await api.get('/admin/analytics/export/csv', {
      params: {
        from: formatDateForApi(from),
        to: formatDateForApi(to),
      },
      responseType: 'blob',
    });
    const url = URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.download = `revenue_${formatDateForFilename(new Date())}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  },

  exportPdf: async (from: Date, to: Date): Promise<void> => {
    const response = await api.get('/admin/analytics/export/pdf', {
      params: {
        from: formatDateForApi(from),
        to: formatDateForApi(to),
      },
      responseType: 'blob',
    });
    const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = `revenue_${formatDateForFilename(new Date())}.pdf`;
    link.click();
    URL.revokeObjectURL(url);
  },
};
