import api from './api';
import type { ApiResponse } from '../types/auth';
import type { ComboDto } from '../types/promotion';

export const comboService = {
  getComboById: async (id: string): Promise<ComboDto> => {
    const { data } = await api.get<ApiResponse<ComboDto>>(`/combos/${id}`);
    return data.data!;
  },
};
