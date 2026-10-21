import notificationApi from '@/features/activity/notificationApi.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { inject, provide, readonly, ref, watch, type InjectionKey, type Ref } from 'vue'
import { useRoute } from 'vue-router'

export type NotificationCountContext = {
  unseenCount: Readonly<Ref<number | null>>
  refreshUnseenCount: () => Promise<void>
  decrementUnseenCount: () => void
}

const notificationCountKey: InjectionKey<NotificationCountContext> = Symbol('notification-count')

export function provideNotificationCount() {
  const authStore = useAuthStore()
  const { loggedInUserId } = storeToRefs(authStore)
  const route = useRoute()
  const unseenCount = ref<number | null>(null)
  let refreshPromise: Promise<void> | null = null
  let authGeneration = 0

  function refreshUnseenCount() {
    if (refreshPromise) return refreshPromise

    const generation = authGeneration
    refreshPromise = (async () => {
      try {
        const response = await notificationApi.getUnseenNotificationCount()
        if (generation === authGeneration && loggedInUserId.value) {
          unseenCount.value = response.count
        }
      } catch {
        return
      } finally {
        if (generation === authGeneration) refreshPromise = null
      }
    })()

    return refreshPromise
  }

  function decrementUnseenCount() {
    if (unseenCount.value === null) return
    unseenCount.value--
  }

  watch(
    loggedInUserId,
    (userId) => {
      authGeneration++
      refreshPromise = null

      if (!userId) {
        unseenCount.value = null
        return
      }

      void refreshUnseenCount()
    },
    { immediate: true },
  )

  watch(
    () => route.name,
    (routeName, previousRouteName) => {
      if (routeName === 'activity' && previousRouteName !== undefined && loggedInUserId.value) {
        void refreshUnseenCount()
      }
    },
  )

  const context: NotificationCountContext = {
    unseenCount: readonly(unseenCount),
    refreshUnseenCount,
    decrementUnseenCount,
  }

  provide(notificationCountKey, context)
  return context
}

export function useNotificationCount() {
  const context = inject(notificationCountKey)
  if (!context) throw new Error('Notification count provider is missing.')
  return context
}
