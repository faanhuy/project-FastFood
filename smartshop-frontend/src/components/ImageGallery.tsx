import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import type { ProductImageDto } from '../types/product';

interface ImageGalleryProps {
  images: ProductImageDto[];
  productName: string;
}

export default function ImageGallery({ images, productName }: ImageGalleryProps) {
  const { t } = useTranslation('product');

  const primaryImage = useMemo(
    () => images.find((img) => img.isPrimary) ?? images[0],
    [images]
  );

  const [selectedImage, setSelectedImage] = useState<ProductImageDto | null>(primaryImage ?? null);

  if (images.length === 0) {
    return (
      <div className="w-full bg-gray-200 rounded-lg flex items-center justify-center aspect-square">
        <span className="text-gray-500 text-sm">{t('noImages')}</span>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Main image */}
      <div className="w-full bg-gray-100 rounded-lg overflow-hidden flex items-center justify-center aspect-square">
        {selectedImage ? (
          <img
            src={selectedImage.imageUrl}
            alt={productName}
            className="w-full h-full object-contain"
          />
        ) : (
          <span className="text-gray-400 text-sm">{t('noImages')}</span>
        )}
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <div className="grid grid-cols-5 gap-2">
          {images.map((img) => (
            <button
              key={img.id}
              onClick={() => setSelectedImage(img)}
              className={`relative aspect-square rounded-lg overflow-hidden border-2 transition-colors ${
                selectedImage?.id === img.id
                  ? 'border-rose-400'
                  : 'border-gray-300 hover:border-gray-400'
              }`}
              aria-label={`Select image ${img.sortOrder + 1}`}
            >
              <img src={img.imageUrl} alt={`Thumbnail ${img.sortOrder}`} className="w-full h-full object-cover" />
              {img.isPrimary && (
                <div className="absolute top-0 right-0 bg-rose-400 text-white text-xs px-2 py-1 rounded-bl">
                  {t('primaryBadge')}
                </div>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
