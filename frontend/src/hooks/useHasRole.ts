import { useUserRoles } from "./useUserRoles";

export function useHasRole(role: string): boolean {
  const roles = useUserRoles();
  return roles.includes(role);
}
