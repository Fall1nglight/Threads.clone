import { getApi } from '@/shared/api/baseApi.ts'
import type { FollowStatus } from '@/features/follow/followTypes.ts'

const api = getApi('/users')

export type UserProfileDto = {
  id: string
  username: string
  isPrivate: boolean
  createdAtUtc: string
  updatedAtUtc: string | null
  bio: string | null
  followerCount: number
  followingCount: number
  followStatusWithCurrentUser: FollowStatus | null
}

export async function getUser(userId: string): Promise<UserProfileDto> {
  const response = await api.get<UserProfileDto>('/' + userId)
  return response.data
}

export default { getUser }
