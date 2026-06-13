export interface WishlistItem {
  productId: string;
  productName: string;
  price: number;
  effectivePrice?: number;
  imageUrl?: string;
  isInStock: boolean;
  slug: string;
}
