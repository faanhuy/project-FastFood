export interface LoyaltyAccountDto {
  id: string;
  userId: string;
  totalPoints: number;
  lifetimePoints: number;
  tier: 'Bronze' | 'Silver' | 'Gold' | 'Platinum';
  nextTierPoints: number;
  nextTier?: string;
  pointsValueVnd: number;
}

export interface PointTransactionDto {
  id: string;
  points: number;
  type: 'Earn' | 'Redeem' | 'Expire' | 'Adjust' | 'Reverse';
  note?: string;
  orderId?: string;
  createdAt: string;
}
