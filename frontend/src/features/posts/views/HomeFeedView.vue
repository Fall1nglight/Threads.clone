<script setup lang="ts">
import FeedHeader from '@/features/posts/components/FeedHeader.vue'
import PostCard from '@/features/posts/components/PostCard.vue'
import PostComposerPreview from '@/features/posts/components/PostComposerPreview.vue'
import { computed, ref, watch } from 'vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { useFeedStore } from '@/features/posts/feed/feedStore.ts'
import { storeToRefs } from 'pinia'
import type { FeedKind, SelectableFeedKind } from '@/features/posts/feed/feedTypes.ts'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import { useRouter } from 'vue-router'
import type { PostDto } from '@/features/posts/postTypes.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const router = useRouter()

const authStore = useAuthStore()
const modalStore = useModalStore()
const feedStore = useFeedStore()
const { isLoggedIn } = storeToRefs(authStore)

const selectedFeedKind = ref<SelectableFeedKind>('global')
const actualFeedKind = computed<FeedKind>(() =>
  isLoggedIn.value ? selectedFeedKind.value : 'anonymous',
)

const postsState = computed(() => feedStore.feeds[actualFeedKind.value])
const posts = computed(() => postsState.value.items)
const canLoadMorePosts = computed(
  () =>
    postsState.value.status === 'success' && postsState.value.hasMore && postsState.value.cursor,
)

function loadMorePosts() {
  return feedStore.fetchMoreFeed(actualFeedKind.value)
}

watch(
  actualFeedKind,
  (newFeedKind) => {
    // Reset before replacing the list so its load-more trigger cannot inherit the previous position.
    window.scrollTo({ left: 0, top: 0, behavior: 'instant' })
    return feedStore.fetchInitialFeed(newFeedKind)
  },
  { immediate: true },
)

function navigateToPost(postId: string) {
  router.replace({ name: 'post-detail', params: { postId } })
}

async function handleToggleLike(post: PostDto) {
  try {
    await feedStore.toggleLike(post)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openErrorModal(
      post.isLikedByCurrentUser
        ? 'Failed to unlike this post. Please try again later!'
        : 'Failed to like this post. Please try again later!',
    )
  }
}

async function handleDeletePost(postId: string) {
  try {
    await feedStore.deletePost(postId)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openErrorModal('Failed to delete post. Please try again later!')
  }
}
</script>

<template>
  <FeedHeader v-model:selected-feed="selectedFeedKind" />

  <PostComposerPreview />

  <section class="home-feed" aria-label="For you feed">
    <Transition name="async-fade" mode="out-in">
      <BaseLoader
        v-if="postsState.status === 'initial-loading'"
        key="initial-loader"
        message="Loading posts..."
      />

      <BaseErrorMessage
        v-else-if="postsState.status === 'initial-error'"
        key="initial-error"
        message="Failed to load posts. Please try again later!"
      />

      <div v-else key="feed-content">
        <TransitionGroup name="async-list-fade">
          <PostCard
            v-for="post in posts"
            :key="post.id"
            :content="post.content"
            :post-id="post.id"
            :user-id="post.user.id"
            :author-name="post.user.username"
            :username="post.user.username"
            :avatar-initials="getInitials(post.user.username)"
            :createdAtUtc="post.createdAtUtc"
            :updated-at-utc="post.updatedAtUtc"
            :comment-count="post.commentCount"
            :like-count="post.likeCount"
            :is-liked-by-current-user="post.isLikedByCurrentUser"
            @toggle-like="handleToggleLike(post)"
            @delete-post="handleDeletePost(post.id)"
            @show-comments="navigateToPost(post.id)"
          />
        </TransitionGroup>

        <BaseEmptyMessage v-if="!posts.length" message="There are no posts yet." />

        <PaginationObserver v-if="canLoadMorePosts" @visible="loadMorePosts" />

        <Transition name="async-fade" mode="out-in">
          <BaseLoader
            v-if="postsState.status === 'more-loading'"
            key="more-loader"
            message="Loading more posts..."
          />

          <BaseErrorMessage
            v-else-if="postsState.status === 'more-error'"
            key="more-error"
            message="Failed to load more posts. Please try again later!"
          />
        </Transition>
      </div>
    </Transition>
  </section>
</template>
