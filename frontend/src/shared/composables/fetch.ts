import { shallowRef, readonly, watchEffect, ref } from 'vue'

export type FetchStatus = 'idle' | 'loading' | 'success' | 'error'
export type FetchFn<T> = () => Promise<T>
export function useFetch<T>(fetcher: FetchFn<T>) {
  const status = shallowRef<FetchStatus>('idle')
  const data = ref<T | null>(null)
  const error = shallowRef<unknown>(null)

  watchEffect(async () => {
    try {
      status.value = 'loading'
      data.value = await fetcher()
      status.value = 'success'
    } catch (err) {
      status.value = 'error'
      error.value = err
    }
  })

  return {
    status: readonly(status),
    data: data,
    error: readonly(error),
  }
}
