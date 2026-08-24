import type { PostDto } from '@/features/posts/postTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import { getApi } from '@/shared/api/baseApi.ts'
import type { FeedKind } from '@/features/posts/feed/feedTypes.ts'

const api = getApi('/posts')

const routes = {
  anonymousFeed: '/anonymous-feed',
  personalFeed: '/personal-feed',
  globalFeed: '/global-feed',
}

function createPagedUrlFor(path: string, paginationDetails: PagedRequest) {
  return `${path}?pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
}

async function getFeed(kind: FeedKind, paginationDetails: PagedRequest) {
  switch (kind) {
    case 'anonymous':
      return await getAnonymousFeed(paginationDetails)

    case 'personal':
      return await getPersonalFeed(paginationDetails)

    case 'global':
      return await getGlobalFeed(paginationDetails)

    default:
      throw new Error('Unknown kind: ' + kind)
  }
}

async function getAnonymousFeed(paginationDetails: PagedRequest): Promise<PagedResponse<PostDto>> {
  const response = await api.get<PagedResponse<PostDto>>(
    createPagedUrlFor(routes.anonymousFeed, paginationDetails),
  )
  return response.data
}

async function getPersonalFeed(paginationDetails: PagedRequest): Promise<PagedResponse<PostDto>> {
  const response = await api.get<PagedResponse<PostDto>>(
    createPagedUrlFor(routes.personalFeed, paginationDetails),
  )
  return response.data
}

async function getGlobalFeed(paginationDetails: PagedRequest): Promise<PagedResponse<PostDto>> {
  const response = await api.get<PagedResponse<PostDto>>(
    createPagedUrlFor(routes.globalFeed, paginationDetails),
  )
  return response.data
}

export default { getFeed, getAnonymousFeed, getGlobalFeed, getPersonalFeed }
