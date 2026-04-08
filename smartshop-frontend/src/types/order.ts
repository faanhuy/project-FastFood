export interface OrderItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface OrderDto {
  id: string;
  userId: string;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  notes: string | null;
  items: OrderItemDto[];
  createdAt: string;
}

export interface PlaceOrderRequest {
  shippingAddress: string;
  notes?: string;
}
