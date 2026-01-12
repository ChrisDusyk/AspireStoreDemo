import type { Cart, CartItem } from "../types/cart";

const CART_STORAGE_KEY = "copilot-demo-cart";

export function loadCart(): CartItem[] {
  try {
    const stored = localStorage.getItem(CART_STORAGE_KEY);
    if (!stored) return [];

    const cart: Cart = JSON.parse(stored);
    return cart.items || [];
  } catch (error) {
    console.error("Failed to load cart from localStorage:", error);
    return [];
  }
}

export function saveCart(items: CartItem[]): void {
  try {
    const cart: Cart = { items };
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart));
  } catch (error) {
    console.error("Failed to save cart to localStorage:", error);
  }
}

export function clearCart(): void {
  try {
    localStorage.removeItem(CART_STORAGE_KEY);
  } catch (error) {
    console.error("Failed to clear cart from localStorage:", error);
  }
}
