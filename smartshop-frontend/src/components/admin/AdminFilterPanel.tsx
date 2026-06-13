import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FiChevronDown, FiChevronRight } from 'react-icons/fi';

export interface FilterOption {
  value: string;
  label: string;
}

export interface FilterField {
  key: string;
  type: 'select' | 'text' | 'number' | 'date';
  label: string;
  options?: FilterOption[];
  placeholder?: string;
}

interface AdminFilterPanelProps {
  fields: FilterField[];
  values: Record<string, string>;
  onChange: (key: string, value: string) => void;
  onApply: () => void;
  onReset: () => void;
  isLoading?: boolean;
}

export default function AdminFilterPanel({
  fields,
  values,
  onChange,
  onApply,
  onReset,
  isLoading = false,
}: AdminFilterPanelProps) {
  const { t } = useTranslation('admin');
  const [open, setOpen] = useState(false);

  // Local draft state for text/number inputs — prevents focus loss on parent re-render.
  // Select/date propagate immediately since they don't involve keyboard input.
  const [draft, setDraft] = useState<Record<string, string>>(values);
  const prevValuesRef = useRef(values);

  // Sync draft when parent resets (detects reference change to empty-ish object)
  useEffect(() => {
    if (values !== prevValuesRef.current) {
      prevValuesRef.current = values;
      setDraft(values);
    }
  }, [values]);

  const handleApply = () => {
    // Push pending text/number drafts to parent before applying
    fields.forEach(field => {
      if (field.type === 'text' || field.type === 'number') {
        const draftVal = draft[field.key] ?? '';
        const parentVal = values[field.key] ?? '';
        if (draftVal !== parentVal) onChange(field.key, draftVal);
      }
    });
    onApply();
  };

  const handleReset = () => {
    setDraft({});
    onReset();
  };

  const getDisplayValue = (field: FilterField) =>
    field.type === 'text' || field.type === 'number'
      ? (draft[field.key] ?? '')
      : (values[field.key] ?? '');

  const inputCls =
    'px-3 py-2 border border-gray-300 rounded text-sm focus:ring-2 focus:ring-rose-400 focus:border-rose-400 focus:outline-none disabled:opacity-50';

  return (
    <div className="mb-4">
      <button
        onClick={() => setOpen(v => !v)}
        className="flex items-center gap-2 px-4 py-2 rounded-lg bg-gray-100 hover:bg-gray-200 text-gray-700 text-sm font-medium transition-colors"
      >
        <span>{t('filterTitle')}</span>
        {open ? <FiChevronDown size={16} /> : <FiChevronRight size={16} />}
      </button>

      {open && (
        <div className="mt-2 p-4 bg-white border border-gray-200 rounded-lg space-y-3">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {fields.map(field => (
              <div key={field.key} className="flex flex-col gap-1">
                <label className="text-xs font-semibold text-gray-700">
                  {field.label}
                </label>

                {field.type === 'select' ? (
                  <select
                    value={getDisplayValue(field)}
                    onChange={e => onChange(field.key, e.target.value)}
                    disabled={isLoading}
                    className={inputCls}
                  >
                    <option value="">{field.placeholder || t('filterAll')}</option>
                    {field.options?.map(opt => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                ) : field.type === 'date' ? (
                  <input
                    type="date"
                    value={getDisplayValue(field)}
                    onChange={e => onChange(field.key, e.target.value)}
                    disabled={isLoading}
                    className={inputCls}
                  />
                ) : field.type === 'number' ? (
                  <input
                    type="number"
                    value={getDisplayValue(field)}
                    onChange={e => setDraft(prev => ({ ...prev, [field.key]: e.target.value }))}
                    onKeyDown={e => { if (e.key === 'Enter') handleApply(); }}
                    placeholder={field.placeholder}
                    disabled={isLoading}
                    className={inputCls}
                  />
                ) : (
                  <input
                    type="text"
                    value={getDisplayValue(field)}
                    onChange={e => setDraft(prev => ({ ...prev, [field.key]: e.target.value }))}
                    onKeyDown={e => { if (e.key === 'Enter') handleApply(); }}
                    placeholder={field.placeholder}
                    disabled={isLoading}
                    className={inputCls}
                  />
                )}
              </div>
            ))}
          </div>

          <div className="flex gap-2 pt-2 border-t">
            <button
              onClick={handleApply}
              disabled={isLoading}
              className="px-4 py-2 bg-rose-600 text-white rounded text-sm font-medium hover:bg-rose-700 transition-colors disabled:opacity-50"
            >
              {t('filterApply')}
            </button>
            <button
              onClick={handleReset}
              disabled={isLoading}
              className="px-4 py-2 bg-gray-300 text-gray-700 rounded text-sm font-medium hover:bg-gray-400 transition-colors disabled:opacity-50"
            >
              {t('filterReset')}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
