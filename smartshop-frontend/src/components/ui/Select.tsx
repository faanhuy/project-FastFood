import React, { useState, useRef, useEffect, useMemo } from 'react';
import ReactDOM from 'react-dom';
import { useTranslation } from 'react-i18next';

export interface SelectOption<T = string> {
  value: T;
  label: string;
  disabled?: boolean;
}

export interface SelectProps<T = string> {
  options: SelectOption<T>[];
  value: T | null;
  onChange: (value: T | null) => void;
  placeholder?: string;
  searchable?: boolean;
  clearable?: boolean;
  loading?: boolean;
  disabled?: boolean;
  className?: string;
  error?: string;
}

export const Select = React.forwardRef<
  HTMLDivElement,
  SelectProps
>(
  (
    {
      options,
      value,
      onChange,
      placeholder,
      searchable = false,
      clearable = false,
      loading = false,
      disabled = false,
      className = '',
      error,
    },
    ref
  ) => {
    const { t } = useTranslation('common');
    const [isOpen, setIsOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [highlightedIdx, setHighlightedIdx] = useState(-1);
    const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({});
    const containerRef = useRef<HTMLDivElement>(null);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const selectedOption = options.find((opt) => opt.value === value);
    const displayLabel = placeholder || t('selectPlaceholder');

    const filteredOptions = useMemo(() => {
      if (!searchable || !searchQuery) return options;
      return options.filter((opt) =>
        opt.label.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }, [options, searchable, searchQuery]);

    const updateDropdownPosition = () => {
      if (containerRef.current) {
        const rect = containerRef.current.getBoundingClientRect();
        setDropdownStyle({
          position: 'fixed',
          top: rect.bottom + 4,
          left: rect.left,
          width: rect.width,
          zIndex: 9999,
        });
      }
    };

    const handleOpen = () => {
      if (disabled) return;
      if (!isOpen) {
        updateDropdownPosition();
        setIsOpen(true);
      } else {
        setIsOpen(false);
      }
    };

    useEffect(() => {
      if (!isOpen) return;

      const handleClickOutside = (e: MouseEvent) => {
        const target = e.target as Node;
        const inContainer = containerRef.current?.contains(target);
        const inDropdown = dropdownRef.current?.contains(target);
        if (!inContainer && !inDropdown) {
          setIsOpen(false);
        }
      };

      const handleScrollOrResize = () => setIsOpen(false);

      document.addEventListener('mousedown', handleClickOutside);
      window.addEventListener('scroll', handleScrollOrResize, true);
      window.addEventListener('resize', handleScrollOrResize);

      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
        window.removeEventListener('scroll', handleScrollOrResize, true);
        window.removeEventListener('resize', handleScrollOrResize);
      };
    }, [isOpen]);

    const handleKeyDown = (e: React.KeyboardEvent) => {
      if (!isOpen) {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          handleOpen();
        }
        return;
      }

      switch (e.key) {
        case 'ArrowDown':
          e.preventDefault();
          setHighlightedIdx((prev) =>
            prev < filteredOptions.length - 1 ? prev + 1 : 0
          );
          break;
        case 'ArrowUp':
          e.preventDefault();
          setHighlightedIdx((prev) =>
            prev > 0 ? prev - 1 : filteredOptions.length - 1
          );
          break;
        case 'Enter':
          e.preventDefault();
          if (highlightedIdx >= 0 && filteredOptions[highlightedIdx]) {
            const option = filteredOptions[highlightedIdx];
            if (!option.disabled) {
              onChange(option.value);
              setIsOpen(false);
              setSearchQuery('');
              setHighlightedIdx(-1);
            }
          }
          break;
        case 'Escape':
          e.preventDefault();
          setIsOpen(false);
          setSearchQuery('');
          setHighlightedIdx(-1);
          break;
        default:
          break;
      }
    };

    const handleSelect = (option: SelectOption) => {
      if (!option.disabled) {
        onChange(option.value);
        setIsOpen(false);
        setSearchQuery('');
        setHighlightedIdx(-1);
      }
    };

    const handleClear = (e: React.MouseEvent) => {
      e.stopPropagation();
      onChange(null);
    };

    const dropdown = isOpen ? (
      <div
        ref={dropdownRef}
        style={dropdownStyle}
        className="bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto"
      >
        {searchable && (
          <div className="p-2 border-b border-gray-200 dark:border-gray-700 sticky top-0 bg-white dark:bg-gray-800">
            <input
              type="text"
              placeholder={t('search')}
              value={searchQuery}
              onChange={(e) => {
                setSearchQuery(e.target.value);
                setHighlightedIdx(-1);
              }}
              className="w-full px-3 py-1.5 border border-gray-300 dark:border-gray-600 rounded-md text-sm dark:bg-gray-700 dark:text-white dark:placeholder-gray-400 focus:outline-none focus:border-blue-500"
              autoFocus
            />
          </div>
        )}

        {loading ? (
          <div className="flex items-center justify-center p-4">
            <svg className="w-5 h-5 animate-spin text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </div>
        ) : filteredOptions.length === 0 ? (
          <div className="p-3 text-center text-gray-500 dark:text-gray-400 text-sm">
            {t('noOptions')}
          </div>
        ) : (
          <ul className="py-1">
            {filteredOptions.map((option, idx) => (
              <li key={idx}>
                <button
                  type="button"
                  onClick={() => handleSelect(option)}
                  disabled={option.disabled}
                  className={`w-full text-left px-4 py-2 text-sm transition-colors ${
                    highlightedIdx === idx
                      ? 'bg-blue-50 dark:bg-blue-900 text-blue-700 dark:text-blue-200'
                      : 'text-gray-900 dark:text-white hover:bg-gray-50 dark:hover:bg-gray-700'
                  } ${
                    option.disabled
                      ? 'opacity-50 cursor-not-allowed'
                      : 'cursor-pointer'
                  }`}
                >
                  {option.label}
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    ) : null;

    return (
      <div ref={ref} className={`w-full ${className}`}>
        <div
          ref={containerRef}
          className="relative"
          onKeyDown={handleKeyDown}
          tabIndex={disabled ? -1 : 0}
        >
          <button
            type="button"
            onClick={handleOpen}
            disabled={disabled}
            className={`w-full px-4 py-2 text-left border rounded-lg transition-colors flex items-center justify-between ${
              disabled
                ? 'bg-gray-100 text-gray-500 cursor-not-allowed'
                : isOpen
                  ? 'border-blue-500 bg-white dark:bg-gray-800'
                  : 'border-gray-300 bg-white dark:bg-gray-800 dark:border-gray-600 hover:border-gray-400'
            } ${error ? 'border-red-500' : ''} dark:text-white`}
          >
            <span className="truncate">
              {selectedOption ? selectedOption.label : displayLabel}
            </span>
            <div className="flex items-center gap-1">
              {clearable && selectedOption && !disabled && (
                <button
                  type="button"
                  onClick={handleClear}
                  className="p-0.5 hover:bg-gray-200 dark:hover:bg-gray-700 rounded text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                >
                  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path
                      fillRule="evenodd"
                      d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                      clipRule="evenodd"
                    />
                  </svg>
                </button>
              )}
              <svg
                className={`w-5 h-5 text-gray-400 transition-transform ${
                  isOpen ? 'transform rotate-180' : ''
                }`}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M19 14l-7 7m0 0l-7-7m7 7V3"
                />
              </svg>
            </div>
          </button>

          {ReactDOM.createPortal(dropdown, document.body)}
        </div>

        {error && (
          <p className="mt-1 text-sm text-red-600 dark:text-red-400">{error}</p>
        )}
      </div>
    );
  }
);

Select.displayName = 'Select';
