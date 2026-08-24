<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import PageHeader from '@/app/components/PageHeader.vue'
import UserResultItem from '@/features/search/components/UserResultItem.vue'
import { FollowStatus, type FollowUserDto } from '@/features/follow/followTypes.ts'
import { useFetch } from '@/shared/composables/fetch.ts'
import followApi from '@/features/follow/followApi.ts'
import FeedLoadMoreTrigger from '@/features/posts/components/FeedLoadMoreTrigger.vue'
import type { PagedResponse } from '@/shared/types/paginationTypes.ts'
import userApi from '@/features/users/userApi.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'

const props = defineProps<{
  listType: 'followers' | 'following'
}>()

const route = useRoute()
const authStore = useAuthStore()
const { currentUserId } = storeToRefs(authStore)
const userId = computed(() => String(route.params.userId))
const backTo = computed(() => `/users/${userId.value}`)

const {
  status,
  data: users,
  error,
} = useFetch(() =>
  props.listType == 'followers'
    ? followApi.getFollowers(userId.value)
    : followApi.getFollowing(userId.value),
)

const {
  status: profileStatus,
  data: profile,
  error: profileError,
} = useFetch(() => userApi.getUser(userId.value))

const title = computed(() => `${profile.value?.username}'s ${props.listType}`)

async function loadMore() {
  if (!users.value?.hasMore) return
  if (!users.value?.cursor) return
  try {
    let response: PagedResponse<FollowUserDto>

    if (props.listType == 'followers') {
      response = await followApi.getFollowers(userId.value, {
        pageSize: 5,
        cursor: users.value.cursor,
      })
    } else {
      response = await followApi.getFollowing(userId.value, {
        pageSize: 5,
        cursor: users.value.cursor,
      })
    }

    users.value.items.push(...response.items)
    users.value.hasMore = response.hasMore
    users.value.cursor = response.cursor ?? null
  } catch (err) {
    console.error(err)
  }
}

async function handleFollow(targetUserId: string) {
  const profile = users.value?.items.find((user) => user.id === targetUserId)

  if (!profile) return

  try {
    switch (profile.followStatusWithCurrentUser) {
      // Pending => already sent follow request
      // action => remove outgoing Pending follow request
      case FollowStatus.Pending:
        await followApi.removeFollowedUser(profile.id)
        profile.followStatusWithCurrentUser = null
        if (!users.value?.items) return
        users.value.items = users.value?.items.filter((u) => u.id != profile.id) || []
        break

      case FollowStatus.Accepted:
        await followApi.removeFollowedUser(profile.id)
        profile.followStatusWithCurrentUser = null
        if (!users.value?.items) return
        users.value.items = users.value?.items.filter((u) => u.id != profile.id) || []
        break

      case null:
        const result = await followApi.sendFollowRequest(profile.id)
        profile.followStatusWithCurrentUser = result.status
        break
    }
  } catch (err) {
    console.error(err)
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
  <div>
    <PageHeader :back-to="backTo" :title="title" />

    <section :aria-label="title" class="follow-list">
      <div v-if="users?.items">
        <UserResultItem
          v-for="user in users.items"
          :key="user.id"
          :initials="user.username.slice(0, 2).toUpperCase()"
          :name="user.username"
          :summary="getUserSummary(user)"
          :user-id="user.id"
          :username="user.username"
          :follow-status="user.followStatusWithCurrentUser ?? null"
          :show-follow-button="currentUserId != user.id"
          @handle-follow="handleFollow(user.id)"
        />
      </div>

      <FeedLoadMoreTrigger v-if="users?.hasMore && users?.cursor" @visible="loadMore" />
    </section>
  </div>
</template>

<style scoped>
.follow-list {
  padding: var(--space-4);
}
</style>
