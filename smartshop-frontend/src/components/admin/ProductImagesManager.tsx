import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { FiTrash2, FiStar, FiUploadCloud, FiX } from 'react-icons/fi';
import { productImageService } from '../../services/productImageService';
import { imageService } from '../../services/imageService';
import { getApiError } from '../../utils/errorHandler';
import { getImageUrl } from '../../utils/imageUrl';
import type { ProductImageDto } from '../../types/product';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
const MAX_SIZE_MB = 5;

interface PendingFile {
  id: string;
  file: File;
  preview: string;
  status: 'pending' | 'uploading' | 'done' | 'error';
  error?: string;
}

interface ProductImagesManagerProps {
  productId: string;
}

export default function ProductImagesManager({ productId }: ProductImagesManagerProps) {
  const { t } = useTranslation(['product', 'common', 'toast']);
  const { t: tToast } = useTranslation('toast');
  const { t: tAdmin } = useTranslation('admin');
  const inputRef = useRef<HTMLInputElement>(null);

  const [images, setImages] = useState<ProductImageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pendingFiles, setPendingFiles] = useState<PendingFile[]>([]);
  const [uploading, setUploading] = useState(false);
  const [deleting, setDeleting] = useState<string | null>(null);
  const [settingPrimary, setSettingPrimary] = useState<string | null>(null);

  useEffect(() => {
    loadImages();
  }, [productId]);

  const loadImages = async () => {
    setLoading(true);
    try {
      const data = await productImageService.getImages(productId);
      setImages(data.sort((a, b) => a.sortOrder - b.sortOrder));
    } catch {
      /* ignore */
    } finally {
      setLoading(false);
    }
  };

  const validateFile = (file: File): string | null => {
    if (!ALLOWED_TYPES.includes(file.type)) return tToast('imageFormatInvalid');
    if (file.size > MAX_SIZE_MB * 1024 * 1024) return tToast('imageSizeExceeded', { maxMb: MAX_SIZE_MB });
    return null;
  };

  const addFiles = (files: File[]) => {
    const newPending: PendingFile[] = files.map((file) => {
      const error = validateFile(file);
      return {
        id: `${Date.now()}-${Math.random()}`,
        file,
        preview: URL.createObjectURL(file),
        status: error ? 'error' : 'pending',
        error: error ?? undefined,
      };
    });
    setPendingFiles((prev) => [...prev, ...newPending]);
  };

  const removePending = (id: string) => {
    setPendingFiles((prev) => {
      const item = prev.find((f) => f.id === id);
      if (item) URL.revokeObjectURL(item.preview);
      return prev.filter((f) => f.id !== id);
    });
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const files = Array.from(e.dataTransfer.files).filter((f) => f.type.startsWith('image/'));
    if (files.length) addFiles(files);
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []);
    if (files.length) addFiles(files);
    e.target.value = '';
  };

  const handleUploadAll = async () => {
    const toUpload = pendingFiles.filter((f) => f.status === 'pending');
    if (toUpload.length === 0) return;
    setUploading(true);

    let successCount = 0;
    for (const item of toUpload) {
      setPendingFiles((prev) =>
        prev.map((f) => (f.id === item.id ? { ...f, status: 'uploading' } : f))
      );
      try {
        const url = await imageService.uploadProductImage(item.file);
        await productImageService.addImage(productId, url, false, images.length + successCount);
        successCount++;
        setPendingFiles((prev) =>
          prev.map((f) => (f.id === item.id ? { ...f, status: 'done' } : f))
        );
      } catch (err) {
        setPendingFiles((prev) =>
          prev.map((f) =>
            f.id === item.id
              ? { ...f, status: 'error', error: getApiError(err, tToast('imageUploadFailed')) }
              : f
          )
        );
      }
    }

    setUploading(false);
    // Clear done, keep errored
    setPendingFiles((prev) => {
      prev.filter((f) => f.status === 'done').forEach((f) => URL.revokeObjectURL(f.preview));
      return prev.filter((f) => f.status !== 'done');
    });
    if (successCount > 0) {
      await loadImages();
      toast.success(tToast('imageAdded'));
    }
  };

  const handleDeleteImage = async (imageId: string) => {
    setDeleting(imageId);
    try {
      await productImageService.deleteImage(productId, imageId);
      setImages((prev) => prev.filter((img) => img.id !== imageId));
      toast.success(tToast('imageDeleted'));
    } catch (err) {
      toast.error(getApiError(err, tToast('imageDeleteFailed')));
    } finally {
      setDeleting(null);
    }
  };

  const handleSetPrimary = async (imageId: string) => {
    setSettingPrimary(imageId);
    try {
      await productImageService.setPrimary(productId, imageId);
      setImages((prev) => prev.map((img) => ({ ...img, isPrimary: img.id === imageId })));
      toast.success(tToast('imagePrimarySet'));
    } catch (err) {
      toast.error(getApiError(err, tToast('imageSetPrimaryFailed')));
    } finally {
      setSettingPrimary(null);
    }
  };

  const pendingCount = pendingFiles.filter((f) => f.status === 'pending').length;

  return (
    <div className="border-t pt-4 mt-4">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-semibold text-gray-700">
          {tAdmin('productImagesLabel')}
          {images.length > 0 && (
            <span className="ml-1.5 text-xs font-normal text-gray-400">({images.length})</span>
          )}
        </h3>
      </div>

      {/* Upload zone */}
      <input
        ref={inputRef}
        type="file"
        accept={ALLOWED_TYPES.join(',')}
        multiple
        className="hidden"
        onChange={handleFileInput}
      />
      <div
        onDrop={handleDrop}
        onDragOver={(e) => e.preventDefault()}
        onClick={() => inputRef.current?.click()}
        className="w-full h-20 rounded-xl border-2 border-dashed border-gray-300 hover:border-rose-400 bg-gray-50 hover:bg-rose-50 transition-colors cursor-pointer flex flex-col items-center justify-center gap-1"
      >
        <FiUploadCloud size={20} className="text-gray-400" />
        <p className="text-xs text-gray-500">
          {tAdmin('dragDropMultiple')} <span className="text-rose-600 font-medium">{tAdmin('selectFiles')}</span>
        </p>
        <p className="text-[10px] text-gray-400">JPG, PNG, WebP — {MAX_SIZE_MB}MB max</p>
      </div>

      {/* Pending files staging area */}
      {pendingFiles.length > 0 && (
        <div className="mt-3 space-y-2">
          <div className="grid grid-cols-4 gap-2">
            {pendingFiles.map((item) => (
              <div
                key={item.id}
                className="relative rounded-lg overflow-hidden border border-gray-200 bg-gray-50 aspect-square"
              >
                <img src={item.preview} alt="" className="w-full h-full object-cover" />

                {item.status === 'uploading' && (
                  <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
                    <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  </div>
                )}
                {item.status === 'done' && (
                  <div className="absolute inset-0 bg-green-500/40 flex items-center justify-center">
                    <span className="text-white text-xl font-bold">✓</span>
                  </div>
                )}
                {item.status === 'error' && (
                  <div className="absolute inset-0 bg-red-500/60 flex flex-col items-center justify-center p-1">
                    <FiX size={16} className="text-white mb-0.5" />
                    <span className="text-white text-[9px] text-center leading-tight line-clamp-2">{item.error}</span>
                  </div>
                )}

                {item.status !== 'uploading' && (
                  <button
                    type="button"
                    onClick={(e) => { e.stopPropagation(); removePending(item.id); }}
                    className="absolute top-1 right-1 bg-black/60 text-white rounded-full p-0.5 hover:bg-black/80"
                  >
                    <FiX size={10} />
                  </button>
                )}
              </div>
            ))}
          </div>

          {pendingCount > 0 && (
            <button
              type="button"
              onClick={handleUploadAll}
              disabled={uploading}
              className="w-full py-2 text-xs font-semibold rounded-lg bg-rose-600 text-white hover:bg-rose-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {uploading
                ? tAdmin('uploadingImages')
                : tAdmin('uploadImagesBtn', { count: pendingCount })}
            </button>
          )}
        </div>
      )}

      {/* Saved images grid */}
      {loading ? (
        <div className="text-center py-4 text-xs text-gray-400 mt-3">{t('common:loading')}</div>
      ) : images.length > 0 ? (
        <div className="mt-3 grid grid-cols-3 gap-2">
          {images.map((img) => (
            <div
              key={img.id}
              className="relative group rounded-lg overflow-hidden border border-gray-200 bg-gray-50 aspect-square"
            >
              <img
                src={getImageUrl(img.imageUrl)}
                alt="Product"
                className="w-full h-full object-cover group-hover:opacity-75 transition-opacity"
              />

              {img.isPrimary && (
                <div className="absolute top-1 left-1 bg-rose-500 text-white text-[10px] px-1.5 py-0.5 rounded-full flex items-center gap-0.5 font-medium">
                  <FiStar size={9} fill="currentColor" />
                  {t('product:primaryBadge')}
                </div>
              )}

              <div className="absolute bottom-0 left-0 right-0 bg-black/70 opacity-0 group-hover:opacity-100 transition-opacity p-1.5 flex gap-1">
                {!img.isPrimary && (
                  <button
                    type="button"
                    onClick={() => handleSetPrimary(img.id)}
                    disabled={settingPrimary === img.id}
                    className="flex-1 py-1 bg-rose-500 text-white text-[10px] rounded hover:bg-rose-600 disabled:opacity-50 font-medium"
                  >
                    {settingPrimary === img.id ? '...' : t('product:setPrimary')}
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => handleDeleteImage(img.id)}
                  disabled={deleting === img.id}
                  className="p-1 bg-red-500 text-white rounded hover:bg-red-600 disabled:opacity-50"
                >
                  <FiTrash2 size={12} />
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="mt-3 text-center text-xs text-gray-400 py-3">{t('product:noImages')}</p>
      )}
    </div>
  );
}
