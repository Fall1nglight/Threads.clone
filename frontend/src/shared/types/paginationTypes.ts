export type PagedResponse<T> = {
  items: T[]
  hasMore: boolean
  cursor: string | null
}

export type PagedRequest = {
  cursor?: string
  pageSize: number
}
