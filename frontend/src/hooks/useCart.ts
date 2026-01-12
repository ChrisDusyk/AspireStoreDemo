import { useState, useEffect, useCallback } from "react";
import type { CartItem } from "../types/cart";
import type { ProductResponse } from "../types/product";
import {
  loadCart,
  saveCart,
  clearCart as clearCartStorage,
} from "../utils/cartStorage";

export function useCart() {
  const [items, setItems] = useState<CartItem[]>(() => loadCart());

  useEffect(() => {
    saveCart(items);
  }, [items]);

  const addItem = useCallback((product: ProductResponse) => {
    setItems((currentItems) => {
      const existingItem = currentItems.find(
        (item) => item.productId === product.id
      );

      if (existingItem) {
        return currentItems.map((item) =>
          item.productId === product.id
            ? { ...item, quantity: item.quantity + 1 }
            : item
        );
      }

      return [...currentItems, { productId: product.id, quantity: 1, product }];
    });
  }, []);

  const removeItem = useCallback((productId: string) => {
    setItems((currentItems) =>
      currentItems.filter((item) => item.productId !== productId)
    );
  }, []);

  const updateQuantity = useCallback(
    (productId: string, quantity: number) => {
      if (quantity <= 0) {
        removeItem(productId);
        return;
      }

      setItems((currentItems) =>
        currentItems.map((item) =>
          item.productId === productId ? { ...item, quantity } : item
        )
      );
    },
    [removeItem]
  );

  const clearCart = useCallback(() => {
    setItems([]);
    clearCartStorage();
  }, []);

  const totalPrice = items.reduce((total, item) => {
    const price = item.product.price ?? 0;
    return total + price * item.quantity;
  }, 0);

  const itemCount = items.reduce((count, item) => count + item.quantity, 0);

  return {
    items,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    totalPrice,
    itemCount,
  };
}
