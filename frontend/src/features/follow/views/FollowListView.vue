<script setup lang="ts">
import { computed } from 'vue'
import { type RouteLocationRaw, useRoute } from 'vue-router'
import PageHeader from '@/app/components/PageHeader.vue'
import UserResultItem from '@/features/search/components/UserResultItem.vue'
import type { FollowUserDto } from '@/features/follow/followTypes.ts'
import { useFetch } from '@/shared/composables/useFetch.ts'
import followApi from '@/features/follow/followApi.ts'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import userApi from '@/features/users/userApi.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { useToggleFollow } from '@/features/follow/composables/useToggleFollow.ts'
import { usePaginatedFetch } from '@/shared/composables/usePaginatedFetch.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const props = defineProps<{
  listType: 'followers' | 'following'
}>()

const route = useRoute()
const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const visitedUserId = computed(() => String(route.params.userId))
const visitedUserPage = computed<RouteLocationRaw>(() => ({
  name: 'user-profile',
  params: { userId: visitedUserId.value },
}))

const { state: userState } = useFetch(visitedUserId, (currentUserId) =>
  userApi.getUser(currentUserId),
)
const user = computed(() => (userState.value.status === 'success' ? userState.value.item : null))
const visitedUserPageDesc = computed(() =>
  user.value ? `${user.value.username}'s ${props.listType}` : props.listType,
)

const usersSource = computed(() => ({
  userId: visitedUserId.value,
  listType: props.listType,
}))

const {
  state: usersState,
  canLoadMore: canLoadMoreUsers,
  loadMore: loadMoreUsers,
} = usePaginatedFetch(usersSource, (source, request) =>
  source.listType === 'followers'
    ? followApi.getFollowers(source.userId, request)
    : followApi.getFollowing(source.userId, request),
)

const users = computed(() => ('items' in usersState.value ? usersState.value.items : null))

const isOwnProfile = computed(() => loggedInUserId.value === visitedUserId.value)

const messageIfEmpty = computed(() => {
  if (isOwnProfile.value) {
    return `You have ${props.listType === 'followers' ? 'no followers' : 'not followed anyone'} yet.`
  } else {
    return `This user has ${props.listType === 'followers' ? 'no followers' : 'not followed anyone'} yet.`
  }
})

const { toggleFollow } = useToggleFollow()
async function handleFollow(targetUser: FollowUserDto) {
  const followResult = await toggleFollow(targetUser)
  if (!followResult) return
  if (usersState.value.status !== 'success') return
  if (isOwnProfile.value && props.listType === 'following' && followResult.action === 'removed') {
    usersState.value.items = usersState.value.items.filter((u) => u.id !== targetUser.id)
  }
}

function getUserSummary(user: FollowUserDto) {
  const joinedAt = new Intl.DateTimeFormat('en', {
    month: 'short',
    year: 'numeric',
  }).format(new Date(user.createdAtUtc))

  return user.isPrivate ? `Private account · Joined ${joinedAt}` : `Joined ${joinedAt}`
}
</script>

<template>
  <Transition name="async-fade" mode="out-in">
    <BaseLoader
      v-if="userState.status === 'loading' || usersState.status === 'initial-loading'"
      key="follow-list-loader"
      message="Loading...."
    />

    <div v-else>
      <PageHeader
        key="follow-list-header"
        :back-to="visitedUserPage"
        :title="visitedUserPageDesc"
      />

      <BaseErrorMessage
        v-if="userState.status === 'error' || usersState.status === 'initial-error'"
        key="follow-list-error"
        :message="`Failed to load ${listType}. Please try again later!`"
      />

      <section v-if="user" :aria-label="visitedUserPageDesc" class="follow-list">
        <div v-if="users">
          <TransitionGroup name="async-list-fade" appear>
            <UserResultItem
              v-for="user in users"
              :key="user.id"
              :initials="getInitials(user.username)"
              :name="user.username"
              :summary="getUserSummary(user)"
              :user-id="user.id"
              :username="user.username"
              :follow-status="user.followStatusWithCurrentUser"
              :show-follow-button="loggedInUserId !== null && loggedInUserId !== user.id"
              @handle-follow="handleFollow(user)"
            />
          </TransitionGroup>

          <BaseEmptyMessage
            v-if="!users.length"
            key="follow-list-empty"
            :message="messageIfEmpty"
          />

          <PaginationObserver v-if="canLoadMoreUsers" @visible="loadMoreUsers" />

          <BaseLoader
            v-if="usersState.status === 'more-loading'"
            key="follow-list-more-loader"
            :message="`Loading more ${listType}...`"
          />
        </div>
      </section>
    </div>
  </Transition>
</template>

<style scoped>
.follow-list {
  padding: var(--space-4);
}
</style>
