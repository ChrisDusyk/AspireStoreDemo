import { useEffect, useState, useCallback } from "react";
import { useAuth } from "react-oidc-context";
import type { Order, OrderStatus } from "../types/order";
import ErrorDisplay from "../components/ErrorDisplay";

interface PagedOrderResponse {
  orders: Order[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

const STATUS_OPTIONS = [
  { value: "", label: "All Statuses" },
  { value: "1", label: "Processing" },
  { value: "2", label: "Shipped" },
  { value: "3", label: "Delivered" },
];

const SORT_OPTIONS = [
  { value: "OrderDate", label: "Order Date" },
  { value: "Status", label: "Status" },
  { value: "UserEmail", label: "User Email" },
];

function ProcessingQueuePage() {
  const auth = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [updatingOrders, setUpdatingOrders] = useState<Set<string>>(new Set());
  const [secondsUntilRefresh, setSecondsUntilRefresh] = useState(60);

  // Filters
  const [statusFilter, setStatusFilter] = useState<string>("1"); // Default to Processing
  const [userEmailFilter, setUserEmailFilter] = useState<string>("");
  const [sortBy, setSortBy] = useState<string>("OrderDate");
  const [sortDescending, setSortDescending] = useState<boolean>(false);

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
        sortBy: sortBy,
        sortDescending: sortDescending.toString(),
      });

      if (statusFilter) {
        params.append("status", statusFilter);
      }

      if (userEmailFilter.trim()) {
        params.append("userEmail", userEmailFilter.trim());
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(
        `${apiBaseUrl}/api/admin/orders/processing-queue?${params}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        throw new Error("Failed to fetch processing queue orders.");
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
  }, [page, statusFilter, userEmailFilter, sortBy, sortDescending, auth.user]);

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

  const handleFilterChange = () => {
    setPage(1); // Reset to first page when filters change
    setSecondsUntilRefresh(60);
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

  const handleMarkAsShipped = async (orderId: string) => {
    setUpdatingOrders((prev) => new Set(prev).add(orderId));
    setError(null);

    try {
      const token = auth.user?.access_token;
      if (!token) {
        setError("No access token available");
        setUpdatingOrders((prev) => {
          const newSet = new Set(prev);
          newSet.delete(orderId);
          return newSet;
        });
        return;
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(
        `${apiBaseUrl}/api/admin/orders/${orderId}/ship`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}));
        throw new Error(errorData.error || "Failed to mark order as shipped");
      }

      // Remove order from list if it no longer matches current filters
      setOrders((prev) => prev.filter((order) => order.id !== orderId));
      setTotalCount((prev) => prev - 1);

      // Refresh to get updated list
      setSecondsUntilRefresh(60);
      fetchOrders();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Unknown error occurred while updating order.");
      }
    } finally {
      setUpdatingOrders((prev) => {
        const newSet = new Set(prev);
        newSet.delete(orderId);
        return newSet;
      });
    }
  };

  const handleMarkAsDelivered = async (orderId: string) => {
    setUpdatingOrders((prev) => new Set(prev).add(orderId));
    setError(null);

    try {
      const token = auth.user?.access_token;
      if (!token) {
        setError("No access token available");
        setUpdatingOrders((prev) => {
          const newSet = new Set(prev);
          newSet.delete(orderId);
          return newSet;
        });
        return;
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(
        `${apiBaseUrl}/api/admin/orders/${orderId}/deliver`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}));
        throw new Error(errorData.error || "Failed to mark order as delivered");
      }

      // Remove order from list if it no longer matches current filters
      setOrders((prev) => prev.filter((order) => order.id !== orderId));
      setTotalCount((prev) => prev - 1);

      // Refresh to get updated list
      setSecondsUntilRefresh(60);
      fetchOrders();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Unknown error occurred while updating order.");
      }
    } finally {
      setUpdatingOrders((prev) => {
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

  const getStatusLabel = (status: OrderStatus) => {
    const statusMap: Record<OrderStatus, string> = {
      0: "Pending",
      1: "Processing",
      2: "Shipped",
      3: "Delivered",
    };
    return statusMap[status] || "Unknown";
  };

  const getStatusColor = (status: OrderStatus) => {
    const colorMap: Record<OrderStatus, string> = {
      0: "bg-yellow-100 text-yellow-800",
      1: "bg-blue-100 text-blue-800",
      2: "bg-purple-100 text-purple-800",
      3: "bg-green-100 text-green-800",
    };
    return colorMap[status] || "bg-gray-100 text-gray-800";
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Order Processing Queue</h1>
        <button
          onClick={handleManualRefresh}
          disabled={loading}
          className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
        >
          Refresh ({secondsUntilRefresh}s)
        </button>
      </div>

      {error && <ErrorDisplay message={error} />}

      {/* Filters */}
      <div className="bg-white shadow-md rounded-lg p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Status Filter */}
          <div>
            <label
              htmlFor="statusFilter"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Status
            </label>
            <select
              id="statusFilter"
              value={statusFilter}
              onChange={(e) => {
                setStatusFilter(e.target.value);
                handleFilterChange();
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          {/* User Email Filter */}
          <div>
            <label
              htmlFor="userEmailFilter"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              User Email
            </label>
            <input
              type="text"
              id="userEmailFilter"
              value={userEmailFilter}
              onChange={(e) => setUserEmailFilter(e.target.value)}
              onBlur={handleFilterChange}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  handleFilterChange();
                }
              }}
              placeholder="Filter by email..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Sort By */}
          <div>
            <label
              htmlFor="sortBy"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Sort By
            </label>
            <select
              id="sortBy"
              value={sortBy}
              onChange={(e) => {
                setSortBy(e.target.value);
                handleFilterChange();
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {SORT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          {/* Sort Direction */}
          <div>
            <label
              htmlFor="sortDirection"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Sort Direction
            </label>
            <select
              id="sortDirection"
              value={sortDescending ? "desc" : "asc"}
              onChange={(e) => {
                setSortDescending(e.target.value === "desc");
                handleFilterChange();
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="asc">Ascending</option>
              <option value="desc">Descending</option>
            </select>
          </div>
        </div>
      </div>

      {/* Results Count */}
      <div className="mb-4 text-sm text-gray-600">
        {totalCount === 0 ? (
          <span className="font-medium text-green-600">No orders found</span>
        ) : (
          <>
            Showing {orders.length} of {totalCount} order
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
                    Status
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
                {orders.map((order) => (
                  <>
                    {/* Main Row */}
                    <tr key={order.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <button
                          onClick={() => toggleRowExpansion(order.id)}
                          className="text-blue-600 hover:text-blue-800 font-medium"
                        >
                          {expandedRows.has(order.id) ? "−" : "+"}
                        </button>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm font-medium text-gray-900">
                          {order.userEmail}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(order.status)}`}
                        >
                          {getStatusLabel(order.status)}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {formatCurrency(order.totalAmount)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(order.orderDate)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                        {order.status === 1 && (
                          <button
                            onClick={() => handleMarkAsShipped(order.id)}
                            disabled={updatingOrders.has(order.id)}
                            className="bg-purple-600 hover:bg-purple-700 text-white py-1 px-3 rounded transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
                          >
                            {updatingOrders.has(order.id)
                              ? "Updating..."
                              : "Mark as Shipped"}
                          </button>
                        )}
                        {order.status === 2 && (
                          <button
                            onClick={() => handleMarkAsDelivered(order.id)}
                            disabled={updatingOrders.has(order.id)}
                            className="bg-green-600 hover:bg-green-700 text-white py-1 px-3 rounded transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
                          >
                            {updatingOrders.has(order.id)
                              ? "Updating..."
                              : "Mark as Delivered"}
                          </button>
                        )}
                        {order.status === 3 && (
                          <span className="text-gray-500 italic">Complete</span>
                        )}
                      </td>
                    </tr>

                    {/* Expanded Row */}
                    {expandedRows.has(order.id) && (
                      <tr key={`${order.id}-details`}>
                        <td colSpan={6} className="px-6 py-4 bg-gray-50">
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Shipping Information */}
                            <div>
                              <h3 className="text-sm font-semibold text-gray-700 mb-2">
                                Shipping Information
                              </h3>
                              <div className="text-sm text-gray-600 space-y-1">
                                <p>{order.shippingAddress}</p>
                                <p>
                                  {order.shippingCity}, {order.shippingState}{" "}
                                  {order.shippingPostalCode}
                                </p>
                              </div>
                            </div>

                            {/* Line Items */}
                            <div>
                              <h3 className="text-sm font-semibold text-gray-700 mb-2">
                                Order Items
                              </h3>
                              <ul className="text-sm text-gray-600 space-y-1">
                                {order.lineItems.map((item) => (
                                  <li key={item.productId}>
                                    {item.productName} × {item.quantity} @{" "}
                                    {formatCurrency(item.productPrice)} ={" "}
                                    {formatCurrency(
                                      item.quantity * item.productPrice,
                                    )}
                                  </li>
                                ))}
                              </ul>
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between bg-white px-4 py-3 sm:px-6 rounded-lg shadow-md">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => handlePageChange(page - 1)}
              disabled={page === 1 || loading}
              className="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:bg-gray-100 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <button
              onClick={() => handlePageChange(page + 1)}
              disabled={page === totalPages || loading}
              className="relative ml-3 inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:bg-gray-100 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-700">
                Page <span className="font-medium">{page}</span> of{" "}
                <span className="font-medium">{totalPages}</span>
              </p>
            </div>
            <div className="flex items-center space-x-2">
              <button
                onClick={() => handlePageChange(page - 1)}
                disabled={page === 1 || loading}
                className="relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <form onSubmit={handlePageJump} className="flex items-center">
                <label htmlFor="jumpPage" className="sr-only">
                  Jump to page
                </label>
                <input
                  type="number"
                  id="jumpPage"
                  name="jumpPage"
                  min="1"
                  max={totalPages}
                  placeholder="Page"
                  className="w-20 px-2 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <button
                  type="submit"
                  className="ml-2 px-3 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
                >
                  Go
                </button>
              </form>
              <button
                onClick={() => handlePageChange(page + 1)}
                disabled={page === totalPages || loading}
                className="relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProcessingQueuePage;
