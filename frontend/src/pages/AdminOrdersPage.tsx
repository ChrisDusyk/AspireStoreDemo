import { useEffect, useState, useCallback } from "react";
import { useAuth } from "react-oidc-context";
import type { Order } from "../types/order";
import ErrorDisplay from "../components/ErrorDisplay";

interface PagedOrderResponse {
  orders: Order[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

function AdminOrdersPage() {
  const auth = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [acceptingOrders, setAcceptingOrders] = useState<Set<string>>(
    new Set(),
  );
  const [secondsUntilRefresh, setSecondsUntilRefresh] = useState(60);

  const fetchOrders = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const token = auth.user?.access_token;
      if (!token) {
        setError("No access token available");
        setLoading(false);
        return;
      }

      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: "25",
      });

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(
        `${apiBaseUrl}/api/admin/orders/pending?${params}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        throw new Error("Failed to fetch pending orders.");
      }

      const data: PagedOrderResponse = await res.json();
      setOrders(data.orders);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Unknown error occurred.");
      }
      setOrders([]);
      setTotalPages(1);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [page, auth.user]);

  // Auto-refresh countdown timer
  useEffect(() => {
    const countdownInterval = setInterval(() => {
      setSecondsUntilRefresh((prev) => {
        if (prev <= 1) {
          return 60;
        }
        return prev - 1;
      });
    }, 1000);

    return () => {
      clearInterval(countdownInterval);
    };
  }, []);

  // Auto-refresh on timer
  useEffect(() => {
    if (secondsUntilRefresh === 60) {
      fetchOrders();
    }
  }, [secondsUntilRefresh, fetchOrders]);

  // Initial fetch
  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  const handleManualRefresh = () => {
    setSecondsUntilRefresh(60);
    fetchOrders();
  };

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setPage(newPage);
    }
  };

  const handlePageJump = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const jumpPage = parseInt(formData.get("jumpPage") as string, 10);
    if (!isNaN(jumpPage) && jumpPage >= 1 && jumpPage <= totalPages) {
      setPage(jumpPage);
    }
  };

  const toggleRowExpansion = (orderId: string) => {
    setExpandedRows((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(orderId)) {
        newSet.delete(orderId);
      } else {
        newSet.add(orderId);
      }
      return newSet;
    });
  };

  const handleAcceptOrder = async (orderId: string) => {
    setAcceptingOrders((prev) => new Set(prev).add(orderId));
    setError(null);

    try {
      const token = auth.user?.access_token;
      if (!token) {
        setError("No access token available");
        setAcceptingOrders((prev) => {
          const newSet = new Set(prev);
          newSet.delete(orderId);
          return newSet;
        });
        return;
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(
        `${apiBaseUrl}/api/admin/orders/${orderId}/accept`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}));
        throw new Error(errorData.error || "Failed to accept order");
      }

      // Remove order from list immediately
      setOrders((prev) => prev.filter((order) => order.id !== orderId));
      setTotalCount((prev) => prev - 1);

      // Refresh to get updated list
      setSecondsUntilRefresh(60);
      fetchOrders();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Unknown error occurred while accepting order.");
      }
    } finally {
      setAcceptingOrders((prev) => {
        const newSet = new Set(prev);
        newSet.delete(orderId);
        return newSet;
      });
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit",
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(amount);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Pending Orders Queue</h1>
        <button
          onClick={handleManualRefresh}
          disabled={loading}
          className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
        >
          Refresh ({secondsUntilRefresh}s)
        </button>
      </div>

      {error && <ErrorDisplay message={error} />}

      {/* Results Count */}
      <div className="mb-4 text-sm text-gray-600">
        {totalCount === 0 ? (
          <span className="font-medium text-green-600">No pending orders</span>
        ) : (
          <>
            Showing {orders.length} of {totalCount} pending order
            {totalCount !== 1 ? "s" : ""}
          </>
        )}
      </div>

      {/* Orders Table */}
      {orders.length > 0 && (
        <div className="bg-white shadow-md rounded-lg overflow-hidden mb-6">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-12">
                    Expand
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Customer
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Total
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Order Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {orders.map((order) => {
                  const isExpanded = expandedRows.has(order.id);
                  const isAccepting = acceptingOrders.has(order.id);

                  return (
                    <>
                      <tr key={order.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <button
                            onClick={() => toggleRowExpansion(order.id)}
                            className="text-blue-600 hover:text-blue-800 font-medium"
                            aria-label={isExpanded ? "Collapse" : "Expand"}
                          >
                            {isExpanded ? "▼" : "▶"}
                          </button>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">
                            {order.userEmail}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {formatCurrency(order.totalAmount)}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {formatDate(order.orderDate)}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <button
                            onClick={() => handleAcceptOrder(order.id)}
                            disabled={isAccepting}
                            className="bg-green-600 hover:bg-green-700 text-white font-medium py-2 px-4 rounded transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center gap-2"
                          >
                            {isAccepting && (
                              <svg
                                className="animate-spin h-4 w-4 text-white"
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 24 24"
                              >
                                <circle
                                  className="opacity-25"
                                  cx="12"
                                  cy="12"
                                  r="10"
                                  stroke="currentColor"
                                  strokeWidth="4"
                                ></circle>
                                <path
                                  className="opacity-75"
                                  fill="currentColor"
                                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                ></path>
                              </svg>
                            )}
                            {isAccepting ? "Accepting..." : "Accept"}
                          </button>
                        </td>
                      </tr>
                      {isExpanded && (
                        <tr key={`${order.id}-details`}>
                          <td colSpan={5} className="px-6 py-4 bg-gray-50">
                            <div className="space-y-4">
                              {/* Shipping Information */}
                              <div>
                                <h3 className="text-sm font-semibold text-gray-700 mb-2">
                                  Shipping Address
                                </h3>
                                <div className="text-sm text-gray-600">
                                  <p>{order.shippingAddress}</p>
                                  <p>
                                    {order.shippingCity}, {order.shippingState}{" "}
                                    {order.shippingPostalCode}
                                  </p>
                                  {order.trackingNumber && (
                                    <p className="mt-2">
                                      <span className="font-medium">
                                        Tracking Number:
                                      </span>{" "}
                                      {order.trackingNumber}
                                    </p>
                                  )}
                                </div>
                              </div>

                              {/* Order Line Items */}
                              <div>
                                <h3 className="text-sm font-semibold text-gray-700 mb-2">
                                  Order Items
                                </h3>
                                <table className="min-w-full bg-white border border-gray-200 rounded">
                                  <thead className="bg-gray-100">
                                    <tr>
                                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                                        Product
                                      </th>
                                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                                        Price
                                      </th>
                                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                                        Quantity
                                      </th>
                                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                                        Subtotal
                                      </th>
                                    </tr>
                                  </thead>
                                  <tbody className="divide-y divide-gray-200">
                                    {order.lineItems.map((item, index) => (
                                      <tr key={index}>
                                        <td className="px-4 py-2 text-sm text-gray-900">
                                          {item.productName}
                                        </td>
                                        <td className="px-4 py-2 text-sm text-gray-900">
                                          {formatCurrency(item.productPrice)}
                                        </td>
                                        <td className="px-4 py-2 text-sm text-gray-900">
                                          {item.quantity}
                                        </td>
                                        <td className="px-4 py-2 text-sm text-gray-900">
                                          {formatCurrency(
                                            item.productPrice * item.quantity,
                                          )}
                                        </td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                              </div>
                            </div>
                          </td>
                        </tr>
                      )}
                    </>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex justify-between items-center mt-6">
          <div className="flex gap-2">
            <button
              onClick={() => handlePageChange(page - 1)}
              disabled={page === 1}
              className="px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <button
              onClick={() => handlePageChange(page + 1)}
              disabled={page === totalPages}
              className="px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>

          <div className="text-sm text-gray-600">
            Page {page} of {totalPages}
          </div>

          <form onSubmit={handlePageJump} className="flex gap-2 items-center">
            <label htmlFor="jumpPage" className="text-sm text-gray-600">
              Jump to:
            </label>
            <input
              type="number"
              id="jumpPage"
              name="jumpPage"
              min="1"
              max={totalPages}
              defaultValue={page}
              className="border border-gray-300 rounded px-2 py-1 w-20 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
            <button
              type="submit"
              className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm"
            >
              Go
            </button>
          </form>
        </div>
      )}
    </div>
  );
}

export default AdminOrdersPage;
