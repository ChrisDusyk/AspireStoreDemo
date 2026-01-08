/* eslint-disable @typescript-eslint/no-unused-vars */
import React from "react";
import { Link } from "react-router-dom";

// Seattle Seahawks colors
const NAVY = "#002244";
const ACTION_GREEN = "#39FF14";
const WOLF_GREY = "#A5ACAF";
const WHITE = "#FFFFFF";

function Layout({ children }: { children: React.ReactNode }) {
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
            <button
              className={`ml-4 px-4 py-2 rounded bg-[${ACTION_GREEN}] text-[${NAVY}] font-semibold hover:bg-white hover:text-[${NAVY}] transition-colors`}
              style={{ backgroundColor: ACTION_GREEN, color: NAVY }}
              disabled
            >
              Login
            </button>
          </div>
        </div>
      </nav>
      <main className="pt-8">{children}</main>
    </div>
  );
}

export default Layout;
