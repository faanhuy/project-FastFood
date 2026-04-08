export interface CartItemDto {
  productId: string;
  productName: string;
  productImageUrl: string | null;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface CartDto {
  id: string;
  userId: string;
  items: CartItemDto[];
  totalAmount: number;
}
