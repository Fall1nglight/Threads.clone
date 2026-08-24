import { getApi } from '@/shared/api/baseApi.ts'
import type { BlockedUserDto } from '@/features/block/blockTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'

const api = getApi('/blocks')

const routes = {
  blockUser: (userId: string) => '/' + userId,
  getBlockedUsers: '/',
  unblockUser: (userId: string) => '/' + userId,
}

function createPagedUrlFor(path: string, paginationDetails: PagedRequest) {
  return `${path}?pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
}

async function blockUser(userId: string) {
  const response = await api.post(routes.blockUser(userId))
  return
}

async function unblockUser(userId: string) {
  const response = await api.delete(routes.unblockUser(userId))
  return
}

async function getBlockedUsers(paginationDetails: PagedRequest = { pageSize: 5 }) {
  const response = await api.get<PagedResponse<BlockedUserDto>>(
    createPagedUrlFor(routes.getBlockedUsers, paginationDetails),
  )
  return response.data
}

export default { blockUser, unblockUser, getBlockedUsers }
