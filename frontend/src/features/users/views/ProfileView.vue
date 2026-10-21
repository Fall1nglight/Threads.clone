<script setup lang="ts">
import PostCard from '@/features/posts/components/PostCard.vue'
import ProfileHeader from '@/features/users/components/ProfileHeader.vue'
import ProfileTabs from '@/features/users/components/ProfileTabs.vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useTemplateRef, watch } from 'vue'
import { useFetch } from '@/shared/composables/useFetch.ts'
import { usePaginatedFetch } from '@/shared/composables/usePaginatedFetch.ts'
import userApi from '@/features/users/userApi.ts'
import postApi from '@/features/posts/postApi.ts'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { FollowStatus } from '@/features/follow/followTypes.ts'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import blockApi from '@/features/block/blockApi.ts'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import { useToggleLike } from '@/features/posts/composables/useToggleLike.ts'
import { useToggleFollow } from '@/features/follow/composables/useToggleFollow.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const { runOrShowLoginForm } = useLoginPrompt()

const router = useRouter()
const route = useRoute()
const modalStore = useModalStore()
const authStore = useAuthStore()
const { isLoggedIn, loggedInUserId } = storeToRefs(authStore)

const userId = computed(() => String(route.params.userId))
const { state: profileState } = useFetch(userId, (currentUserId) => userApi.getUser(currentUserId))
const profile = computed(() =>
  profileState.value.status === 'success' ? profileState.value.item : null,
)
const isOwnProfile = computed(() => profile.value?.id === loggedInUserId.value)

const { toggleLike } = useToggleLike()

// ========= POSTS =========
const {
  state: postsState,
  canLoadMore: canLoadMorePosts,
  loadMore: loadMorePosts,
} = usePaginatedFetch(userId, (currentUserId, request) =>
  postApi.getPostsForUser(currentUserId, request),
)

const posts = computed(() => ('items' in postsState.value ? postsState.value.items : null))

const messageIfNoPosts = computed(() =>
  isOwnProfile.value ? 'You have no posts yet.' : 'This user has no posts.',
)

// ========= EVENTS =========

const { toggleFollow } = useToggleFollow()

async function handleFollow() {
  if (!profile.value) return

  const followResult = await toggleFollow(profile.value)
  if (!followResult) return

  if (followResult.action === 'followed' && followResult.status === FollowStatus.Accepted) {
    profile.value.followerCount++
    return
  }

  // user has been unfollowed
  if (followResult.action === 'removed' && followResult.previousStatus === FollowStatus.Accepted) {
    profile.value.followerCount = Math.max(0, profile.value.followerCount - 1)
  }
}

async function handleBlock() {
  if (!confirm('Are you sure you want to block this user?')) return
  closeProfileActionsMenu(true)
  if (!profile.value) return

  try {
    await blockApi.blockUser(profile.value.id)
    await router.replace({ name: 'home' })
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openAlertModal('Failed to block user. Please try again later!')
  }
}

// ========= Helpers =========
function navigateToPost(postId: string) {
  router.replace({ name: 'post-detail', params: { postId } })
}

// ========= MENU =========
const isProfileActionsMenuOpen = ref(false)
const profileActionsMenu = useTemplateRef<HTMLElement>('profileActionsMenu')
const profileActionsMenuPanelId = 'visitedProfile-actions-panel'

function toggleProfileActionsMenu() {
  isProfileActionsMenuOpen.value = !isProfileActionsMenuOpen.value
}

function closeProfileActionsMenu(restoreFocus = false) {
  if (!isProfileActionsMenuOpen.value) return

  isProfileActionsMenuOpen.value = false

  if (restoreFocus) {
    nextTick(() => profileActionsMenu.value?.querySelector<HTMLButtonElement>('button')?.focus())
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isProfileActionsMenuOpen.value) return
  if (!(event.target instanceof Node)) return
  if (profileActionsMenu.value?.contains(event.target)) return

  closeProfileActionsMenu()
}

onMounted(() => document.addEventListener('pointerdown', handleDocumentPointerDown))
onBeforeUnmount(() => document.removeEventListener('pointerdown', handleDocumentPointerDown))
watch(userId, () => closeProfileActionsMenu())
watch(isLoggedIn, (isLoggedIn) => {
  if (!isLoggedIn) closeProfileActionsMenu()
})
</script>

<template>
  <Transition name="async-fade" mode="out-in">
    <BaseLoader
      v-if="profileState.status === 'loading'"
      key="profile-loader"
      message="Loading profile..."
    />

    <div
      v-else-if="profileState.status === 'error'"
      key="profile-error"
      class="profile-view__error"
    >
      <BaseErrorMessage message="Failed to load this profile. Please try again later!" />
    </div>

    <div v-else-if="profile" key="profile-content">
      <ProfileHeader
        :user-id="profile.id"
        :username="profile.username"
        :initials="getInitials(profile.username)"
        :bio="profile.bio"
        :follower-count="profile.followerCount"
        :following-count="profile.followingCount"
        :show-button="!isOwnProfile && isLoggedIn"
        :follow-status-with-current-user="profile.followStatusWithCurrentUser"
        @handle-follow="runOrShowLoginForm(handleFollow)"
      >
        <template v-if="isLoggedIn" #actions>
          <div
            ref="profileActionsMenu"
            class="profile-actions"
            @keydown.esc.stop.prevent="closeProfileActionsMenu(true)"
          >
            <BaseIconButton
              :aria-controls="profileActionsMenuPanelId"
              :aria-expanded="isProfileActionsMenuOpen"
              :active="isProfileActionsMenuOpen"
              label="More profile options"
              @click="toggleProfileActionsMenu"
            >
              <AppIcon name="more" />
            </BaseIconButton>

            <div
              v-if="isProfileActionsMenuOpen"
              :id="profileActionsMenuPanelId"
              aria-label="Profile actions"
              class="profile-actions__panel"
              role="group"
            >
              <RouterLink
                v-if="isOwnProfile"
                :to="{ name: 'blocked-users' }"
                aria-label="View blocked users"
                class="profile-actions__item"
                @click="closeProfileActionsMenu()"
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
        <Transition name="async-fade" mode="out-in">
          <BaseLoader
            v-if="postsState.status === 'initial-loading'"
            key="profile-posts-loader"
            message="Loading posts..."
          />

          <BaseErrorMessage
            v-else-if="postsState.status === 'initial-error'"
            key="profile-posts-error"
            message="Failed to load posts. Please try again later!"
          />

          <div v-else-if="posts" key="profile-posts-content">
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
                @toggle-like="toggleLike(post)"
                @show-comments="navigateToPost(post.id)"
                detail
                threaded
              />
            </TransitionGroup>

            <BaseEmptyMessage v-if="!posts.length" :message="messageIfNoPosts" />

            <PaginationObserver v-if="canLoadMorePosts" @visible="loadMorePosts" />

            <Transition name="async-fade" mode="out-in">
              <BaseLoader
                v-if="postsState.status === 'more-loading'"
                key="profile-posts-more-loader"
                message="Loading more posts..."
              />

              <BaseErrorMessage
                v-else-if="postsState.status === 'more-error'"
                key="profile-posts-more-error"
                message="Failed to load more posts. Please try again later!"
              />
            </Transition>
          </div>
        </Transition>
      </section>
    </div>
  </Transition>
</template>

<style scoped>
.profile-view__error {
  display: flow-root;
}

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
