import { useTranslation } from 'react-i18next';
import { CountdownTimer } from './CountdownTimer';
import type { FlashSaleItemDto } from '../types/flashSale';

interface Props {
  item: FlashSaleItemDto & { remainingSeconds: number };
  onExpire?: () => void;
}

export function FlashSaleBadge({ item, onExpire }: Props) {
  const { t } = useTranslation('product');

  if (item.remainingStock <= 0) {
    return (
      <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-gray-200 text-gray-600">
        {t('flashSaleSoldOut')}
      </span>
    );
  }

  return (
    <div className="flex flex-col gap-0.5">
      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-bold bg-red-500 text-white">
        {'⚡'} {t('flashSaleOff', { percent: item.percentDiscount })}
      </span>
      <div className="text-xs text-red-600 font-medium flex items-center gap-1">
        <span>{t('flashSaleEnds')}</span>
        <CountdownTimer remainingSeconds={item.remainingSeconds} onExpire={onExpire} className="text-red-700" />
      </div>
      <span className="text-xs text-gray-500">{t('flashSaleRemaining', { count: item.remainingStock })}</span>
    </div>
  );
}
