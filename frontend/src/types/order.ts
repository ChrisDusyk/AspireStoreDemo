export const OrderStatus = {
  Pending: 0,
  Processing: 1,
  Shipped: 2,
  Delivered: 3,
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];

export interface OrderLineItem {
  productId: string;
  productName: string;
  productPrice: number;
  quantity: number;
}

export interface Order {
  id: string;
  userId: string;
  userEmail: string;
  shippingAddress: string;
  shippingCity: string;
  shippingState: string;
  shippingPostalCode: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
  lineItems: OrderLineItem[];
}

export interface CreateOrderLineItemDto {
  productId: string;
  productName: string;
  productPrice: number;
  quantity: number;
}

export interface CreateOrderRequest {
  shippingAddress: string;
  shippingCity: string;
  shippingState: string;
  shippingPostalCode: string;
  lineItems: CreateOrderLineItemDto[];
  cardNumber: string;
  cardholderName: string;
  expiryDate: string;
  cvv: string;
}
