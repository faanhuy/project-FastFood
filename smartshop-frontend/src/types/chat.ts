export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  createdAt?: string;
}

export interface SendMessageResponse {
  reply: string;
  sessionId: string;
  sources: string[];
}
