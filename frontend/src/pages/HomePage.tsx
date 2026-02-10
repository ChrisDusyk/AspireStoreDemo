import { useEffect, useState, useCallback } from "react";
import { Link } from "react-router-dom";

import ErrorDisplay from "../components/ErrorDisplay";
import ProductImage from "../components/ProductImage";
import type { ProductResponse } from "../types/product";
import { useCartContext } from "../contexts/CartContext";

interface PagedProductResponse {
  products: ProductResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

const SKELETON_COUNT = 8;

function HomePage() {
  const { addItem } = useCartContext();
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState("");

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 2000);
    return () => {
      clearTimeout(handler);
    };
  }, [search]);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: "8",
      });
      if (debouncedSearch) params.append("name", debouncedSearch);
      const res = await fetch(`/api/products?${params}`);
      if (!res.ok) {
        throw new Error("Failed to fetch products.");
      }
      const data: PagedProductResponse = await res.json();
      setProducts(data.products);
      setTotalPages(data.totalPages);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Unknown error occurred.");
      }
      setProducts([]);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  }, [page, debouncedSearch]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPages) {
      setPage(newPage);
    }
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6 text-center">Store Products</h1>
      <div className="mb-6 flex justify-center">
        <input
          type="text"
          placeholder="Search products by name..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="border border-gray-300 rounded px-4 py-2 w-full max-w-md focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>
      {error && <ErrorDisplay message={error} />}
      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {[...Array(SKELETON_COUNT)].map((_, i) => (
            <div
              key={i}
              className="animate-pulse bg-gray-200 h-40 rounded shadow"
            />
          ))}
        </div>
      ) : products.length === 0 ? (
        <div className="text-center text-gray-500 mt-8">
          No products returned for search.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {products.map((product) => (
              <div
                key={product.id}
                className="bg-white rounded shadow overflow-hidden flex flex-col hover:ring-2 hover:ring-blue-400 transition"
              >
                <Link
                  to={`/products/${product.id}`}
                  className="flex flex-col flex-grow cursor-pointer"
                  tabIndex={0}
                  aria-label={`View details for ${
                    product.name ?? "Unnamed Product"
                  }`}
                >
                  <ProductImage
                    imageUrl={product.imageUrl}
                    alt={product.name ?? "Product"}
                    className="w-full h-48 object-cover"
                  />
                  <div className="p-6 flex flex-col flex-grow">
                    <div>
                      <h2 className="text-xl font-semibold mb-2">
                        {product.name ?? "Unnamed Product"}
                      </h2>
                      <p className="text-gray-600 mb-4">
                        {product.description ?? "No description."}
                      </p>
                    </div>
                    <div className="text-lg font-bold text-blue-600 mb-4 mt-auto">
                      {product.price !== null
                        ? `$${product.price.toFixed(2)}`
                        : "Price not available"}
                    </div>
                  </div>
                </Link>
                <div className="p-6 pt-0">
                  <button
                    onClick={() => addItem(product.id)}
                    className="w-full px-4 py-2 rounded bg-green-500 text-white font-semibold hover:bg-green-600 transition-colors"
                  >
                    Add to Cart
                  </button>
                </div>
              </div>
            ))}
          </div>
          <div className="flex justify-center items-center mt-8 space-x-2">
            <button
              className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50"
              onClick={() => handlePageChange(page - 1)}
              disabled={page === 1}
            >
              Previous
            </button>
            <span className="mx-2 text-gray-700">
              Page {page} of {totalPages}
            </span>
            <button
              className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50"
              onClick={() => handlePageChange(page + 1)}
              disabled={page === totalPages}
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default HomePage;
