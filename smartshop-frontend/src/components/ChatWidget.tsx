import { useEffect, useRef, useState } from 'react';
import { FiMessageCircle, FiX, FiSend, FiTrash2 } from 'react-icons/fi';
import { useChatStore } from '@/store/chatStore';

export default function ChatWidget() {
  const { isOpen, messages, isLoading, toggleOpen, sendMessage, clearSession } = useChatStore();
  const [inputValue, setInputValue] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  // Focus input when panel opens
  useEffect(() => {
    if (isOpen) {
      setTimeout(() => inputRef.current?.focus(), 100);
    }
  }, [isOpen]);

  const handleSend = async () => {
    const text = inputValue.trim();
    if (!text || isLoading) return;
    setInputValue('');
    await sendMessage(text);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <>
      {/* Chat panel */}
      {isOpen && (
        <div className="fixed bottom-20 right-6 z-50 w-80 h-[460px] bg-white rounded-2xl shadow-xl flex flex-col overflow-hidden border border-gray-100">
          {/* Header */}
          <div className="flex items-center justify-between px-4 py-3 bg-rose-500 text-white shrink-0">
            <div className="flex items-center gap-2">
              <FiMessageCircle size={18} />
              <span className="font-semibold text-sm">Trợ lý FastFood</span>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={clearSession}
                className="text-rose-200 hover:text-white transition-colors"
                title="Xóa cuộc trò chuyện"
              >
                <FiTrash2 size={15} />
              </button>
              <button
                onClick={toggleOpen}
                className="text-rose-200 hover:text-white transition-colors"
                title="Đóng"
              >
                <FiX size={18} />
              </button>
            </div>
          </div>

          {/* Messages area */}
          <div className="flex-1 overflow-y-auto px-3 py-3 space-y-3">
            {messages.length === 0 && (
              <div className="flex flex-col items-center justify-center h-full text-center text-gray-400 gap-2">
                <FiMessageCircle size={32} className="text-rose-200" />
                <p className="text-sm">Xin chào! Tôi có thể giúp gì cho bạn?</p>
                <p className="text-xs text-gray-300">Hỏi về món ăn, giá cả, hoặc đặt hàng</p>
              </div>
            )}

            {messages.map((msg, idx) => (
              <div
                key={idx}
                className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-[75%] px-3 py-2 rounded-2xl text-sm leading-relaxed ${
                    msg.role === 'user'
                      ? 'bg-rose-500 text-white rounded-br-sm'
                      : 'bg-gray-100 text-gray-800 rounded-bl-sm'
                  }`}
                >
                  {msg.content}
                </div>
              </div>
            ))}

            {/* Loading indicator */}
            {isLoading && (
              <div className="flex justify-start">
                <div className="bg-gray-100 px-4 py-3 rounded-2xl rounded-bl-sm">
                  <div className="flex gap-1 items-center">
                    <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.3s]" />
                    <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.15s]" />
                    <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" />
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          {/* Input area */}
          <div className="shrink-0 border-t border-gray-100 px-3 py-2 flex items-center gap-2">
            <input
              ref={inputRef}
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={isLoading}
              placeholder="Nhập tin nhắn..."
              className="flex-1 text-sm border border-gray-200 rounded-xl px-3 py-2 outline-none focus:border-rose-400 focus:ring-1 focus:ring-rose-200 disabled:bg-gray-50 disabled:text-gray-400 transition"
            />
            <button
              onClick={handleSend}
              disabled={isLoading || !inputValue.trim()}
              className="bg-rose-500 hover:bg-rose-600 disabled:bg-rose-200 text-white rounded-xl p-2 transition-colors shrink-0"
              title="Gửi"
            >
              <FiSend size={16} />
            </button>
          </div>
        </div>
      )}

      {/* Floating toggle button */}
      <button
        onClick={toggleOpen}
        className="fixed bottom-6 right-6 z-50 w-14 h-14 bg-rose-500 hover:bg-rose-600 text-white rounded-full shadow-xl flex items-center justify-center transition-all hover:scale-105 active:scale-95"
        title={isOpen ? 'Đóng trợ lý' : 'Mở trợ lý FastFood'}
      >
        {isOpen ? <FiX size={22} /> : <FiMessageCircle size={22} />}
      </button>
    </>
  );
}
