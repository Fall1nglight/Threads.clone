import { onScopeDispose, ref, toValue, watch, type MaybeRefOrGetter, type Ref } from 'vue'
import { sleep } from '@/shared/helpers/sleep.ts'

export type FetchState<T> =
  | { readonly status: 'idle' | 'loading'; readonly item: null; readonly error: null }
  | { readonly status: 'success'; item: T; readonly error: null }
  | { readonly status: 'error'; readonly item: null; readonly error: unknown }

export type Fetch<TSource, TItem> = (sourceValue: TSource) => Promise<TItem>

export type FetchSource<T> = MaybeRefOrGetter<T>

export type FetchSourceTuple<TValues extends readonly unknown[]> = {
  readonly [TIndex in keyof TValues]: FetchSource<TValues[TIndex]>
}

export type TupleFetch<TValues extends readonly unknown[], TItem> = (
  sourceValues: TValues,
) => Promise<TItem>

export type FetchResult<TItem> = {
  state: Ref<FetchState<TItem>>
}

export function useFetch<TValues extends readonly unknown[], TItem>(
  sources: FetchSourceTuple<TValues>,
  fetcher: TupleFetch<TValues, TItem>,
): FetchResult<TItem>
export function useFetch<TSource, TItem>(
  source: FetchSource<TSource>,
  fetcher: Fetch<TSource, TItem>,
): FetchResult<TItem>
export function useFetch<TItem>(sourceOrSources: unknown, fetcher: unknown): FetchResult<TItem> {
  const hasMultipleSources = Array.isArray(sourceOrSources)
  const sources = (
    hasMultipleSources ? sourceOrSources : [sourceOrSources]
  ) as readonly FetchSource<unknown>[]
  const sourceGetters = sources.map((source) => () => toValue(source))
  const fetchItem = fetcher as Fetch<unknown, TItem>

  const state = ref({
    status: 'idle',
    item: null,
    error: null,
  }) as Ref<FetchState<TItem>>

  let activeSession = 0
  let isDisposed = false

  function resolveSourceValue(values: readonly unknown[]) {
    return hasMultipleSources ? values : values[0]
  }

  watch(
    sourceGetters,
    async (sourceValues) => {
      const session = ++activeSession
      const sourceValue = resolveSourceValue(sourceValues)

      state.value = {
        status: 'loading',
        item: null,
        error: null,
      }

      try {
        await sleep(2000)
        if (isDisposed || session !== activeSession) return

        const item = await fetchItem(sourceValue)

        if (isDisposed || session !== activeSession) return

        state.value = {
          status: 'success',
          item,
          error: null,
        }
      } catch (error) {
        if (isDisposed || session !== activeSession) return

        state.value = {
          status: 'error',
          item: null,
          error,
        }
      }
    },
    { immediate: true },
  )

  onScopeDispose(() => {
    isDisposed = true
    activeSession++
  })

  return { state }
}
