import { getApi } from '@/shared/api/baseApi.ts'
import type { CreatePostDto, PostDto, UpdatePostDto } from '@/features/posts/postTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'

const api = getApi('/posts')

async function getPost(id: string) {
  const response = await api.get<PostDto>('/' + id)
  return response.data
}

async function getPostsForUser(userId: string, paginationDetails: PagedRequest = { pageSize: 5 }) {
  const path = `?userId=${userId}&pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
  const response = await api.get<PagedResponse<PostDto>>(path)
  return response.data
}

async function addPost(payload: CreatePostDto) {
  const response = await api.post<PostDto>('/', payload)
  return response.data
}

async function updatePost(payload: UpdatePostDto) {
  const response = await api.put('/' + payload.id, { content: payload.content })
  return response.status == 204
}

async function deletePost(id: string) {
  const response = await api.delete('/' + id)
}

export default { getPost, getPostsForUser, addPost, updatePost, deletePost }
