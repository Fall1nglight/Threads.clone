export type FollowUserDto = {
  id: string
  username: string
  isPrivate: boolean
  createdAtUtc: string
  followStatusWithCurrentUser?: FollowStatus | null
}

export enum FollowStatus {
  Pending = 0,
  Accepted = 1,
}

export type FollowStatusResponse = {
  status: FollowStatus
}
