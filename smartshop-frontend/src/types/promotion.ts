export interface PriceCampaignItem {
  id: string;
  productId: string;
  productName: string;
  sizeId: string | null;
  sizeLabel: string | null;
  ruleType: number; // 1=Coefficient, 2=FixedPrice
  discountValue: number;
}

export interface PriceCampaignDto {
  id: string;
  name: string;
  startsAt: string;
  endsAt: string;
  appliesToAll: boolean;
  isActive: boolean;
  stores: { id: string; name: string }[];
  items: PriceCampaignItem[];
}

export interface PriceCampaignSummaryDto {
  id: string;
  name: string;
  startsAt: string;
  endsAt: string;
  appliesToAll: boolean;
  isActive: boolean;
  storeCount: number;
  itemCount: number;
}

export interface CreatePriceCampaignRequest {
  name: string;
  startsAt: string;
  endsAt: string;
  appliesToAll: boolean;
  storeIds: string[];
  items: {
    productId: string;
    sizeId: string | null;
    ruleType: number;
    discountValue: number;
  }[];
}

// Combo
export interface ComboItemDto {
  id: string;
  productId: string;
  productName: string;
  sizeId: string | null;
  sizeLabel: string | null;
  quantity: number;
  unitPriceSnapshot: number;
}

export interface ComboDto {
  id: string;
  name: string;
  title: string;
  description: string | null;
  imageUrl: string;
  originalPrice: number;
  salePrice: number;
  isActive: boolean;
  startsAt: string;
  endsAt: string | null;
  isCurrentlyActive: boolean;
  items: ComboItemDto[];
  createdAt: string;
}

export interface ComboSummaryDto {
  id: string;
  name: string;
  title: string;
  imageUrl: string;
  originalPrice: number;
  salePrice: number;
  isActive: boolean;
  startsAt: string;
  endsAt: string | null;
  isCurrentlyActive: boolean;
  itemCount: number;
  createdAt: string;
}

export interface CreateComboItemRequest {
  productId: string;
  sizeId: string | null;
  quantity: number;
}

export interface CreateComboRequest {
  name: string;
  title: string;
  description: string | null;
  imageUrl: string;
  salePrice: number;
  startsAt: string;
  endsAt: string | null;
  items: CreateComboItemRequest[];
}

