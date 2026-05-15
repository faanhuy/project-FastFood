export interface CartItemComponentDto {
  productId: string;
  productName: string;
  sizeId: string | null;
  sizeLabel: string | null;
  quantityPerCombo: number;
  totalQuantity: number;
  unitPriceSnapshot: number;
}

export interface CartItemDto {
  id: string;
  itemType: 'Product' | 'Combo';
  productId: string | null;
  comboId: string | null;
  displayName: string;
  imageUrl: string | null;
  quantity: number;
  unitPrice: number;
  subTotal: number;
  sizeId: string | null;
  sizeLabel: string | null;
  components: CartItemComponentDto[];
}

export interface CartDto {
  id: string;
  userId: string;
  items: CartItemDto[];
  totalAmount: number;
}
