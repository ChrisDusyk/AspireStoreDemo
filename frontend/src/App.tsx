import { Routes, Route } from "react-router-dom";

import HomePage from "./pages/HomePage";
import AboutPage from "./pages/AboutPage";
import ProductDetailsPage from "./pages/ProductDetailsPage";
import AdminProductsPage from "./pages/AdminProductsPage";
import AdminOrdersPage from "./pages/AdminOrdersPage";
import ProcessingQueuePage from "./pages/ProcessingQueuePage";
import EditProductPage from "./pages/EditProductPage";
import CreateProductPage from "./pages/CreateProductPage";
import OrdersPage from "./pages/OrdersPage";
import UnauthorizedPage from "./pages/UnauthorizedPage";
import { CartPage } from "./pages/CartPage";
import CheckoutPage from "./pages/CheckoutPage";
import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoute";
import { CartProvider } from "./contexts/CartContext";

function App() {
  return (
    <CartProvider>
      <Layout>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/products/:id" element={<ProductDetailsPage />} />
          <Route
            path="/admin/products"
            element={
              <ProtectedRoute requiredRole="admin">
                <AdminProductsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/products/create"
            element={
              <ProtectedRoute requiredRole="admin">
                <CreateProductPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/products/:id/edit"
            element={
              <ProtectedRoute requiredRole="admin">
                <EditProductPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/orders"
            element={
              <ProtectedRoute requiredRole="admin">
                <AdminOrdersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/processing-queue"
            element={
              <ProtectedRoute requiredRole="admin">
                <ProcessingQueuePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/orders"
            element={
              <ProtectedRoute>
                <OrdersPage />
              </ProtectedRoute>
            }
          />
          <Route path="/unauthorized" element={<UnauthorizedPage />} />
        </Routes>
      </Layout>
    </CartProvider>
  );
}

export default App;
