import { useAuth } from "react-oidc-context";

export function useUserRoles(): string[] {
  const auth = useAuth();

  if (!auth.user) return [];

  // Keycloak stores roles in realm_access.roles
  const realmAccess = auth.user.profile.realm_access as
    | { roles?: string[] }
    | undefined;
  return realmAccess?.roles || [];
}
