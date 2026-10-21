<script setup lang="ts">
import PageHeader from '@/app/components/PageHeader.vue'
import { computed } from 'vue'
import blockApi from '@/features/block/blockApi.ts'
import BlockedUserItem from '@/features/block/components/BlockedUserItem.vue'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import { usePaginatedFetch } from '@/shared/composables/usePaginatedFetch.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const modalStore = useModalStore()

// ========= BLOCKED USERS =========
// FETCH BLOCKED USERS
const {
  state: blockedUsersState,
  canLoadMore: canLoadMoreBlockedUsers,
  loadMore: loadMoreBlockedUsers,
} = usePaginatedFetch(null, (_source, request) => blockApi.getBlockedUsers(request))

const blockedUsers = computed(() =>
  'items' in blockedUsersState.value ? blockedUsersState.value.items : null,
)

// UNBLOCK USER
async function handleUnblock(targetUserId: string) {
  if (blockedUsersState.value.status !== 'success') return
  const _blockedUsers = blockedUsersState.value

  try {
    await blockApi.unblockUser(targetUserId)
    _blockedUsers.items = _blockedUsers.items.filter((u) => u.id !== targetUserId)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openAlertModal('Failed to unblock user. Please try again later!')
  }
}
</script>

<template>
  <div>
    <PageHeader :back-to="{ name: 'home' }" title="Blocked users" />

    <section aria-label="Blocked users" class="blocked-users-list">
      <Transition name="async-fade" mode="out-in">
        <BaseLoader
          v-if="blockedUsersState.status === 'initial-loading'"
          key="blocked-users-loader"
          message="Loading blocked users..."
        />

        <BaseErrorMessage
          v-else-if="blockedUsersState.status === 'initial-error'"
          key="blocked-users-error"
          message="Failed to load blocked users. Please try again later!"
        />

        <div v-else-if="blockedUsers" key="blocked-users-content">
          <TransitionGroup name="async-list-fade">
            <BlockedUserItem
              v-for="blockedUser in blockedUsers"
              :key="blockedUser.id"
              :user-id="blockedUser.id"
              :name="blockedUser.username"
              :username="blockedUser.username"
              :initials="getInitials(blockedUser.username)"
              :summary="blockedUser.bio"
              @handle-unblock="handleUnblock(blockedUser.id)"
            />
          </TransitionGroup>

          <BaseEmptyMessage
            v-if="!blockedUsers.length"
            message="You have not blocked any users yet."
          />

          <PaginationObserver v-if="canLoadMoreBlockedUsers" @visible="loadMoreBlockedUsers" />

          <Transition name="async-fade" mode="out-in">
            <BaseLoader
              v-if="blockedUsersState.status === 'more-loading'"
              key="blocked-users-more-loader"
              message="Loading more blocked users..."
            />

            <BaseErrorMessage
              v-else-if="blockedUsersState.status === 'more-error'"
              key="blocked-users-more-error"
              message="Failed to load more blocked users. Please try again later!"
            />
          </Transition>
        </div>
      </Transition>
    </section>
  </div>
</template>

<style scoped>
.blocked-users-list {
  padding: var(--space-4);
}
</style>
