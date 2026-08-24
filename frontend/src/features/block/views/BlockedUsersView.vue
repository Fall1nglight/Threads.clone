<script setup lang="ts">
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import PageHeader from '@/app/components/PageHeader.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { useFetch } from '@/shared/composables/fetch.ts'
import blockApi from '@/features/block/blockApi.ts'
import BlockedUserItem from '@/features/block/components/BlockedUserItem.vue'
import FeedLoadMoreTrigger from '@/features/posts/components/FeedLoadMoreTrigger.vue'

const authStore = useAuthStore()
const { currentUserId } = storeToRefs(authStore)
const backTo = computed(() => (currentUserId.value ? `/users/${currentUserId.value}` : '/'))

const {
  error: blockError,
  data: blockedUsers,
  status: blockStatus,
} = useFetch(() => blockApi.getBlockedUsers())

async function loadMore() {
  if (!blockedUsers.value?.hasMore) return
  if (!blockedUsers.value?.cursor) return

  try {
    const response = await blockApi.getBlockedUsers({
      cursor: blockedUsers.value.cursor,
      pageSize: 5,
    })

    blockedUsers.value.items.push(...response.items)
    blockedUsers.value.hasMore = response.hasMore
    blockedUsers.value.cursor = response.cursor ?? null
  } catch (err) {
    console.error(err)
  }
}

async function handleUnblock(targetUserId: string) {
  if (!blockedUsers.value) return
  await blockApi.unblockUser(targetUserId)
  blockedUsers.value.items = blockedUsers.value.items.filter((u) => u.id != targetUserId) || []
}
</script>

<template>
  <div>
    <PageHeader :back-to="backTo" title="Blocked users" />

    <section aria-label="Blocked users" class="blocked-users-list">
      <div v-if="blockedUsers?.items">
        <BlockedUserItem
          v-for="blockedUser in blockedUsers.items"
          :key="blockedUser.id"
          :user-id="blockedUser.id"
          :name="blockedUser.username"
          :username="blockedUser.username"
          :initials="blockedUser.username.slice(0, 2).toUpperCase()"
          :summary="blockedUser.bio ?? ''"
          @handle-unblock="handleUnblock(blockedUser.id)"
        />
      </div>

      <FeedLoadMoreTrigger
        v-if="blockedUsers?.hasMore && blockedUsers?.cursor"
        @visible="loadMore"
      />
    </section>
  </div>
</template>

<style scoped>
.blocked-users-list {
  padding: var(--space-4);
}
</style>
