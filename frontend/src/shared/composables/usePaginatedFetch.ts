import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import { watch, ref, computed, onScopeDispose, toValue, type ComputedRef, type Ref } from 'vue'
import type { FetchSource, FetchSourceTuple } from '@/shared/composables/useFetch.ts'

export type PaginatedFetchState<T> =
  | { readonly status: 'idle' | 'initial-loading'; readonly items: null; readonly error: null }
  | {
      readonly status: 'success' | 'more-loading'
      items: T[]
      readonly cursor: string | null
      readonly hasMore: boolean
      readonly error: null
    }
  | { readonly status: 'initial-error'; readonly items: null; readonly error: unknown }
  | {
      readonly status: 'more-error'
      items: T[]
      readonly cursor: string | null
      readonly hasMore: boolean
      readonly error: unknown
    }

const PAGE_SIZE = 5

export type PaginatedFetch<TSource, TItem> = (
  sourceValue: TSource,
  request?: PagedRequest,
) => Promise<PagedResponse<TItem>>

export type PaginatedTupleFetch<TValues extends readonly unknown[], TItem> = (
  sourceValues: TValues,
  request?: PagedRequest,
) => Promise<PagedResponse<TItem>>

export type PaginatedFetchResult<TItem> = {
  state: Ref<PaginatedFetchState<TItem>>
  canLoadMore: ComputedRef<boolean>
  loadMore: () => Promise<void>
}

export function usePaginatedFetch<TValues extends readonly unknown[], TItem>(
  sources: FetchSourceTuple<TValues>,
  fetcher: PaginatedTupleFetch<TValues, TItem>,
): PaginatedFetchResult<TItem>
export function usePaginatedFetch<TSource, TItem>(
  source: FetchSource<TSource>,
  fetcher: PaginatedFetch<TSource, TItem>,
): PaginatedFetchResult<TItem>
export function usePaginatedFetch<TItem>(
  sourceOrSources: unknown,
  fetcher: unknown,
): PaginatedFetchResult<TItem> {
  const hasMultipleSources = Array.isArray(sourceOrSources)
  const sources = (
    hasMultipleSources ? sourceOrSources : [sourceOrSources]
  ) as readonly FetchSource<unknown>[]
  const sourceGetters = sources.map((source) => () => toValue(source))
  const fetchPage = fetcher as PaginatedFetch<unknown, TItem>

  const state = ref({
    status: 'idle',
    items: null,
    error: null,
  }) as Ref<PaginatedFetchState<TItem>>

  let activeSession = 0
  let currentSourceValue = resolveSourceValue(sourceGetters.map((sourceGetter) => sourceGetter()))
  let isDisposed = false

  function resolveSourceValue(values: readonly unknown[]) {
    return hasMultipleSources ? values : values[0]
  }

  const canLoadMore = computed(
    () => state.value.status === 'success' && state.value.hasMore && Boolean(state.value.cursor),
  )

  watch(
    sourceGetters,
    async (sourceValues) => {
      const session = ++activeSession
      const sourceValue = resolveSourceValue(sourceValues)
      currentSourceValue = sourceValue

      state.value = {
        status: 'initial-loading',
        items: null,
        error: null,
      }

      try {
        if (isDisposed || session !== activeSession) return

        const response = await fetchPage(sourceValue)

        if (isDisposed || session !== activeSession) return

        state.value = {
          status: 'success',
          items: response.items,
          hasMore: response.hasMore,
          cursor: response.cursor,
          error: null,
        }
      } catch (err) {
        if (isDisposed || session !== activeSession) return

        state.value = {
          status: 'initial-error',
          items: null,
          error: err,
        }
      }
    },
    { immediate: true },
  )

  async function loadMore() {
    if (state.value.status === 'more-loading') return
    if (state.value.status !== 'success') return
    if (!state.value.hasMore) return
    if (!state.value.cursor) return

    const prevState = state.value
    const session = activeSession
    const sourceValue = currentSourceValue

    const newRequest: PagedRequest = {
      pageSize: PAGE_SIZE,
      cursor: prevState.cursor,
    }

    state.value = {
      ...prevState,
      status: 'more-loading',
    }

    try {
      if (isDisposed || session !== activeSession) return

      const response = await fetchPage(sourceValue, newRequest)

      if (isDisposed || session !== activeSession) return

      state.value = {
        status: 'success',
        items: [...prevState.items, ...response.items],
        hasMore: response.hasMore,
        cursor: response.cursor,
        error: null,
      }
    } catch (err) {
      if (isDisposed || session !== activeSession) return

      state.value = {
        ...prevState,
        status: 'more-error',
        error: err,
      }
    }
  }

  onScopeDispose(() => {
    isDisposed = true
    activeSession++
  })

  return { state, canLoadMore, loadMore }
}
