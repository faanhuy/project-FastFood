import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import { flashSaleService } from '../services/flashSaleService';
import { CountdownTimer } from '../components/CountdownTimer';
import { formatPrice } from '../utils/formatters';
import { getImageUrl } from '../utils/imageUrl';
import type { FlashSaleDto } from '../types/flashSale';

export default function FlashSalePage() {
  const { t } = useTranslation(['product', 'common']);
  const [flashSales, setFlashSales] = useState<FlashSaleDto[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await flashSaleService.getActive();
      setFlashSales(result.items);
    } catch {
      // silent - show empty state
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  if (loading)
    return (
      <>
        <Navbar />
        <div className="flex justify-center py-20 text-gray-400">{t('common:loading')}</div>
        <Footer />
      </>
    );

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <Navbar />
      <div className="flex-1 max-w-6xl mx-auto px-4 py-8 w-full">
        <h1 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
          {'⚡'} {t('product:flashSale')}
        </h1>

        {flashSales.length === 0 ? (
          <div className="text-center py-16 text-gray-400">{t('product:noFlashSales')}</div>
        ) : (
          <div className="space-y-8">
            {flashSales.map((fs) => (
              <div key={fs.id}>
                <div className="mb-4">
                  <h2 className="text-lg font-semibold text-gray-900">{fs.name}</h2>
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <span>{t('product:flashSaleEnds')}</span>
                    <CountdownTimer
                      remainingSeconds={fs.remainingSeconds}
                      onExpire={load}
                      className="text-red-600 font-medium"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                  {fs.items.map((item) => (
                    <Link
                      key={`${item.flashSaleId}-${item.productId}-${item.sizeId}`}
                      to={`/products/${item.productId}${item.sizeId ? `?sizeId=${item.sizeId}` : ''}`}
                      className="bg-white rounded-xl border shadow-sm hover:shadow-md transition-shadow p-4 flex flex-col gap-2"
                    >
                      {item.imageUrl && (
                        <img
                          src={getImageUrl(item.imageUrl)}
                          alt={item.productName}
                          className="w-full h-32 object-cover rounded-lg bg-gray-100"
                        />
                      )}
                      <div className="flex-1">
                        <p className="font-medium text-gray-900 line-clamp-2 text-sm">{item.productName}</p>
                        {item.sizeLabel && (
                          <p className="text-xs text-gray-500 mt-1">{item.sizeLabel}</p>
                        )}
                      </div>
                      <div>
                        <p className="text-xs text-gray-400 line-through">{formatPrice(item.originalPrice)}</p>
                        <p className="text-lg font-bold text-red-600">{formatPrice(item.salePrice)}</p>
                        <span className="inline-block px-2 py-0.5 rounded bg-red-100 text-red-600 text-xs font-bold">
                          {'-'}{item.percentDiscount}%
                        </span>
                      </div>
                      <div className="text-xs text-gray-400">
                        {item.remainingStock > 0
                          ? t('product:flashSaleRemaining', { count: item.remainingStock })
                          : t('product:flashSaleSoldOut')}
                      </div>
                    </Link>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
      <Footer />
    </div>
  );
}
