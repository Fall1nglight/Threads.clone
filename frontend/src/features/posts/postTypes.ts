export type PostDto = {
  id: string
  content: string
  user: PostUserDto
  likeCount: number
  commentCount: number
  isLikedByCurrentUser: boolean
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type PostUserDto = {
  id: string
  username: string
}

export type CreatePostDto = {
  content: string
}

export type UpdatePostDto = {
  id: string
  content: string
}
