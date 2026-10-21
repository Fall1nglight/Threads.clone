import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'

// calls given callback if the user is logged in, otherwise calls the other callback
export function useAuthGuard() {
  const authStore = useAuthStore()
  const { isLoggedIn } = storeToRefs(authStore)

  function runIfAuthenticated(action: () => void, onUnauthenticated: () => void): void {
    if (isLoggedIn.value) action()
    else onUnauthenticated()
  }

  return { runIfAuthenticated }
}
