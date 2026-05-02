import { create } from 'zustand';
import toast from 'react-hot-toast';
import chatService from '@/services/chatService';
import type { ChatMessage } from '@/types/chat';

interface ChatStore {
  sessionId: string | null;
  messages: ChatMessage[];
  isOpen: boolean;
  isLoading: boolean;

  toggleOpen: () => void;
  sendMessage: (message: string) => Promise<void>;
  clearSession: () => void;
}

export const useChatStore = create<ChatStore>((set, get) => ({
  sessionId: null,
  messages: [],
  isOpen: false,
  isLoading: false,

  toggleOpen: () => set((state) => ({ isOpen: !state.isOpen })),

  sendMessage: async (message: string) => {
    const userMessage: ChatMessage = {
      role: 'user',
      content: message,
      createdAt: new Date().toISOString(),
    };

    set((state) => ({
      messages: [...state.messages, userMessage],
      isLoading: true,
    }));

    try {
      const { sessionId } = get();
      const response = await chatService.sendMessage(message, sessionId ?? undefined);

      const assistantMessage: ChatMessage = {
        role: 'assistant',
        content: response.reply,
        createdAt: new Date().toISOString(),
      };

      set((state) => ({
        sessionId: response.sessionId,
        messages: [...state.messages, assistantMessage],
        isLoading: false,
      }));
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      toast.error(axiosError.response?.data?.message ?? 'Có lỗi xảy ra');
      set({ isLoading: false });
    }
  },

  clearSession: () => set({ sessionId: null, messages: [], isLoading: false }),
}));
