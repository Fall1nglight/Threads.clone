import { getApi } from '@/shared/api/baseApi.ts'

const api = getApi('/posts')

const paths = {
  likePost: (postId: string) => `${postId}/likes`,
  unlikePost: (postId: string) => `${postId}/likes`,
} as const

async function likePost(id: string) {
  await api.post(paths.likePost(id))
}

async function unlikePost(id: string) {
  await api.delete(paths.unlikePost(id))
}

export default { likePost, unlikePost }
