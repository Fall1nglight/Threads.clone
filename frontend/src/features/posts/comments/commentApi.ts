import { getApi } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import { getPaginatedUrl } from '@/shared/api/baseApi.ts'
import type {
  CommentDto,
  CreateCommentDto,
  UpdateCommentDto,
} from '@/features/posts/comments/commentTypes.ts'

const api = getApi('/posts')

const paths = {
  getComments: (postId: string) => `${postId}/comments`,
  addComment: (postId: string) => `${postId}/comments`,
  deleteComment: (postId: string, commentId: string) => `${postId}/comments/${commentId}`,
  updateComment: (postId: string, commentId: string) => `${postId}/comments/${commentId}`,
} as const

async function getComments(
  postId: string,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.getComments(postId), paginationDetails)
  const response = await api.get<PagedResponse<CommentDto>>(url)
  return response.data
}

async function addComment(postId: string, payload: CreateCommentDto) {
  const response = await api.post<CommentDto>(paths.addComment(postId), payload)
  return response.data
}

async function deleteComment(postId: string, commentId: string) {
  await api.delete(paths.deleteComment(postId, commentId))
}

async function updateComment(postId: string, payload: UpdateCommentDto) {
  await api.put(paths.updateComment(postId, payload.id), {
    content: payload.content,
  })
}

export default { getComments, addComment, deleteComment, updateComment }
