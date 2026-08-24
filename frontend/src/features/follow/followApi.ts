import { getApi } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type { FollowStatusResponse, FollowUserDto } from '@/features/follow/followTypes.ts'

const api = getApi('/follows')
const userApi = getApi('/users')

function createPagedUrlFor(path: string, paginationDetails: PagedRequest) {
  return `${path}?pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
}

const routes = {
  getFollowersFor: (userId: string) => '/' + userId + '/followers',
  getFollowingFor: (userId: string) => '/' + userId + '/following',
  getFollowing: '/following',
  getIncomingFollowRequests: '/requests/incoming',
  getOutgoingFollowRequests: '/requests/outgoing',
  sendFollowRequest: (targetUserId: string) => '/' + targetUserId,
  deleteFollowRequest: (followedUserId: string) => '/following/' + followedUserId,
}

async function getFollowers(userId: string, paginationDetails: PagedRequest = { pageSize: 5 }) {
  const url = routes.getFollowersFor(userId)
  const response = await userApi.get<PagedResponse<FollowUserDto>>(
    createPagedUrlFor(url, paginationDetails),
  )
  return response.data
}

async function getFollowing(userId: string, paginationDetails: PagedRequest = { pageSize: 5 }) {
  const url = routes.getFollowingFor(userId)
  const response = await userApi.get<PagedResponse<FollowUserDto>>(
    createPagedUrlFor(url, paginationDetails),
  )
  return response.data
}

async function getIncomingFollowRequests() {
  const response = await api.get<PagedResponse<FollowUserDto>>(routes.getIncomingFollowRequests)
  return response.data
}

async function getOutgoingFollowRequests() {
  const response = await api.get<PagedResponse<FollowUserDto>>(routes.getIncomingFollowRequests)
  return response.data
}

async function sendFollowRequest(targetUserId: string) {
  const response = await api.post<FollowStatusResponse>(routes.sendFollowRequest(targetUserId))
  return response.data
}

async function removeFollowedUser(followedUserId: string) {
  const response = await api.delete(routes.deleteFollowRequest(followedUserId))
  return
}

export default {
  getFollowers,
  getFollowing,
  getIncomingFollowRequests,
  getOutgoingFollowRequests,
  sendFollowRequest,
  removeFollowedUser,
}
