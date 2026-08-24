import { getApi } from '@/shared/api/baseApi.ts'

const api = getApi('/posts')

async function likePost(id: string) {
  await api.post(id + '/likes')
}

async function unlikePost(id: string) {
  await api.delete(id + '/likes')
}

export default { likePost, unlikePost }
