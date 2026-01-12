import { Navigate } from "react-router-dom";
import { useAuth } from "react-oidc-context";
import { useUserRoles } from "../hooks/useUserRoles";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
}

function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const auth = useAuth();
  const roles = useUserRoles();

  if (auth.isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl">Loading authentication...</div>
      </div>
    );
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (requiredRole && !roles.includes(requiredRole)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;
