import { useAuthGuard } from '@/features/auth/composables/useAuthGuard.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

// calls given callback if the user is logged in, otherwise opens the login form
export function useLoginPrompt() {
  const { runIfAuthenticated } = useAuthGuard()
  const modalStore = useModalStore()

  function runOrShowLoginForm(action: () => void): void {
    runIfAuthenticated(action, modalStore.openLoginForm)
  }

  return { runOrShowLoginForm }
}
