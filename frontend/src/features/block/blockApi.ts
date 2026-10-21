import { getApi } from '@/shared/api/baseApi.ts'
import type { BlockedUserDto } from '@/features/block/blockTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import { getPaginatedUrl } from '@/shared/api/baseApi.ts'

const api = getApi('/blocks')

const paths = {
  blockUser: (userId: string) => `/${userId}`,
  getBlockedUsers: '/',
  unblockUser: (userId: string) => `/${userId}`,
} as const

async function blockUser(userId: string) {
  await api.post(paths.blockUser(userId))
}

async function unblockUser(userId: string) {
  await api.delete(paths.unblockUser(userId))
}

async function getBlockedUsers(paginationDetails: PagedRequest = { pageSize: 5, cursor: null }) {
  const url = getPaginatedUrl(paths.getBlockedUsers, paginationDetails)
  const response = await api.get<PagedResponse<BlockedUserDto>>(url)
  return response.data
}

export default { blockUser, unblockUser, getBlockedUsers }
