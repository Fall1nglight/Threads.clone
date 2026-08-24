<script setup lang="ts">
import PostCard from '@/features/posts/components/PostCard.vue'
import ProfileHeader from '@/features/users/components/ProfileHeader.vue'
import ProfileTabs from '@/features/users/components/ProfileTabs.vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useTemplateRef, watch } from 'vue'
import { useFetch } from '@/shared/composables/fetch.ts'
import userApi from '@/features/users/userApi.ts'
import postApi from '@/features/posts/postApi.ts'
import likeApi from '@/features/posts/likeApi.ts'
import FeedLoadMoreTrigger from '@/features/posts/components/FeedLoadMoreTrigger.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { FollowStatus } from '@/features/follow/followTypes.ts'
import followApi from '@/features/follow/followApi.ts'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import blockApi from '@/features/block/blockApi.ts'

const router = useRouter()
const route = useRoute()

const authStore = useAuthStore()
const { user } = storeToRefs(authStore)

const userId = computed<string>(() => String(route.params.userId))
const isProfileActionsOpen = ref(false)
const profileActions = useTemplateRef<HTMLElement>('profileActions')

function toggleProfileActions() {
  isProfileActionsOpen.value = !isProfileActionsOpen.value
}

function closeProfileActions(restoreFocus = false) {
  isProfileActionsOpen.value = false

  if (restoreFocus) {
    nextTick(() => profileActions.value?.querySelector<HTMLButtonElement>('button')?.focus())
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isProfileActionsOpen.value) return
  if (!(event.target instanceof Node)) return
  if (profileActions.value?.contains(event.target)) return

  closeProfileActions()
}

onMounted(() => document.addEventListener('pointerdown', handleDocumentPointerDown))
onBeforeUnmount(() => document.removeEventListener('pointerdown', handleDocumentPointerDown))
watch(userId, () => closeProfileActions())

const {
  status: profileStatus,
  data: profile,
  error: profileError,
} = useFetch(() => userApi.getUser(userId.value))

const {
  status: postStatus,
  data: posts,
  error: postError,
} = useFetch(() => postApi.getPostsForUser(userId.value))

async function handleLike(id: string, nextLikeState: boolean) {
  const post = posts.value?.items.find((post) => post.id === id)
  if (!post) return

  try {
    if (nextLikeState) {
      await likeApi.likePost(post.id)
    } else {
      await likeApi.unlikePost(post.id)
    }

    if (nextLikeState == post.isLikedByCurrentUser) return

    post.isLikedByCurrentUser = nextLikeState
    post.likeCount = Math.max(0, post.likeCount + (nextLikeState ? 1 : -1))
  } catch (err) {
    console.error(err)
  }
}

async function loadMore() {
  if (!posts.value?.hasMore) return
  if (!posts.value?.cursor) return

  try {
    const response = await postApi.getPostsForUser(userId.value, {
      pageSize: 5,
      cursor: posts.value.cursor,
    })

    posts.value.items.push(...response.items)
    posts.value.hasMore = response.hasMore
    posts.value.cursor = response.cursor ?? null
  } catch (err) {
    console.error(err)
  }
}

async function handleFollow() {
  if (!profile.value) return

  try {
    switch (profile.value.followStatusWithCurrentUser) {
      // Pending => already sent follow request
      // action => remove outgoing Pending follow request
      case FollowStatus.Pending:
        await followApi.removeFollowedUser(userId.value)
        profile.value.followStatusWithCurrentUser = null
        profile.value.followerCount = Math.max(0, profile.value.followerCount - 1)
        break

      case FollowStatus.Accepted:
        await followApi.removeFollowedUser(userId.value)
        profile.value.followStatusWithCurrentUser = null
        profile.value.followerCount = Math.max(0, profile.value.followerCount - 1)
        break

      case null:
        const result = await followApi.sendFollowRequest(userId.value)
        profile.value.followStatusWithCurrentUser = result.status
        if (result.status == FollowStatus.Accepted) profile.value.followerCount++
        break
    }
  } catch (err) {
    console.error(err)
  }
}

async function handleBlock() {
  if (!confirm('Are you sure you want to block this user?')) return
  closeProfileActions(true)
  if (!profile.value) return

  try {
    await blockApi.blockUser(profile.value.id)
    router.push('/')
  } catch (err) {
    console.error(err)
  }
}
</script>

<template>
  <div v-if="profileStatus == 'loading'">
    <h1>Loading...</h1>
  </div>
  <div v-else-if="profileStatus == 'error'">
    <h1>Failed to load user profile.</h1>
  </div>
  <div v-else-if="profile">
    <ProfileHeader
      :user-id="profile.id"
      :username="profile.username"
      :initials="profile.username.slice(0, 2)"
      :bio="profile.bio ?? ''"
      :follower-count="profile.followerCount + ''"
      :following-count="profile.followingCount + ''"
      :show-follow-button="profile.id !== user?.id"
      :follow-status="profile.followStatusWithCurrentUser"
      @handle-follow="handleFollow"
    >
      <template #actions>
        <div
          ref="profileActions"
          class="profile-actions"
          @keydown.esc.stop.prevent="closeProfileActions(true)"
        >
          <BaseIconButton
            aria-controls="profile-actions-panel"
            :aria-expanded="isProfileActionsOpen"
            :active="isProfileActionsOpen"
            label="More profile options"
            @click="toggleProfileActions"
          >
            <AppIcon name="more" />
          </BaseIconButton>

          <div
            v-if="isProfileActionsOpen"
            id="profile-actions-panel"
            aria-label="Profile actions"
            class="profile-actions__panel"
            role="group"
          >
            <RouterLink
              v-if="profile.id === user?.id"
              class="profile-actions__item"
              :to="{ name: 'blocked-users' }"
              @click="closeProfileActions()"
            >
              View blocked users
            </RouterLink>
            <button
              v-else
              class="profile-actions__item profile-actions__item--danger"
              type="button"
              @click="handleBlock"
            >
              Block user
            </button>
          </div>
        </div>
      </template>
    </ProfileHeader>

    <ProfileTabs />

    <section aria-label="Profile threads">
      <div v-if="posts?.items">
        <PostCard
          v-for="post in posts.items"
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
          @toggle-like="handleLike"
          @show-comments="router.push({ name: 'post-detail', params: { postId: post.id } })"
          detail
          threaded
        />

        <FeedLoadMoreTrigger v-if="posts?.hasMore && posts?.cursor" @visible="loadMore" />
      </div>
    </section>
  </div>
</template>

<style scoped>
.profile-actions {
  position: relative;
  display: flex;
}

.profile-actions__panel {
  position: absolute;
  z-index: 10;
  inset-block-start: calc(100% + var(--space-1));
  inset-inline-end: 0;
  min-width: 12rem;
  max-width: calc(100vw - var(--space-8));
  padding: var(--space-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  box-shadow: 0 var(--space-2) var(--space-6) rgb(0 0 0 / 18%);
}

.profile-actions__item {
  display: flex;
  width: 100%;
  min-height: var(--control-height);
  padding-inline: var(--space-3);
  align-items: center;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text);
  font: inherit;
  font-weight: var(--font-weight-semibold);
  text-align: start;
  text-decoration: none;
  white-space: nowrap;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.profile-actions__item--danger {
  color: var(--color-danger);
}

.profile-actions__item:hover {
  background: var(--color-surface-subtle);
}

.profile-actions__item:active {
  background: var(--color-border);
}
</style>
