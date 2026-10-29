import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import healthApi from '@/app/api/healthApi.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

export type AppStartupStatus = 'checking-service' | 'restoring-session' | 'unavailable' | 'ready'

const HEALTH_DEADLINE_MS = 30_000
const HEALTH_REQUEST_TIMEOUT_MS = 5_000
const HEALTH_RETRY_DELAY_MS = 1_000

function delay(milliseconds: number): Promise<void> {
  return new Promise((resolve) => {
    window.setTimeout(resolve, milliseconds)
  })
}

export const useAppStartupStore = defineStore('appStartup', () => {
  const authStore = useAuthStore()
  const modalStore = useModalStore()

  const status = ref<AppStartupStatus>('checking-service')
  const initializationPromise = ref<Promise<void> | null>(null)

  let retryResolver: (() => void) | null = null

  const isReady = computed(() => status.value === 'ready')
  const isUnavailable = computed(() => status.value === 'unavailable')

  async function waitForService(): Promise<void> {
    const deadline = Date.now() + HEALTH_DEADLINE_MS
    let lastError: unknown = new Error('The service did not become available.')

    while (Date.now() < deadline) {
      const remainingTime = deadline - Date.now()

      try {
        await healthApi.check(Math.min(HEALTH_REQUEST_TIMEOUT_MS, remainingTime))
        return
      } catch (error) {
        lastError = error

        const retryTimeRemaining = deadline - Date.now()
        if (retryTimeRemaining <= 0) break

        await delay(Math.min(HEALTH_RETRY_DELAY_MS, retryTimeRemaining))
      }
    }

    throw lastError
  }

  function waitForRetry(): Promise<void> {
    return new Promise((resolve) => {
      retryResolver = resolve
      modalStore.openServiceUnavailableModal(retry)
    })
  }

  function retry(): void {
    if (!retryResolver) return

    const resolveRetry = retryResolver
    retryResolver = null

    status.value = 'checking-service'
    modalStore.close()
    resolveRetry()
  }

  async function initializeCore(): Promise<void> {
    while (true) {
      status.value = 'checking-service'

      try {
        await waitForService()
        break
      } catch {
        status.value = 'unavailable'
        await waitForRetry()
      }
    }

    status.value = 'restoring-session'
    await authStore.ensureSessionReady()
    status.value = 'ready'
  }

  function ensureReady(): Promise<void> {
    initializationPromise.value ??= initializeCore()
    return initializationPromise.value
  }

  return {
    status,
    isReady,
    isUnavailable,
    ensureReady,
    retry,
  }
})
