import { getApi, getPaginatedUrl } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type { FollowStatusResponse, FollowUserDto } from '@/features/follow/followTypes.ts'

const api = getApi('/me')
const userApi = getApi('/users')

const paths = {
  getFollowers: (userId: string) => `/${userId}/followers`,
  getFollowing: (userId: string) => `/${userId}/following`,

  getIncomingFollowRequests: '/follow-requests/incoming',

  acceptIncomingFollowRequest: (followerId: string) =>
    `/follow-requests/incoming/${followerId}/accept`,

  rejectIncomingFollowRequest: (followerId: string) =>
    `/follow-requests/incoming/${followerId}/reject`,

  getOutgoingFollowRequests: '/follow-requests/outgoing',

  cancelOutgoingFollowRequest: (followedUserId: string) =>
    `/follow-requests/outgoing/${followedUserId}/cancel`,

  deleteFollower: (followerId: string) => `/followers/${followerId}`,
  deleteFollowing: (followedId: string) => `/following/${followedId}`,
  sendFollowRequest: (targetUserId: string) => `/following/${targetUserId}`,
} as const

async function getFollowers(
  userId: string,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.getFollowers(userId), paginationDetails)
  const response = await userApi.get<PagedResponse<FollowUserDto>>(url)
  return response.data
}

async function getFollowing(
  userId: string,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.getFollowing(userId), paginationDetails)
  const response = await userApi.get<PagedResponse<FollowUserDto>>(url)
  return response.data
}
async function getIncomingFollowRequests() {
  const response = await api.get<PagedResponse<FollowUserDto>>(paths.getIncomingFollowRequests)
  return response.data
}

async function acceptIncomingFollowRequest(followerId: string) {
  await api.post(paths.acceptIncomingFollowRequest(followerId))
}

async function rejectIncomingFollowRequest(followerId: string) {
  await api.post(paths.rejectIncomingFollowRequest(followerId))
}

async function getOutgoingFollowRequests() {
  const response = await api.get<PagedResponse<FollowUserDto>>(paths.getOutgoingFollowRequests)
  return response.data
}

async function cancelOutgoingFollowRequest(followedId: string) {
  await api.post(paths.cancelOutgoingFollowRequest(followedId))
}

async function sendFollowRequest(targetUserId: string) {
  const response = await api.post<FollowStatusResponse>(paths.sendFollowRequest(targetUserId))
  return response.data
}

async function deleteFollower(followerId: string) {
  await api.delete(paths.deleteFollower(followerId))
}

async function deleteFollowing(followedId: string) {
  await api.delete(paths.deleteFollowing(followedId))
}

export default {
  getFollowers,
  getFollowing,
  getIncomingFollowRequests,
  acceptIncomingFollowRequest,
  rejectIncomingFollowRequest,
  getOutgoingFollowRequests,
  cancelOutgoingFollowRequest,
  sendFollowRequest,
  deleteFollower,
  deleteFollowing,
}
