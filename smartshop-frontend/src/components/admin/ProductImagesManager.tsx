import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { FiTrash2, FiStar } from 'react-icons/fi';
import { productImageService } from '../../services/productImageService';
import { getApiError } from '../../utils/errorHandler';
import type { ProductImageDto } from '../../types/product';

interface ProductImagesManagerProps {
  productId: string;
}

export default function ProductImagesManager({ productId }: ProductImagesManagerProps) {
  const { t } = useTranslation(['product', 'common', 'toast']);
  const { t: tToast } = useTranslation('toast');

  const [images, setImages] = useState<ProductImageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [imageUrl, setImageUrl] = useState('');
  const [adding, setAdding] = useState(false);
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

  const handleAddImage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!imageUrl.trim()) return;

    setAdding(true);
    try {
      const newImage = await productImageService.addImage(
        productId,
        imageUrl,
        false,
        images.length
      );
      setImages((prev) => [...prev, newImage].sort((a, b) => a.sortOrder - b.sortOrder));
      setImageUrl('');
      toast.success(tToast('imageAdded'));
    } catch (error: any) {
      toast.error(getApiError(error, tToast('imageAddFailed')));
    } finally {
      setAdding(false);
    }
  };

  const handleDeleteImage = async (imageId: string) => {
    setDeleting(imageId);
    try {
      await productImageService.deleteImage(productId, imageId);
      setImages((prev) => prev.filter((img) => img.id !== imageId));
      toast.success(tToast('imageDeleted'));
    } catch (error: any) {
      toast.error(getApiError(error, tToast('imageDeleteFailed')));
    } finally {
      setDeleting(null);
    }
  };

  const handleSetPrimary = async (imageId: string) => {
    setSettingPrimary(imageId);
    try {
      await productImageService.setPrimary(productId, imageId);
      setImages((prev) =>
        prev.map((img) => ({
          ...img,
          isPrimary: img.id === imageId,
        }))
      );
      toast.success(tToast('imagePrimarySet'));
    } catch (error: any) {
      toast.error(getApiError(error, tToast('imageSetPrimaryFailed')));
    } finally {
      setSettingPrimary(null);
    }
  };

  if (loading) {
    return <div className="text-center py-4 text-sm text-gray-500">{t('common:loading')}</div>;
  }

  return (
    <div className="border-t pt-6 mt-6">
      <h3 className="text-lg font-semibold mb-4">{t('product:images')}</h3>

      {/* Add image form */}
      <form onSubmit={handleAddImage} className="mb-6 p-4 bg-gray-50 rounded-lg">
        <div className="flex gap-2">
          <input
            type="text"
            placeholder={t('product:imageUrl')}
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-rose-400 focus:outline-none"
          />
          <button
            type="submit"
            disabled={adding || !imageUrl.trim()}
            className="px-4 py-2 bg-rose-400 text-white rounded-lg text-sm font-medium hover:bg-rose-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {adding ? t('common:adding') : t('product:addImage')}
          </button>
        </div>
      </form>

      {/* Images grid */}
      {images.length === 0 ? (
        <div className="text-center py-8 text-gray-500 text-sm">{t('product:noImages')}</div>
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {images.map((img) => (
            <div
              key={img.id}
              className="relative group border border-gray-200 rounded-lg overflow-hidden bg-gray-50"
            >
              <img
                src={img.imageUrl}
                alt="Product"
                className="w-full h-40 object-cover group-hover:opacity-75 transition-opacity"
              />

              {/* Primary badge */}
              {img.isPrimary && (
                <div className="absolute top-2 right-2 bg-rose-400 text-white text-xs px-2 py-1 rounded flex items-center gap-1">
                  <FiStar size={12} fill="currentColor" />
                  {t('product:primaryBadge')}
                </div>
              )}

              {/* Action buttons */}
              <div className="absolute bottom-0 left-0 right-0 bg-black/70 opacity-0 group-hover:opacity-100 transition-opacity p-2 flex gap-2">
                {!img.isPrimary && (
                  <button
                    onClick={() => handleSetPrimary(img.id)}
                    disabled={settingPrimary === img.id}
                    className="flex-1 px-2 py-1 bg-rose-400 text-white text-xs rounded hover:bg-rose-500 disabled:opacity-50 transition-colors"
                  >
                    {settingPrimary === img.id ? t('common:updating') : t('product:setPrimary')}
                  </button>
                )}
                <button
                  onClick={() => handleDeleteImage(img.id)}
                  disabled={deleting === img.id}
                  className="px-2 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50 transition-colors"
                >
                  {deleting === img.id ? (
                    <FiTrash2 size={14} />
                  ) : (
                    <FiTrash2 size={14} />
                  )}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
