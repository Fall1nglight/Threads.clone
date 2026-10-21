export enum NotificationType {
  Followed = 0,
  FollowRequestReceived = 1,
  FollowRequestAccepted = 2,
  PostLiked = 3,
  PostCommented = 4,
}

export enum NotificationSubjectKind {
  Post = 0,
}

export enum NotificationCategory {
  Follow = 'Follow',
  PostLiked = 'PostLiked',
  PostCommented = 'PostCommented',
}

export enum NotificationStatus {
  Seen = 'Seen',
  Unseen = 'Unseen',
}

export type NotificationUserDto = {
  id: string
  username: string
}

export type NotificationDto = {
  id: string
  triggeredByUser: NotificationUserDto
  type: NotificationType
  subjectKind: NotificationSubjectKind | null
  subjectId: string | null
  seenAtUtc: string | null
  createdAtUtc: string
}

export type UnseenNotificationCountDto = {
  count: number
}
