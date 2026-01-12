/* eslint-disable @typescript-eslint/no-unused-vars */
import React from "react";
import { Link } from "react-router-dom";
import { useAuth } from "react-oidc-context";
import { useHasRole } from "../hooks/useHasRole";
import { CartButton } from "./CartButton";

// Seattle Seahawks colors
const NAVY = "#002244";
const ACTION_GREEN = "#39FF14";
const WOLF_GREY = "#A5ACAF";
const WHITE = "#FFFFFF";

function Layout({ children }: { children: React.ReactNode }) {
  const auth = useAuth();
  const isAdmin = useHasRole("admin");
  const isUser = useHasRole("user");

  const handleLogin = () => {
    auth.signinRedirect();
  };

  const handleLogout = () => {
    auth.signoutRedirect();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav
        className="sticky top-0 z-50 w-full shadow-md"
        style={{ backgroundColor: NAVY }}
      >
        <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
          <div className="flex items-center">
            <span
              className="text-2xl font-bold tracking-tight"
              style={{ color: ACTION_GREEN }}
            >
              Aspire Store
            </span>
          </div>
          <div className="flex items-center space-x-6">
            <Link
              to="/"
              className={`text-white hover:text-[${ACTION_GREEN}] font-medium transition-colors`}
            >
              Home
            </Link>
            <Link
              to="/about"
              className={`text-white hover:text-[${ACTION_GREEN}] font-medium transition-colors`}
            >
              About
            </Link>

            {auth.isAuthenticated && (
              <>
                {isUser && (
                  <Link
                    to="/orders"
                    className={`text-white hover:text-[${ACTION_GREEN}] font-medium transition-colors`}
                  >
                    My Orders
                  </Link>
                )}
                {isAdmin && (
                  <Link
                    to="/admin/products"
                    className={`text-white hover:text-[${ACTION_GREEN}] font-medium transition-colors`}
                  >
                    Manage Products
                  </Link>
                )}
              </>
            )}

            <CartButton />

            {auth.isAuthenticated ? (
              <div className="flex items-center space-x-4">
                <span className="text-white text-sm">
                  {auth.user?.profile.email ||
                    auth.user?.profile.name ||
                    "User"}
                </span>
                <button
                  onClick={handleLogout}
                  className={`px-4 py-2 rounded bg-[${ACTION_GREEN}] text-[${NAVY}] font-semibold hover:bg-white hover:text-[${NAVY}] transition-colors`}
                  style={{ backgroundColor: ACTION_GREEN, color: NAVY }}
                >
                  Logout
                </button>
              </div>
            ) : (
              <button
                onClick={handleLogin}
                className={`ml-4 px-4 py-2 rounded bg-[${ACTION_GREEN}] text-[${NAVY}] font-semibold hover:bg-white hover:text-[${NAVY}] transition-colors`}
                style={{ backgroundColor: ACTION_GREEN, color: NAVY }}
              >
                Login
              </button>
            )}
          </div>
        </div>
      </nav>
      <main className="pt-8">{children}</main>
    </div>
  );
}

export default Layout;
