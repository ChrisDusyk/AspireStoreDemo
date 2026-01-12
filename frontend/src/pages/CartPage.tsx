import { useCart } from "../hooks/useCart";

export function CartPage() {
  const { items, updateQuantity, removeItem, totalPrice, clearCart } =
    useCart();

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
          <a
            href="/"
            className="inline-block bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Continue Shopping
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Shopping Cart</h1>

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
            {items.map((item) => {
              const price = item.product.price ?? 0;
              const subtotal = price * item.quantity;

              return (
                <tr key={item.productId}>
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">
                      {item.product.name || "Unnamed Product"}
                    </div>
                    {item.product.description && (
                      <div className="text-sm text-gray-500 line-clamp-2">
                        {item.product.description}
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    ${price.toFixed(2)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
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
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    ${subtotal.toFixed(2)}
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
              disabled
              className="px-6 py-3 bg-gray-400 text-white rounded-lg cursor-not-allowed"
              title="Checkout functionality coming soon"
            >
              Go to Checkout
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
