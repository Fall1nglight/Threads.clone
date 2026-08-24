<script setup lang="ts">
import FeedHeader from '@/features/posts/components/FeedHeader.vue'
import PostCard from '@/features/posts/components/PostCard.vue'
import PostComposerPreview from '@/features/posts/components/PostComposerPreview.vue'
import { computed, ref, watch } from 'vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { type FeedState, type FeedStatus, useFeedStore } from '@/features/posts/feed/feedStore.ts'
import { storeToRefs } from 'pinia'
import type { FeedKind, SelectableFeedKind } from '@/features/posts/feed/feedTypes.ts'
import FeedLoadMoreTrigger from '@/features/posts/components/FeedLoadMoreTrigger.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const authStore = useAuthStore()
const feedStore = useFeedStore()
const { sessionStatus, isAuthenticated } = storeToRefs(authStore)

const selectedFeed = ref<SelectableFeedKind>('global')

const kind = computed<FeedKind | null>(() => {
  if (sessionStatus.value == 'uninitialized') return null
  if (sessionStatus.value == 'restoring') return null
  if (isAuthenticated.value) return selectedFeed.value

  return 'anonymous'
})

watch(
  kind,
  async (newKind: FeedKind | null) => {
    if (!newKind) return

    await feedStore.fetchInitialFeed(newKind)
  },
  { immediate: true },
)

const feed = computed<FeedState | null>(() => {
  if (!kind.value) return null
  return feedStore.feeds[kind.value]
})

const feedStatus = computed<FeedStatus>(() => {
  if (!kind.value) return 'idle'
  return feedStore.feeds[kind.value].status
})

async function loadMore() {
  if (!kind.value) return
  await feedStore.fetchMoreFeed(kind.value)
}
</script>

<template>
  <div>
    <FeedHeader v-model:selected-feed="selectedFeed" />

    <PostComposerPreview />

    <section aria-label="For you feed">
      <div v-if="feed">
        <PostCard
          v-for="post in feed.items"
          :key="post.id"
          :content="post.content"
          :post-id="post.id"
          :user-id="post.user.id"
          :author-name="post.user.username"
          :username="post.user.username"
          :avatar-initials="post.user.username.slice(0, 2)"
          :createdAtUtc="post.createdAtUtc"
          :updated-at-utc="post.updatedAtUtc"
          :comment-count="post.commentCount"
          :like-count="post.likeCount"
          :is-liked-by-current-user="post.isLikedByCurrentUser"
          @toggle-like="feedStore.setLike"
          @delete-post="feedStore.deletePost"
          @show-comments="router.push({ name: 'post-detail', params: { postId: post.id } })"
        />
      </div>

      <FeedLoadMoreTrigger v-if="feed?.hasMore && feed?.cursor" @visible="loadMore" />

      <p v-if="feedStatus == 'loading'">Loading...</p>
      <p v-else-if="feedStatus == 'error'">Error...</p>
    </section>
  </div>
</template>
