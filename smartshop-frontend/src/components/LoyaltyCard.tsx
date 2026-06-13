import { useTranslation } from 'react-i18next';
import type { LoyaltyAccountDto } from '../types/loyalty';

const TIER_COLORS = {
  Bronze: 'bg-amber-100 text-amber-800 border-amber-200',
  Silver: 'bg-gray-100 text-gray-700 border-gray-200',
  Gold: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  Platinum: 'bg-purple-100 text-purple-800 border-purple-200',
};

const TIER_THRESHOLDS: Record<string, number> = {
  Bronze: 0,
  Silver: 1000,
  Gold: 5000,
  Platinum: 20000,
};

interface Props {
  account: LoyaltyAccountDto;
}

export function LoyaltyCard({ account }: Props) {
  const { t } = useTranslation('common');
  const tierKey = `tier${account.tier}` as const;
  const tierLabel = t(tierKey);

  const currentThreshold = TIER_THRESHOLDS[account.tier] ?? 0;
  const nextThreshold =
    account.tier === 'Platinum'
      ? currentThreshold
      : Object.values(TIER_THRESHOLDS).find((v) => v > account.lifetimePoints) ?? currentThreshold;
  const progress =
    account.tier === 'Platinum' ? 100 : Math.min(100, ((account.lifetimePoints - currentThreshold) / (nextThreshold - currentThreshold)) * 100);

  return (
    <div className="bg-white rounded-xl border shadow-sm p-5">
      <div className="flex items-center justify-between mb-3">
        <h3 className="font-semibold text-gray-800">{t('loyaltyPoints')}</h3>
        <span className={`px-2.5 py-1 rounded-full text-xs font-bold border ${TIER_COLORS[account.tier as keyof typeof TIER_COLORS]}`}>
          {tierLabel}
        </span>
      </div>

      <div className="mb-4">
        <p className="text-3xl font-bold text-rose-600">{account.totalPoints.toLocaleString()}</p>
        <p className="text-xs text-gray-400 mt-0.5">{t('pointsValue', { value: account.pointsValueVnd.toLocaleString() })}</p>
      </div>

      {account.tier !== 'Platinum' ? (
        <div>
          <div className="flex justify-between text-xs text-gray-500 mb-1">
            <span>
              {t('lifetimePoints')}: {account.lifetimePoints.toLocaleString()}
            </span>
            <span>
              {account.nextTierPoints} pts → {account.nextTier && t(`tier${account.nextTier}` as any)}
            </span>
          </div>
          <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
            <div className="h-full bg-rose-500 rounded-full transition-all" style={{ width: `${progress}%` }} />
          </div>
        </div>
      ) : (
        <p className="text-xs text-purple-600 font-medium">{t('alreadyTopTier')}</p>
      )}
    </div>
  );
}
