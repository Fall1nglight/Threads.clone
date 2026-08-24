import { getApi } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type {
  CommentDto,
  CreateCommentDto,
  UpdateCommentDto,
} from '@/features/posts/comments/commentTypes.ts'

const api = getApi('/posts')

function createPagedUrlFor(path: string, paginationDetails: PagedRequest) {
  return `${path}?pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
}

async function getComments(postId: string, paginationDetails: PagedRequest = { pageSize: 5 }) {
  const path = postId + '/comments'
  const pathWithPagination = createPagedUrlFor(path, paginationDetails)
  const response = await api.get<PagedResponse<CommentDto>>(pathWithPagination)
  return response.data
}

async function addComment(postId: string, payload: CreateCommentDto) {
  const response = await api.post<CommentDto>(postId + '/comments', payload)
  return response.data
}

async function deleteComment(postId: string, commentId: string) {
  const response = await api.delete(postId + '/comments/' + commentId)
}

async function updateComment(postId: string, payload: UpdateCommentDto) {
  const response = await api.put(postId + '/comments/' + payload.id, { content: payload.content })
}

export default { getComments, addComment, deleteComment, updateComment }
