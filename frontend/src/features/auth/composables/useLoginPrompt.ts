import { useAuthGuard } from '@/features/auth/composables/useAuthGuard.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

export function useLoginPrompt() {
  const { runIfAuthenticated } = useAuthGuard()
  const modalStore = useModalStore()

  function showLoginForm(): void {
    modalStore.openLoginForm()
  }

  function runOrShowLoginForm(action: () => void): void {
    runIfAuthenticated(action, showLoginForm)
  }

  return { runOrShowLoginForm }
}
