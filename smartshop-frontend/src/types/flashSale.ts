export interface FlashSaleItemDto {
  flashSaleId: string;
  productId: string;
  productName: string;
  imageUrl?: string;
  sizeId?: string;
  sizeLabel?: string;
  salePrice: number;
  originalPrice: number;
  stockLimit: number;
  soldCount: number;
  remainingStock: number;
  percentDiscount: number;
}

export interface FlashSaleDto {
  id: string;
  name: string;
  startAt: string;
  endAt: string;
  isActive: boolean;
  remainingSeconds: number;
  status: 'Draft' | 'PendingApproval' | 'Approved' | 'Rejected';
  approvedBy?: string;
  approvedAt?: string;
  rejectedReason?: string;
  items: FlashSaleItemDto[];
}

export interface CreateFlashSaleItemRequest {
  productId: string;
  sizeId?: string;
  salePrice: number;
  stockLimit: number;
}

export interface CreateFlashSaleRequest {
  name: string;
  startAt: string;
  endAt: string;
  items: CreateFlashSaleItemRequest[];
}

export type UpdateFlashSaleRequest = CreateFlashSaleRequest;
