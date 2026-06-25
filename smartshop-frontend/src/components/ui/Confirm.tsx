import React from 'react';
import { useTranslation } from 'react-i18next';
import { Modal } from './Modal';

export interface ConfirmProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'danger' | 'warning' | 'info';
  loading?: boolean;
}

export const Confirm: React.FC<ConfirmProps> = ({
  isOpen,
  onConfirm,
  onCancel,
  title,
  message,
  confirmLabel,
  cancelLabel,
  variant = 'info',
  loading = false,
}) => {
  const { t } = useTranslation('common');

  const confirmButtonClass = {
    danger: 'bg-red-600 hover:bg-red-700 text-white',
    warning: 'bg-amber-600 hover:bg-amber-700 text-white',
    info: 'bg-blue-600 hover:bg-blue-700 text-white',
  };

  const iconClass = {
    danger: 'text-red-600',
    warning: 'text-amber-600',
    info: 'text-blue-600',
  };

  const getIcon = () => {
    switch (variant) {
      case 'danger':
        return (
          <svg
            className={`w-12 h-12 ${iconClass[variant]}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 9v2m0 4v2m0 5a9 9 0 110-18 9 9 0 010 18z"
            />
          </svg>
        );
      case 'warning':
        return (
          <svg
            className={`w-12 h-12 ${iconClass[variant]}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        );
      default:
        return (
          <svg
            className={`w-12 h-12 ${iconClass[variant]}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        );
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onCancel}
      title={title}
      size="sm"
      closeOnEsc={!loading}
      closeOnOverlay={!loading}
    >
      <div className="flex flex-col items-center gap-4">
        <div className="flex justify-center">{getIcon()}</div>
        <p className="text-center text-gray-700 dark:text-gray-300">
          {message}
        </p>
      </div>

      <div className="flex gap-3 justify-center mt-6">
        <button
          onClick={onCancel}
          disabled={loading}
          className="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {cancelLabel || t('cancel')}
        </button>
        <button
          onClick={onConfirm}
          disabled={loading}
          className={`px-4 py-2 ${confirmButtonClass[variant]} rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed`}
        >
          {loading ? t('processing') : confirmLabel || t('confirm')}
        </button>
      </div>
    </Modal>
  );
};
