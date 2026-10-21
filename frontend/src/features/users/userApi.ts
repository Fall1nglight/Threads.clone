import { getApi } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type { UserProfileDto } from '@/features/users/userTypes.ts'
import { getPaginatedUrl } from '@/shared/api/baseApi.ts'

const api = getApi('/users')

const paths = {
  getUser: (userId: string) => `/${userId}`,
  searchUsers: (username: string) => `/search?username=${username}`,
} as const

async function getUser(userId: string) {
  const response = await api.get<UserProfileDto>(paths.getUser(userId))
  return response.data
}

async function searchUsers(
  username: string,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.searchUsers(username), paginationDetails)
  const response = await api.get<PagedResponse<UserProfileDto>>(url)
  return response.data
}

export default { getUser, searchUsers }
