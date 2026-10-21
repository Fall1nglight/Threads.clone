import type { PostDto } from '@/features/posts/postTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import { getApi } from '@/shared/api/baseApi.ts'
import { getPaginatedUrl } from '@/shared/api/baseApi.ts'
import type { FeedKind } from '@/features/posts/feed/feedTypes.ts'

const api = getApi('/posts')

const paths = {
  anonymousFeed: '/anonymous-feed',
  personalFeed: '/personal-feed',
  globalFeed: '/global-feed',
} as const

async function getFeed(kind: FeedKind, paginationDetails: PagedRequest) {
  switch (kind) {
    case 'anonymous':
      return await getAnonymousFeed(paginationDetails)

    case 'personal':
      return await getPersonalFeed(paginationDetails)

    case 'global':
      return await getGlobalFeed(paginationDetails)

    default:
      throw new Error(`Unknown kind: ${kind}`)
  }
}

async function getAnonymousFeed(paginationDetails: PagedRequest) {
  const url = getPaginatedUrl(paths.anonymousFeed, paginationDetails)
  const response = await api.get<PagedResponse<PostDto>>(url)
  return response.data
}

async function getPersonalFeed(paginationDetails: PagedRequest) {
  const url = getPaginatedUrl(paths.personalFeed, paginationDetails)
  const response = await api.get<PagedResponse<PostDto>>(url)
  return response.data
}

async function getGlobalFeed(paginationDetails: PagedRequest) {
  const url = getPaginatedUrl(paths.globalFeed, paginationDetails)
  const response = await api.get<PagedResponse<PostDto>>(url)
  return response.data
}

export default { getFeed, getAnonymousFeed, getGlobalFeed, getPersonalFeed }
