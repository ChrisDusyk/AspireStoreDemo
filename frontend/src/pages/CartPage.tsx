import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "react-oidc-context";
import { useCartContext } from "../contexts/CartContext";
import type { ProductResponse } from "../types/product";

interface CartItemWithProduct {
  productId: string;
  quantity: number;
  product: ProductResponse | null;
  loading: boolean;
  error: boolean;
}

export function CartPage() {
  const navigate = useNavigate();
  const auth = useAuth();
  const { items, updateQuantity, removeItem, clearCart } = useCartContext();
  const [cartItems, setCartItems] = useState<CartItemWithProduct[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchProducts() {
      if (items.length === 0) {
        setCartItems([]);
        setLoading(false);
        return;
      }

      setLoading(true);

      const itemsWithProducts = await Promise.all(
        items.map(async (item) => {
          try {
            const res = await fetch(`/api/products/${item.productId}`);
            if (!res.ok) {
              return {
                ...item,
                product: null,
                loading: false,
                error: true,
              };
            }
            const product: ProductResponse = await res.json();
            return {
              ...item,
              product,
              loading: false,
              error: false,
            };
          } catch {
            return {
              ...item,
              product: null,
              loading: false,
              error: true,
            };
          }
        })
      );

      setCartItems(itemsWithProducts);
      setLoading(false);
    }

    fetchProducts();
  }, [items]);

  const hasUnavailableProducts = cartItems.some((item) => !item.product);

  const totalPrice = cartItems.reduce((total, item) => {
    if (!item.product) return total;
    const price = item.product.price ?? 0;
    return total + price * item.quantity;
  }, 0);

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Shopping Cart</h1>
        <div className="bg-white rounded-lg shadow p-8">
          <div className="animate-pulse space-y-4">
            <div className="h-16 bg-gray-200 rounded" />
            <div className="h-16 bg-gray-200 rounded" />
            <div className="h-16 bg-gray-200 rounded" />
          </div>
        </div>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Shopping Cart</h1>
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            strokeWidth={1.5}
            stroke="currentColor"
            className="w-16 h-16 mx-auto text-gray-400 mb-4"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 0 0-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 0 0-16.536-1.84M7.5 14.25 5.106 5.272M6 20.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Zm12.75 0a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Z"
            />
          </svg>
          <p className="text-xl text-gray-600 mb-4">Your cart is empty</p>
          <Link
            to="/"
            className="inline-block bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Shopping Cart</h1>

      {hasUnavailableProducts && (
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-yellow-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-yellow-700">
                Some products in your cart are no longer available. Please
                remove them before proceeding to checkout.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Product
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Price
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Quantity
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Subtotal
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {cartItems.map((item) => {
              const price = item.product?.price ?? 0;
              const subtotal = price * item.quantity;
              const isUnavailable = !item.product;

              return (
                <tr
                  key={item.productId}
                  className={isUnavailable ? "bg-red-50" : ""}
                >
                  <td className="px-6 py-4">
                    {isUnavailable ? (
                      <div>
                        <div className="text-sm font-medium text-red-600">
                          Product Unavailable
                        </div>
                        <div className="text-sm text-red-500">
                          This product has been removed or is no longer
                          available
                        </div>
                      </div>
                    ) : (
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {item.product?.name || "Unnamed Product"}
                        </div>
                        {item.product?.description && (
                          <div className="text-sm text-gray-500 line-clamp-2">
                            {item.product?.description}
                          </div>
                        )}
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {isUnavailable ? "—" : `$${price.toFixed(2)}`}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {isUnavailable ? (
                      <span className="text-sm text-gray-500">
                        {item.quantity}
                      </span>
                    ) : (
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() =>
                            updateQuantity(item.productId, item.quantity - 1)
                          }
                          className="w-8 h-8 flex items-center justify-center bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors"
                          aria-label="Decrease quantity"
                        >
                          −
                        </button>
                        <span className="w-12 text-center text-sm font-medium">
                          {item.quantity}
                        </span>
                        <button
                          onClick={() =>
                            updateQuantity(item.productId, item.quantity + 1)
                          }
                          className="w-8 h-8 flex items-center justify-center bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors"
                          aria-label="Increase quantity"
                        >
                          +
                        </button>
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {isUnavailable ? "—" : `$${subtotal.toFixed(2)}`}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm">
                    <button
                      onClick={() => removeItem(item.productId)}
                      className="text-red-600 hover:text-red-800 transition-colors"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>

        <div className="bg-gray-50 px-6 py-4">
          <div className="flex justify-between items-center mb-4">
            <span className="text-lg font-medium text-gray-900">Total:</span>
            <span className="text-2xl font-bold text-gray-900">
              ${totalPrice.toFixed(2)}
            </span>
          </div>

          <div className="flex justify-between items-center gap-4">
            <button
              onClick={clearCart}
              className="px-6 py-3 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
            >
              Clear Cart
            </button>
            <button
              onClick={() => {
                if (!auth.isAuthenticated) {
                  auth.signinRedirect();
                } else {
                  navigate("/checkout");
                }
              }}
              disabled={hasUnavailableProducts}
              className={`px-6 py-3 rounded-lg transition-colors ${
                hasUnavailableProducts
                  ? "bg-gray-400 text-white cursor-not-allowed"
                  : "bg-blue-600 text-white hover:bg-blue-700"
              }`}
              title={
                hasUnavailableProducts
                  ? "Remove unavailable products before checkout"
                  : undefined
              }
            >
              {auth.isAuthenticated ? "Go to Checkout" : "Login to Checkout"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
