import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";

import type { ProductResponse } from "../types/product";
import { useCartContext } from "../contexts/CartContext";

function ProductDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { addItem } = useCartContext();
  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [addedToCart, setAddedToCart] = useState(false);

  useEffect(() => {
    let isMounted = true;
    setLoading(true);
    setError(null);
    setNotFound(false);
    fetch(`/api/products/${id}`)
      .then(async (res) => {
        if (res.status === 404) {
          if (isMounted) setNotFound(true);
          return;
        }
        const data = await res.json();
        if (isMounted) setProduct(data);
      })
      .catch(() => {
        if (isMounted) setError("Failed to load product details.");
      })
      .finally(() => {
        if (isMounted) setLoading(false);
      });
    return () => {
      isMounted = false;
    };
  }, [id]);

  if (loading) {
    return (
      <div className="max-w-xl mx-auto mt-12 p-8 bg-white rounded shadow">
        <div className="animate-pulse">
          <div className="h-6 bg-gray-200 rounded w-1/3 mb-4" />
          <div className="h-4 bg-gray-100 rounded w-1/4 mb-2" />
          <div className="h-4 bg-gray-100 rounded w-2/3 mb-6" />
          <div className="h-8 bg-gray-200 rounded w-1/2 mb-8" />
          <div className="h-10 bg-gray-300 rounded w-1/3" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-xl mx-auto mt-12 p-8 bg-white rounded shadow text-center">
        <Link
          to="/"
          className="inline-block mb-6 px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
        >
          ← Back to Products
        </Link>
        <div className="text-red-600 font-semibold mb-4">{error}</div>
      </div>
    );
  }

  if (notFound) {
    return (
      <div className="max-w-xl mx-auto mt-12 p-8 bg-white rounded shadow text-center">
        <Link
          to="/"
          className="inline-block mb-6 px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
        >
          ← Back to Products
        </Link>
        <h2 className="text-2xl font-bold mb-4">Product Not Found</h2>
        <p className="text-gray-700 mb-6">
          Sorry, we couldn't find the product you're looking for.
        </p>
      </div>
    );
  }

  if (!product) return null;

  return (
    <div className="max-w-xl mx-auto mt-12 p-8 bg-white rounded shadow">
      <Link
        to="/"
        className="inline-block mb-6 px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
      >
        ← Back to Products
      </Link>
      <h1 className="text-3xl font-bold mb-2">
        {product.name ?? "Unnamed Product"}
      </h1>
      <p className="text-gray-700 mb-6">
        {product.description ?? "No description available."}
      </p>
      <div className="text-lg font-bold text-blue-600 mb-8">
        {product.price !== null
          ? `$${product.price.toFixed(2)}`
          : "Price not available"}
      </div>
      <button
        className="px-6 py-2 rounded bg-green-500 text-white font-semibold hover:bg-green-600 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
        onClick={() => {
          addItem(product.id);
          setAddedToCart(true);
          setTimeout(() => setAddedToCart(false), 2000);
        }}
      >
        {addedToCart ? "✓ Added to Cart" : "Add to Cart"}
      </button>
    </div>
  );
}

export default ProductDetailsPage;
