import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  // Fallback server URL for local development when not running through Aspire
  const serverUrl =
    process.env.SERVER_HTTPS ||
    process.env.SERVER_HTTP ||
    "https://localhost:7158";

  return {
    plugins: [react(), tailwindcss()],
    server: {
      proxy: {
        // Proxy API calls to the app service
        "/api": {
          target: serverUrl,
          changeOrigin: true,
        },
        // Proxy OTLP telemetry to avoid CORS issues
        "/otlp": {
          target: serverUrl,
          changeOrigin: true,
          rewrite: (path) => {
            const rewritten = path.replace(/^\/otlp/, "/api/otlp");
            console.log(`[OTLP Rewrite] ${path} -> ${rewritten}`);
            return rewritten;
          },
          configure: (proxy) => {
            proxy.on("proxyReq", (proxyReq, req) => {
              console.log(
                `[OTLP Proxy] ${req.method} ${req.url} -> ${proxyReq.path}`,
              );
            });
          },
        },
      },
    },
    define: {
      __OTEL_EXPORTER_OTLP_ENDPOINT__: JSON.stringify("/otlp/v1/traces"),
      __OTEL_SERVICE_NAME__: JSON.stringify(
        env.OTEL_SERVICE_NAME || "frontend",
      ),
    },
  };
});
