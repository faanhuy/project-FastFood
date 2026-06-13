import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { loyaltyService } from '../services/loyaltyService';
import type { PointTransactionDto } from '../types/loyalty';

export function PointHistoryList() {
  const { t } = useTranslation('common');
  const [items, setItems] = useState<PointTransactionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const result = await loyaltyService.getTransactions(page);
        setItems(result.items);
        setTotalPages(result.totalPages);
      } catch {
        toast.error(t('toast:loyaltyLoadFailed'));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [page, t]);

  const typeLabel = (type: string) => {
    const map: Record<string, string> = {
      Earn: t('pointEarn'),
      Redeem: t('pointRedeem'),
      Expire: t('pointExpire'),
      Adjust: t('pointAdjust'),
      Reverse: t('pointReverse'),
    };
    return map[type] ?? type;
  };

  const isDeduction = (type: string) =>
    type === 'Redeem' || type === 'Expire' || type === 'Reverse';

  if (loading) return <div className="text-sm text-gray-400">{t('loading')}</div>;

  return (
    <div>
      <div className="divide-y divide-gray-100">
        {items.map((tx) => (
          <div key={tx.id} className="flex items-center justify-between py-3">
            <div>
              <p className="text-sm font-medium text-gray-800">{typeLabel(tx.type)}</p>
              {tx.note && <p className="text-xs text-gray-400">{tx.note}</p>}
              <p className="text-xs text-gray-400">{new Date(tx.createdAt).toLocaleDateString('vi-VN')}</p>
            </div>
            <span className={`text-sm font-bold ${isDeduction(tx.type) ? 'text-red-500' : 'text-green-600'}`}>
              {isDeduction(tx.type) ? '-' : '+'}{tx.points}
            </span>
          </div>
        ))}
        {items.length === 0 && <p className="text-sm text-gray-400 py-4 text-center">{t('noData')}</p>}
      </div>
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="px-3 py-1 text-sm border rounded disabled:opacity-40"
          >
            {t('back')}
          </button>
          <span className="text-sm text-gray-500">
            {page}/{totalPages}
          </span>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="px-3 py-1 text-sm border rounded disabled:opacity-40"
          >
            {t('next')}
          </button>
        </div>
      )}
    </div>
  );
}
