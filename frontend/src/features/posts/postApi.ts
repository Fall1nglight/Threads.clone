import { getApi, getPaginatedUrl } from '@/shared/api/baseApi.ts'
import type { CreatePostDto, PostDto, UpdatePostDto } from '@/features/posts/postTypes.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'

const api = getApi('/posts')
const userApi = getApi('/users')

const paths = {
  getPost: (postId: string) => `/${postId}`,
  getUserPosts: (userId: string) => `/${userId}/posts`,
  addPost: '/',
  updatePost: (postId: string) => `/${postId}`,
  deletePost: (postId: string) => `/${postId}`,
} as const

async function getPost(id: string) {
  const response = await api.get<PostDto>(paths.getPost(id))
  return response.data
}

async function getPostsForUser(
  userId: string,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.getUserPosts(userId), paginationDetails)
  const response = await userApi.get<PagedResponse<PostDto>>(url)
  return response.data
}

async function addPost(payload: CreatePostDto) {
  const response = await api.post<PostDto>(paths.addPost, payload)
  return response.data
}

async function updatePost(payload: UpdatePostDto) {
  const response = await api.put(paths.updatePost(payload.id), { content: payload.content })
  return response.status == 204
}

async function deletePost(id: string) {
  await api.delete(paths.deletePost(id))
}

export default { getPost, getPostsForUser, addPost, updatePost, deletePost }
