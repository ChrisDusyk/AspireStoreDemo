import { useEffect, useState, type FormEvent } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { useAuth } from "react-oidc-context";
import type { ProductResponse } from "../types/product";
import ErrorDisplay from "../components/ErrorDisplay";

function EditProductPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const auth = useAuth();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Form fields
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [isActive, setIsActive] = useState(true);

  // Track original values for dirty detection
  const [originalValues, setOriginalValues] = useState({
    name: "",
    description: "",
    price: "",
    isActive: true,
  });

  const isDirty =
    name !== originalValues.name ||
    description !== originalValues.description ||
    price !== originalValues.price ||
    isActive !== originalValues.isActive;

  useEffect(() => {
    async function fetchProduct() {
      if (!id) {
        setError("Product ID is missing");
        setLoading(false);
        return;
      }

      try {
        const apiBaseUrl = import.meta.env.VITE_API_URL || "";
        const res = await fetch(`${apiBaseUrl}/api/products/${id}`);
        if (!res.ok) {
          if (res.status === 404) {
            setError("Product not found");
          } else {
            setError("Failed to fetch product");
          }
          setLoading(false);
          return;
        }

        const product: ProductResponse = await res.json();
        const nameValue = product.name || "";
        const descValue = product.description || "";
        const priceValue = product.price?.toString() || "";

        setName(nameValue);
        setDescription(descValue);
        setPrice(priceValue);
        setIsActive(product.isActive);

        setOriginalValues({
          name: nameValue,
          description: descValue,
          price: priceValue,
          isActive: product.isActive,
        });
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load product");
      } finally {
        setLoading(false);
      }
    }

    fetchProduct();
  }, [id]);

  const handleCancel = () => {
    if (isDirty) {
      const confirmed = window.confirm(
        "You have unsaved changes. Are you sure you want to leave?"
      );
      if (!confirmed) return;
    }
    navigate("/admin/products");
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSaving(true);

    try {
      const token = auth.user?.access_token;
      if (!token) {
        setError("No access token available");
        setSaving(false);
        return;
      }

      const apiBaseUrl = import.meta.env.VITE_API_URL || "";
      const res = await fetch(`${apiBaseUrl}/api/admin/products/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          name,
          description,
          price: parseFloat(price),
          isActive,
        }),
      });

      if (!res.ok) {
        const errorData = await res.json().catch(() => null);
        throw new Error(
          errorData?.error || `Failed to update product: ${res.status}`
        );
      }

      // Navigate back with success message
      navigate("/admin/products", {
        state: { successMessage: "Product updated successfully!" },
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update product");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="text-xl">Loading product...</div>
      </div>
    );
  }

  if (error && !name) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <ErrorDisplay message={error} />
        <Link
          to="/admin/products"
          className="mt-4 inline-block text-blue-600 hover:text-blue-800"
        >
          ← Back to Products
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="mb-6">
        <Link
          to="/admin/products"
          className="inline-flex items-center text-blue-600 hover:text-blue-800 font-medium"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="h-5 w-5 mr-1"
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path
              fillRule="evenodd"
              d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z"
              clipRule="evenodd"
            />
          </svg>
          Back to Products
        </Link>
      </div>

      <h1 className="text-3xl font-bold mb-6">Edit Product</h1>

      {error && <ErrorDisplay message={error} />}

      <form
        onSubmit={handleSubmit}
        className="bg-white shadow-md rounded-lg p-6"
      >
        <div className="space-y-6">
          <div>
            <label
              htmlFor="name"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Product Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              disabled={saving}
              className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-100"
            />
          </div>

          <div>
            <label
              htmlFor="description"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Description
            </label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={4}
              disabled={saving}
              className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-100"
            />
          </div>

          <div>
            <label
              htmlFor="price"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Price <span className="text-red-500">*</span>
            </label>
            <input
              type="number"
              id="price"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              required
              min="0.01"
              step="0.01"
              disabled={saving}
              className="w-full border border-gray-300 rounded px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-100"
            />
          </div>

          <div className="flex items-center">
            <input
              type="checkbox"
              id="isActive"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              disabled={saving}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded disabled:bg-gray-100"
            />
            <label
              htmlFor="isActive"
              className="ml-2 block text-sm font-medium text-gray-700"
            >
              Active
            </label>
          </div>
        </div>

        <div className="mt-8 flex gap-4">
          <button
            type="submit"
            disabled={saving}
            className="flex items-center justify-center px-6 py-2 bg-blue-600 text-white rounded font-medium hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
          >
            {saving ? (
              <>
                <svg
                  className="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
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
                Saving...
              </>
            ) : (
              "Save Product"
            )}
          </button>
          <button
            type="button"
            onClick={handleCancel}
            disabled={saving}
            className="px-6 py-2 bg-gray-200 text-gray-700 rounded font-medium hover:bg-gray-300 disabled:bg-gray-100 disabled:cursor-not-allowed transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

export default EditProductPage;
