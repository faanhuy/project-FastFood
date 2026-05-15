export interface CatalogItemDto {
  id: string;
  itemType: 'Product' | 'Combo';
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;

  price: number;
  originalPrice?: number;
  discountPercent?: number;

  categoryId?: string;
  categoryName?: string;
  hasSizes: boolean;

  comboItemCount?: number;
  startsAt?: string;
  endsAt?: string;

  createdAt: string;
}

export interface GetCatalogResult {
  products: CatalogItemDto[];
  combos: CatalogItemDto[];
  totalProducts: number;
  totalCombos: number;
}
