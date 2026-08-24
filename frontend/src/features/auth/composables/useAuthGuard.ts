import { useAuthStore } from '@/features/auth/stores/authStore.ts'

export function useAuthGuard() {
  const authStore = useAuthStore()

  function runIfAuthenticated(action: () => void, onUnauthenticated: () => void): void {
    if (authStore.isAuthenticated) {
      action()
      return
    }

    onUnauthenticated()
  }

  return { runIfAuthenticated }
}
