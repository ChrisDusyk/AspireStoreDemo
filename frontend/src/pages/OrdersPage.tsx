import { useEffect, useState } from "react";
import { useAuth } from "react-oidc-context";

interface Order {
  id: string;
  date: string;
  total: number;
  status: string;
}

function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
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

        const data = await response.json();
        setOrders(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to fetch orders");
      } finally {
        setLoading(false);
      }
    }

    fetchOrders();
  }, [auth.user]);

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
      <p className="text-gray-600 mb-4">
        View your order history. This page is protected and requires
        authentication.
      </p>

      {orders.length === 0 ? (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <p className="text-blue-800">No orders found.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <div
              key={order.id}
              className="border rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div>
                  <p className="font-semibold">
                    Order #{order.id.substring(0, 8)}
                  </p>
                  <p className="text-sm text-gray-600">
                    {new Date(order.date).toLocaleDateString()}
                  </p>
                </div>
                <div className="text-right">
                  <p className="font-semibold">${order.total.toFixed(2)}</p>
                  <p className="text-sm text-gray-600">{order.status}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mt-8">
        <p className="text-sm text-yellow-800">
          <strong>TODO:</strong> Implement full order management with order
          details view and order creation flow.
        </p>
      </div>
    </div>
  );
}

export default OrdersPage;
