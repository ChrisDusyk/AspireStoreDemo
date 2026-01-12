import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "react-oidc-context";
import { useCartContext } from "../contexts/CartContext";
import type { CreateOrderRequest } from "../types/order";
import type { ProductResponse } from "../types/product";

function CheckoutPage() {
  const navigate = useNavigate();
  const auth = useAuth();
  const { items: cartItems, clearCart } = useCartContext();

  const [shippingAddress, setShippingAddress] = useState("");
  const [shippingCity, setShippingCity] = useState("");
  const [shippingState, setShippingState] = useState("");
  const [shippingPostalCode, setShippingPostalCode] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [products, setProducts] = useState<Map<string, ProductResponse>>(
    new Map()
  );
  const [isLoadingProducts, setIsLoadingProducts] = useState(true);

  useEffect(() => {
    if (!auth.isAuthenticated) {
      auth.signinRedirect();
      return;
    }

    if (cartItems.length === 0) {
      navigate("/cart");
      return;
    }

    // Fetch product details for all cart items
    const fetchProducts = async () => {
      try {
        const productIds = cartItems.map((item) => item.productId);
        const responses = await Promise.all(
          productIds.map((id) =>
            fetch(`/api/products/${id}`).then((res) =>
              res.ok ? res.json() : null
            )
          )
        );

        const productMap = new Map<string, ProductResponse>();
        responses.forEach((product: ProductResponse | null) => {
          if (product) {
            productMap.set(product.id, product);
          }
        });

        setProducts(productMap);
      } catch (err) {
        setError("Failed to load product details");
      } finally {
        setIsLoadingProducts(false);
      }
    };

    fetchProducts();
  }, [auth.isAuthenticated, cartItems, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      // Build line items with current product details
      const lineItems = cartItems.map((item) => {
        const product = products.get(item.productId);
        if (!product || !product.name || product.price == null) {
          throw new Error(`Product ${item.productId} not found or incomplete`);
        }
        return {
          productId: item.productId,
          productName: product.name,
          productPrice: product.price,
          quantity: item.quantity,
        };
      });

      const request: CreateOrderRequest = {
        shippingAddress,
        shippingCity,
        shippingState,
        shippingPostalCode,
        lineItems,
      };

      const token = auth.user?.access_token;
      if (!token) {
        throw new Error("No access token available");
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const response = await fetch(`${apiBaseUrl}/api/orders`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const text = await response.text();
        let errorMessage = "Failed to create order";
        try {
          const errorData = JSON.parse(text);
          errorMessage = errorData.error || errorMessage;
        } catch {
          errorMessage = text || errorMessage;
        }
        throw new Error(errorMessage);
      }

      await response.json();

      // Clear cart on success
      clearCart();

      // Navigate to orders page
      navigate("/orders");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create order");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoadingProducts) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-xl">Loading...</div>
      </div>
    );
  }

  const totalAmount = cartItems.reduce((sum, item) => {
    const product = products.get(item.productId);
    return sum + (product ? (product.price ?? 0) * item.quantity : 0);
  }, 0);

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Checkout</h1>

      <div className="bg-gray-50 rounded-lg p-6 mb-8">
        <h2 className="text-xl font-semibold mb-4">Order Summary</h2>
        <div className="space-y-2">
          {cartItems.map((item) => {
            const product = products.get(item.productId);
            return (
              <div key={item.productId} className="flex justify-between">
                <span>
                  {product?.name || "Unknown Product"} x {item.quantity}
                </span>
                <span className="font-semibold">
                  ${((product?.price || 0) * item.quantity).toFixed(2)}
                </span>
              </div>
            );
          })}
          <div className="border-t pt-2 mt-2 flex justify-between text-lg font-bold">
            <span>Total:</span>
            <span>${totalAmount.toFixed(2)}</span>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <h2 className="text-xl font-semibold mb-4">Shipping Information</h2>

          <div className="space-y-4">
            <div>
              <label
                htmlFor="address"
                className="block text-sm font-medium mb-1"
              >
                Street Address
              </label>
              <input
                type="text"
                id="address"
                value={shippingAddress}
                onChange={(e) => setShippingAddress(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="123 Main St"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="city"
                  className="block text-sm font-medium mb-1"
                >
                  City
                </label>
                <input
                  type="text"
                  id="city"
                  value={shippingCity}
                  onChange={(e) => setShippingCity(e.target.value)}
                  required
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  placeholder="Seattle"
                />
              </div>

              <div>
                <label
                  htmlFor="state"
                  className="block text-sm font-medium mb-1"
                >
                  State
                </label>
                <input
                  type="text"
                  id="state"
                  value={shippingState}
                  onChange={(e) => setShippingState(e.target.value)}
                  required
                  maxLength={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  placeholder="WA"
                />
              </div>
            </div>

            <div>
              <label
                htmlFor="postalCode"
                className="block text-sm font-medium mb-1"
              >
                Postal Code
              </label>
              <input
                type="text"
                id="postalCode"
                value={shippingPostalCode}
                onChange={(e) =>
                  setShippingPostalCode(e.target.value.toUpperCase())
                }
                required
                pattern="[A-Z][0-9][A-Z] ?[0-9][A-Z][0-9]"
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="A1A 1A1"
              />
            </div>
          </div>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-300 text-red-800 px-4 py-3 rounded">
            {error}
          </div>
        )}

        <div className="flex gap-4">
          <button
            type="button"
            onClick={() => navigate("/cart")}
            className="flex-1 px-6 py-3 border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Back to Cart
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
          >
            {isSubmitting ? "Processing..." : "Place Order"}
          </button>
        </div>
      </form>
    </div>
  );
}

export default CheckoutPage;
