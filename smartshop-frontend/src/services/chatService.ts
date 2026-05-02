import api from '@/services/api';
import type { ApiResponse } from '@/types/auth';
import type { ChatMessage, SendMessageResponse } from '@/types/chat';

const chatService = {
  sendMessage: async (message: string, sessionId?: string): Promise<SendMessageResponse> => {
    const response = await api.post<ApiResponse<SendMessageResponse>>('/ai/chat', { message, sessionId });
    return response.data.data!;
  },

  getChatHistory: async (sessionId: string): Promise<ChatMessage[]> => {
    const response = await api.get<ApiResponse<ChatMessage[]>>(`/ai/chat/${sessionId}`);
    return response.data.data ?? [];
  },
};

export default chatService;
