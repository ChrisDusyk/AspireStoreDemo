function AdminProductsPage() {
  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-8">Manage Products</h1>
      <p className="text-gray-600 mb-4">
        Admin-only product management interface. This page is protected and
        requires the 'admin' role.
      </p>
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <p className="text-sm text-yellow-800">
          <strong>TODO:</strong> Implement product CRUD interface with forms for
          creating, editing, and deleting products.
        </p>
      </div>
    </div>
  );
}

export default AdminProductsPage;
