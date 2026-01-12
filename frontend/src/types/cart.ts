import type { ProductResponse } from "./product";

export interface CartItem {
  productId: string;
  quantity: number;
  product: ProductResponse;
}

export interface Cart {
  items: CartItem[];
}
