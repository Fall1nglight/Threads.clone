import type { PostDto } from '@/features/posts/postTypes.ts'
import likeApi from '@/features/posts/likes/likeApi.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import { reactive } from 'vue'

export function useToggleLike() {
  const modalStore = useModalStore()
  const pendingLikeIds = reactive(new Set<string>())

  function isLikePending(postId: string): boolean {
    return pendingLikeIds.has(postId)
  }

  async function toggleLike(post: PostDto) {
    // prevent concurrent calls from executing
    if (pendingLikeIds.has(post.id)) return

    pendingLikeIds.add(post.id)

    const nextLikeState = !post.isLikedByCurrentUser

    try {
      if (post.isLikedByCurrentUser) {
        await likeApi.unlikePost(post.id)
      } else {
        await likeApi.likePost(post.id)
      }

      // prevent overriding desired likeState with old value
      if (post.isLikedByCurrentUser === nextLikeState) return

      post.isLikedByCurrentUser = nextLikeState
      post.likeCount = Math.max(0, post.likeCount + (nextLikeState ? 1 : -1))
    } catch (_err) {
      modalStore.openAlertModal(
        nextLikeState
          ? 'Failed to like this post. Please try again later!'
          : 'Failed to unlike this post. Please try again later!',
      )
    } finally {
      pendingLikeIds.delete(post.id)
    }
  }

  return { toggleLike, isLikePending }
}
