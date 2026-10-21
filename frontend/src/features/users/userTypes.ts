import type { FollowStatus } from '@/features/follow/followTypes.ts'

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
