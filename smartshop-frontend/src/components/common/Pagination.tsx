import { useTranslation } from 'react-i18next';

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  disabled?: boolean;
}

export default function Pagination({ page, totalPages, onPageChange, disabled }: PaginationProps) {
  const { t } = useTranslation('common');

  if (totalPages <= 1) return null;
  return (
    <div className="mt-4 flex justify-center gap-2">
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={disabled || page <= 1}
        className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
      >
        {t('prev')}
      </button>
      <span className="px-3 py-1.5 text-sm text-gray-500">{page} / {totalPages}</span>
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={disabled || page >= totalPages}
        className="px-3 py-1.5 rounded border text-sm disabled:opacity-40 hover:bg-gray-100"
      >
        {t('next')}
      </button>
    </div>
  );
}
