import { Link } from "react-router-dom";

function UnauthorizedPage() {
  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center p-8">
        <h1 className="text-6xl font-bold text-red-600 mb-4">403</h1>
        <h2 className="text-3xl font-semibold mb-4">Access Denied</h2>
        <p className="text-gray-600 mb-8">
          You don't have permission to access this page.
        </p>
        <Link
          to="/"
          className="inline-block bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors"
        >
          Go to Home
        </Link>
      </div>
    </div>
  );
}

export default UnauthorizedPage;
