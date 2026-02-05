import { useEffect, useState } from "react";
import { useAuth } from "react-oidc-context";
import type { Order, OrderStatus } from "../types/order";

function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedOrderId, setExpandedOrderId] = useState<string | null>(null);
  const auth = useAuth();

  useEffect(() => {
    async function fetchOrders() {
      try {
        const token = auth.user?.access_token;
        if (!token) {
          setError("No access token available");
          setLoading(false);
          return;
        }

        const apiBaseUrl = import.meta.env.VITE_API_URL || "";
        const response = await fetch(`${apiBaseUrl}/api/orders`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        if (!response.ok) {
          throw new Error(`Failed to fetch orders: ${response.status}`);
        }

        const data: Order[] = await response.json();
        setOrders(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to fetch orders");
      } finally {
        setLoading(false);
      }
    }

    fetchOrders();
  }, [auth.user]);

  const getStatusBadgeClass = (status: OrderStatus) => {
    switch (status) {
      case 0: // Pending
        return "bg-yellow-100 text-yellow-800 border-yellow-300";
      case 1: // Processing
        return "bg-blue-100 text-blue-800 border-blue-300";
      case 2: // Shipped
        return "bg-purple-100 text-purple-800 border-purple-300";
      case 3: // Delivered
        return "bg-green-100 text-green-800 border-green-300";
      default:
        return "bg-gray-100 text-gray-800 border-gray-300";
    }
  };

  const getStatusText = (status: OrderStatus) => {
    switch (status) {
      case 0:
        return "Pending";
      case 1:
        return "Processing";
      case 2:
        return "Shipped";
      case 3:
        return "Delivered";
      default:
        return "Unknown";
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-xl">Loading orders...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <h1 className="text-4xl font-bold mb-8">My Orders</h1>
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-8">My Orders</h1>

      {orders.length === 0 ? (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 text-center">
          <p className="text-blue-800 text-lg mb-2">No orders yet</p>
          <p className="text-blue-600">
            Your order history will appear here after you make a purchase.
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <div
              key={order.id}
              className="border rounded-lg overflow-hidden hover:shadow-md transition-shadow"
            >
              <div className="bg-white p-6">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <p className="font-semibold text-lg">
                      Order #{order.id.substring(0, 8).toUpperCase()}
                    </p>
                    <p className="text-sm text-gray-600">
                      Placed on{" "}
                      {new Date(order.orderDate).toLocaleDateString("en-US", {
                        year: "numeric",
                        month: "long",
                        day: "numeric",
                      })}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-2xl font-bold text-gray-900">
                      ${order.totalAmount.toFixed(2)}
                    </p>
                    <span
                      className={`inline-block px-3 py-1 text-sm font-semibold border rounded-full mt-2 ${getStatusBadgeClass(
                        order.status,
                      )}`}
                    >
                      {getStatusText(order.status)}
                    </span>
                  </div>
                </div>

                <div className="mb-4 text-sm text-gray-600">
                  <p className="font-medium text-gray-700 mb-1">
                    Shipping Address:
                  </p>
                  <p>{order.shippingAddress}</p>
                  <p>
                    {order.shippingCity}, {order.shippingState}{" "}
                    {order.shippingPostalCode}
                  </p>
                  {order.trackingNumber &&
                    (order.status === 2 || order.status === 3) && (
                      <p className="mt-2">
                        <span className="font-medium">Tracking Number:</span>{" "}
                        {order.trackingNumber}
                      </p>
                    )}
                </div>

                <button
                  onClick={() =>
                    setExpandedOrderId(
                      expandedOrderId === order.id ? null : order.id,
                    )
                  }
                  className="text-blue-600 hover:text-blue-800 font-medium text-sm flex items-center gap-1"
                >
                  {expandedOrderId === order.id ? (
                    <>
                      <span>Hide Details</span>
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth={2}
                        stroke="currentColor"
                        className="w-4 h-4"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="m4.5 15.75 7.5-7.5 7.5 7.5"
                        />
                      </svg>
                    </>
                  ) : (
                    <>
                      <span>View Details</span>
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth={2}
                        stroke="currentColor"
                        className="w-4 h-4"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="m19.5 8.25-7.5 7.5-7.5-7.5"
                        />
                      </svg>
                    </>
                  )}
                </button>
              </div>

              {expandedOrderId === order.id && (
                <div className="bg-gray-50 border-t px-6 py-4">
                  <h3 className="font-semibold mb-3">Order Items</h3>
                  <table className="w-full">
                    <thead className="bg-gray-100">
                      <tr>
                        <th className="text-left px-4 py-2 text-sm font-medium text-gray-700">
                          Product
                        </th>
                        <th className="text-right px-4 py-2 text-sm font-medium text-gray-700">
                          Price
                        </th>
                        <th className="text-center px-4 py-2 text-sm font-medium text-gray-700">
                          Quantity
                        </th>
                        <th className="text-right px-4 py-2 text-sm font-medium text-gray-700">
                          Subtotal
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white">
                      {order.lineItems.map((item, index) => (
                        <tr key={index} className="border-t">
                          <td className="px-4 py-3 text-sm">
                            {item.productName}
                          </td>
                          <td className="px-4 py-3 text-sm text-right">
                            ${item.productPrice.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-sm text-center">
                            {item.quantity}
                          </td>
                          <td className="px-4 py-3 text-sm text-right font-semibold">
                            ${(item.productPrice * item.quantity).toFixed(2)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default OrdersPage;
