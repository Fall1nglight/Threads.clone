export type CommentDto = {
  id: string
  postId: string
  content: string
  user: CommentUserDto
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type CommentUserDto = {
  id: string
  username: string
}

export type CreateCommentDto = {
  content: string
}

export type UpdateCommentDto = {
  id: string
  content: string
}
