import React, { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';

export interface UploadProps {
  onFileSelect: (files: File[]) => void;
  accept?: string;
  multiple?: boolean;
  maxSize?: number;
  maxFiles?: number;
  preview?: boolean;
  disabled?: boolean;
  className?: string;
  label?: string;
  error?: string;
}

export const Upload: React.FC<UploadProps> = ({
  onFileSelect,
  accept = 'image/*',
  multiple = false,
  maxSize = 5 * 1024 * 1024,
  maxFiles = 1,
  preview = true,
  disabled = false,
  className = '',
  label,
  error,
}) => {
  const { t } = useTranslation('common');
  const [isDragging, setIsDragging] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [errors, setErrors] = useState<string[]>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const isImagePreview = preview && accept.includes('image');

  const validateFiles = (files: File[]): File[] => {
    const newErrors: string[] = [];
    const validFiles: File[] = [];

    files.forEach((file) => {
      // Check file type
      const isAccepted = !accept || accept === '*' || accept.split(',').some(
        (type) => {
          if (type.includes('*')) {
            const baseType = type.split('/')[0];
            return file.type.startsWith(baseType);
          }
          return file.type === type;
        }
      );

      if (!isAccepted) {
        newErrors.push(t('fileTypeInvalid'));
        return;
      }

      // Check file size
      if (file.size > maxSize) {
        newErrors.push(
          t('fileTooLarge', { size: (maxSize / (1024 * 1024)).toFixed(1) })
        );
        return;
      }

      validFiles.push(file);
    });

    // Check max files
    const totalFiles = selectedFiles.length + validFiles.length;
    if (totalFiles > maxFiles) {
      newErrors.push(
        t('filesSelected', { count: maxFiles }).replace(t('filesSelected', { count: maxFiles }), maxFiles.toString())
      );
    }

    setErrors(newErrors);
    return validFiles.slice(0, maxFiles - selectedFiles.length);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!disabled) {
      setIsDragging(true);
    }
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    if (!disabled) {
      const files = Array.from(e.dataTransfer.files);
      const validated = validateFiles(files);
      if (validated.length > 0) {
        const newFiles = multiple
          ? [...selectedFiles, ...validated]
          : validated;
        setSelectedFiles(newFiles);
        onFileSelect(newFiles);
      }
    }
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      const validated = validateFiles(files);
      if (validated.length > 0) {
        const newFiles = multiple
          ? [...selectedFiles, ...validated]
          : validated;
        setSelectedFiles(newFiles);
        onFileSelect(newFiles);
      }
    }
  };

  const handleRemoveFile = (idx: number) => {
    const newFiles = selectedFiles.filter((_, i) => i !== idx);
    setSelectedFiles(newFiles);
    onFileSelect(newFiles);
  };

  const handleBrowseClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className={className}>
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={handleBrowseClick}
        className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer ${
          disabled
            ? 'bg-gray-100 border-gray-300 text-gray-400'
            : isDragging
              ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
              : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 hover:border-blue-400'
        }`}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept={accept}
          multiple={multiple}
          onChange={handleFileInput}
          className="hidden"
          disabled={disabled}
        />

        <svg
          className={`w-12 h-12 mx-auto mb-3 ${
            disabled
              ? 'text-gray-400'
              : isDragging
                ? 'text-blue-600'
                : 'text-gray-400'
          }`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
          />
        </svg>

        <p className={`text-sm ${disabled ? 'text-gray-400' : 'text-gray-700 dark:text-gray-300'}`}>
          <span className="font-semibold">{t('dragDropHere')}</span>
          {' '}
          <span className="text-blue-600 dark:text-blue-400">{t('browseFiles')}</span>
        </p>

        {label && (
          <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
            {label}
          </p>
        )}
      </div>

      {selectedFiles.length > 0 && (
        <div className="mt-4 space-y-2">
          {selectedFiles.map((file, idx) => (
            <div
              key={idx}
              className="flex items-center gap-3 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg"
            >
              {isImagePreview && file.type.startsWith('image/') && (
                <img
                  src={URL.createObjectURL(file)}
                  alt={file.name}
                  className="w-12 h-12 object-cover rounded-md"
                />
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                  {file.name}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  {(file.size / 1024).toFixed(2)} KB
                </p>
              </div>
              <button
                onClick={() => handleRemoveFile(idx)}
                className="p-1 text-gray-400 hover:text-red-600 transition-colors"
                aria-label="Remove file"
              >
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path
                    fillRule="evenodd"
                    d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                    clipRule="evenodd"
                  />
                </svg>
              </button>
            </div>
          ))}
        </div>
      )}

      {errors.length > 0 && (
        <div className="mt-3 space-y-1">
          {errors.map((err, idx) => (
            <p key={idx} className="text-sm text-red-600 dark:text-red-400">
              {err}
            </p>
          ))}
        </div>
      )}

      {error && (
        <p className="mt-2 text-sm text-red-600 dark:text-red-400">{error}</p>
      )}
    </div>
  );
};
