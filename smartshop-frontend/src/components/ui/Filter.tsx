import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Select } from './Select';
import type { SelectOption } from './Select';

export interface FilterField {
  key: string;
  label: string;
  type: 'text' | 'select' | 'date-range' | 'number-range';
  options?: SelectOption[];
  placeholder?: string;
}

export interface FilterProps {
  fields: FilterField[];
  values: Record<string, unknown>;
  onChange: (values: Record<string, unknown>) => void;
  onReset?: () => void;
  onSearch?: () => void;
  loading?: boolean;
  collapsible?: boolean;
  className?: string;
}

function countActiveFilters(fields: FilterField[], values: Record<string, unknown>): number {
  let count = 0;
  for (const field of fields) {
    if (field.type === 'date-range') {
      if (values[`${field.key}_from`] || values[`${field.key}_to`]) count++;
    } else if (field.type === 'number-range') {
      if (values[`${field.key}_min`] || values[`${field.key}_max`]) count++;
    } else {
      if (values[field.key]) count++;
    }
  }
  return count;
}

export const Filter: React.FC<FilterProps> = ({
  fields,
  values,
  onChange,
  onReset,
  onSearch,
  loading = false,
  collapsible = false,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const [localValues, setLocalValues] = useState<Record<string, unknown>>(values);
  const [open, setOpen] = useState(!collapsible);

  const activeCount = collapsible ? countActiveFilters(fields, localValues) : 0;

  const handleFieldChange = (key: string, value: unknown) => {
    const newValues = { ...localValues, [key]: value };
    setLocalValues(newValues);
    onChange(newValues);
  };

  const handleReset = () => {
    const resetValues: Record<string, unknown> = {};
    fields.forEach((field) => {
      if (field.type === 'date-range') {
        resetValues[`${field.key}_from`] = '';
        resetValues[`${field.key}_to`] = '';
      } else if (field.type === 'number-range') {
        resetValues[`${field.key}_min`] = '';
        resetValues[`${field.key}_max`] = '';
      } else {
        resetValues[field.key] = '';
      }
    });
    setLocalValues(resetValues);
    onChange(resetValues);
    onReset?.();
  };

  const renderField = (field: FilterField) => {
    switch (field.type) {
      case 'text':
        return (
          <input
            type="text"
            placeholder={field.placeholder || t('search')}
            value={(localValues[field.key] as string) || ''}
            onChange={(e) => handleFieldChange(field.key, e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm dark:bg-gray-800 dark:text-white focus:outline-none focus:border-blue-500"
          />
        );

      case 'select':
        return (
          <Select
            options={field.options || []}
            value={(localValues[field.key] as string) || null}
            onChange={(value) => handleFieldChange(field.key, value)}
            placeholder={field.placeholder}
          />
        );

      case 'date-range':
        return (
          <div className="flex gap-2">
            <input
              type="date"
              value={(localValues[`${field.key}_from`] as string) || ''}
              onChange={(e) => handleFieldChange(`${field.key}_from`, e.target.value)}
              className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm dark:bg-gray-800 dark:text-white focus:outline-none focus:border-blue-500"
            />
            <span className="flex items-center text-gray-500 px-1">-</span>
            <input
              type="date"
              value={(localValues[`${field.key}_to`] as string) || ''}
              onChange={(e) => handleFieldChange(`${field.key}_to`, e.target.value)}
              className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm dark:bg-gray-800 dark:text-white focus:outline-none focus:border-blue-500"
            />
          </div>
        );

      case 'number-range':
        return (
          <div className="flex gap-2">
            <input
              type="number"
              placeholder="Min"
              value={(localValues[`${field.key}_min`] as string | number) || ''}
              onChange={(e) => handleFieldChange(`${field.key}_min`, e.target.value)}
              className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm dark:bg-gray-800 dark:text-white focus:outline-none focus:border-blue-500"
            />
            <span className="flex items-center text-gray-500 px-1">-</span>
            <input
              type="number"
              placeholder="Max"
              value={(localValues[`${field.key}_max`] as string | number) || ''}
              onChange={(e) => handleFieldChange(`${field.key}_max`, e.target.value)}
              className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm dark:bg-gray-800 dark:text-white focus:outline-none focus:border-blue-500"
            />
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className={`bg-white dark:bg-gray-900 rounded-lg ${className}`}>
      {collapsible && (
        <div className="px-4 py-3">
          <button
            onClick={() => setOpen((o) => !o)}
            className="flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-200 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg transition-colors"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2a1 1 0 01-.293.707L13 13.414V19a1 1 0 01-.553.894l-4 2A1 1 0 017 21v-7.586L3.293 6.707A1 1 0 013 6V4z" />
            </svg>
            {t('filter')}
            {activeCount > 0 && (
              <span className="inline-flex items-center justify-center w-5 h-5 text-xs font-bold text-white bg-blue-600 rounded-full">
                {activeCount}
              </span>
            )}
            <svg
              className={`w-4 h-4 ml-1 transition-transform duration-200 ${open ? 'rotate-180' : ''}`}
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>
        </div>
      )}

      {open && (
        <div className={`space-y-4 ${collapsible ? 'px-4 pb-4 border-t border-gray-100 dark:border-gray-800 pt-4' : 'p-4'}`}>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {fields.map((field) => (
              <div
                key={field.key}
                className={
                  field.type === 'number-range' || field.type === 'date-range'
                    ? 'sm:col-span-2'
                    : ''
                }
              >
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  {field.label}
                </label>
                {renderField(field)}
              </div>
            ))}
          </div>

          <div className="flex gap-2 justify-end pt-2 border-t border-gray-200 dark:border-gray-700">
            {onReset && (
              <button
                onClick={handleReset}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {t('reset')}
              </button>
            )}
            {onSearch && (
              <button
                onClick={onSearch}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
              >
                {loading && (
                  <svg className="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                      d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                )}
                {t('search')}
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
