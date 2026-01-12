import type { AuthProviderProps } from "react-oidc-context";

const keycloakUrl =
  import.meta.env.VITE_KEYCLOAK_URL || "http://localhost:8080";

export const oidcConfig: AuthProviderProps = {
  authority: `${keycloakUrl}/realms/copilotdemoapp`,
  client_id: "copilotdemoapp-frontend",
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
  response_type: "code",
  scope: "openid",
  automaticSilentRenew: true,
  loadUserInfo: true,
  onSigninCallback: () => {
    // Remove OIDC parameters from URL after successful login
    window.history.replaceState({}, document.title, window.location.pathname);
  },
};
