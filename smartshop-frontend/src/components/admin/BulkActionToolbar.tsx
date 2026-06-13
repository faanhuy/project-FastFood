import { useTranslation } from 'react-i18next';

export interface BulkAction {
  id: string;
  label: string;
  variant?: 'danger' | 'warning' | 'default';
  confirmMessage?: string;
}

interface BulkActionToolbarProps {
  selectedCount: number;
  actions: BulkAction[];
  onAction: (actionId: string) => Promise<void>;
  isLoading?: boolean;
}

export default function BulkActionToolbar({
  selectedCount,
  actions,
  onAction,
  isLoading = false,
}: BulkActionToolbarProps) {
  const { t } = useTranslation('admin');

  if (selectedCount === 0) {
    return null;
  }

  const handleActionClick = async (action: BulkAction) => {
    if (action.confirmMessage) {
      const confirmed = window.confirm(action.confirmMessage);
      if (!confirmed) return;
    }
    await onAction(action.id);
  };

  const getButtonClass = (variant?: string) => {
    const baseClass =
      'px-3 py-1.5 rounded text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed';

    switch (variant) {
      case 'danger':
        return `${baseClass} bg-rose-600 text-white hover:bg-rose-700 disabled:hover:bg-rose-600`;
      case 'warning':
        return `${baseClass} bg-yellow-600 text-white hover:bg-yellow-700 disabled:hover:bg-yellow-600`;
      default:
        return `${baseClass} bg-gray-600 text-white hover:bg-gray-700 disabled:hover:bg-gray-600`;
    }
  };

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow-lg p-4 z-30">
      <div className="max-w-7xl mx-auto flex items-center justify-between gap-4 flex-wrap">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-gray-700">
            {t('bulkSelected', { count: selectedCount })}
          </span>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          {actions.map(action => (
            <button
              key={action.id}
              onClick={() => handleActionClick(action)}
              disabled={isLoading}
              className={getButtonClass(action.variant)}
              title={action.confirmMessage}
            >
              {action.label}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
