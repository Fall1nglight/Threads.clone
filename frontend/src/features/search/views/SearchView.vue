<script setup lang="ts">
import { computed, ref } from 'vue'
import { storeToRefs } from 'pinia'
import PageHeader from '@/app/components/PageHeader.vue'
import UserResultItem from '@/features/search/components/UserResultItem.vue'
import searchSchema from '@/features/search/schemas/searchSchema.ts'
import type { SearchForm } from '@/features/search/searchTypes.ts'
import userApi from '@/features/users/userApi.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { FollowStatus } from '@/features/follow/followTypes.ts'
import { useToggleFollow } from '@/features/follow/composables/useToggleFollow.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import type { PagedRequest } from '@/shared/types/paginationTypes.ts'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseTextField from '@/shared/components/BaseTextField.vue'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import type { PaginatedFetchState } from '@/shared/composables/usePaginatedFetch.ts'
import type { UserProfileDto } from '@/features/users/userTypes.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const PAGE_SIZE = 5

const authStore = useAuthStore()
const { isLoggedIn, loggedInUserId } = storeToRefs(authStore)

const searchState = ref<PaginatedFetchState<UserProfileDto>>({
  status: 'idle',
  items: null,
  error: null,
})

// guard to protect results being overwritten by old ones
let searchGeneration = 0

// stores the latest searched username
const activeUsername = ref<string>('')

const initialSearchForm: SearchForm = { username: '' }
const {
  payload: searchForm,
  errors: searchValidationErrors,
  submit: submitSearch,
  isSubmitting: isSubmittingSearch,
} = usePayloadValidator(initialSearchForm, searchSchema, performSearch)

const users = computed(() => searchState.value.items)

const canLoadMoreUsers = computed(
  () =>
    searchState.value.status === 'success' &&
    searchState.value.hasMore &&
    searchState.value.cursor !== null,
)

const messageIfNoMatch = computed<string>(() => `No match found for ${activeUsername.value}`)

async function performSearch(validPayload: SearchForm): Promise<void> {
  const currentGeneration = ++searchGeneration
  activeUsername.value = validPayload.username
  searchState.value = {
    status: 'initial-loading',
    items: null,
    error: null,
  }

  try {
    const response = await userApi.searchUsers(validPayload.username, {
      pageSize: PAGE_SIZE,
      cursor: null,
    })
    if (currentGeneration !== searchGeneration) return

    searchState.value = {
      status: 'success',
      items: response.items,
      cursor: response.cursor,
      hasMore: response.hasMore,
      error: null,
    }
  } catch (err) {
    if (currentGeneration !== searchGeneration) return

    searchState.value = {
      status: 'initial-error',
      items: null,
      error: err,
    }
  }
}

async function loadMoreUsers(): Promise<void> {
  if (searchState.value.status !== 'success') return
  if (!searchState.value.hasMore || !searchState.value.cursor) return

  const currentGeneration = searchGeneration
  const previousState = searchState.value
  const request: PagedRequest = {
    pageSize: PAGE_SIZE,
    cursor: previousState.cursor,
  }

  searchState.value = {
    ...previousState,
    status: 'more-loading',
  }

  try {
    const response = await userApi.searchUsers(activeUsername.value, request)
    if (currentGeneration !== searchGeneration) return

    searchState.value = {
      status: 'success',
      items: [...previousState.items, ...response.items],
      cursor: response.cursor,
      hasMore: response.hasMore,
      error: null,
    }
  } catch (err) {
    if (currentGeneration !== searchGeneration) return

    searchState.value = {
      ...previousState,
      status: 'more-error',
      error: err,
    }
  }
}

const { toggleFollow, isFollowPending } = useToggleFollow()

async function handleFollow(user: UserProfileDto): Promise<void> {
  if (searchState.value.status !== 'success') return

  const followResult = await toggleFollow(user)
  if (!followResult) return

  if (followResult.action === 'followed' && followResult.status === FollowStatus.Accepted) {
    user.followerCount++

    // in case the logged-in user is displayed in the search results, update the count
    searchState.value.items = searchState.value.items.map((u) =>
      u.id === loggedInUserId.value ? { ...u, followingCount: u.followingCount + 1 } : u,
    )

    return
  }

  if (followResult.action === 'removed' && followResult.previousStatus === FollowStatus.Accepted) {
    user.followerCount = Math.max(0, user.followerCount - 1)

    // in case the logged-in user is displayed in the search results, update the count
    searchState.value.items = searchState.value.items.map((u) =>
      u.id === loggedInUserId.value
        ? { ...u, followingCount: Math.max(0, u.followingCount - 1) }
        : u,
    )
  }
}

function getUserSummary(user: UserProfileDto): string {
  const followerLabel = user.followerCount === 1 ? 'follower' : 'followers'
  return `${user.followerCount} ${followerLabel} · ${user.followingCount} following`
}
</script>

<template>
  <div>
    <PageHeader title="Search" />

    <section aria-label="Search people" class="search-view">
      <form class="search-view__form" @submit.prevent="submitSearch">
        <BaseTextField
          v-model="searchForm.username"
          :error-message="searchValidationErrors.username"
          id="user-search"
          label="Username"
          placeholder="Search by username"
          type="search"
          visually-hidden-label
        />

        <BaseButton :is-loading="isSubmittingSearch" loading-text="Searching..." type="submit">
          Search
        </BaseButton>
      </form>

      <Transition name="async-fade" mode="out-in">
        <BaseLoader
          v-if="searchState.status === 'initial-loading'"
          key="search-users-loader"
          message="Searching users..."
        />

        <BaseErrorMessage
          v-else-if="searchState.status === 'initial-error'"
          key="search-users-error"
          message="Failed to search users. Please try again later!"
        />

        <div v-else-if="users" key="search-users-content" class="search-view__results">
          <section aria-label="Search results">
            <TransitionGroup name="async-list-fade" appear>
              <UserResultItem
                v-for="user in users"
                :key="user.id"
                :follow-status="user.followStatusWithCurrentUser"
                :initials="getInitials(user.username)"
                :is-follow-pending="isFollowPending(user.id)"
                :name="user.username"
                :show-follow-button="isLoggedIn && loggedInUserId !== user.id"
                :summary="getUserSummary(user)"
                :user-id="user.id"
                :username="user.username"
                @handle-follow="handleFollow(user)"
              />
            </TransitionGroup>

            <BaseEmptyMessage v-if="!users.length" :message="messageIfNoMatch" />

            <PaginationObserver v-if="canLoadMoreUsers" @visible="loadMoreUsers" />

            <Transition name="async-fade" mode="out-in">
              <BaseLoader
                v-if="searchState.status === 'more-loading'"
                key="search-users-more-loader"
                message="Loading more users..."
              />

              <BaseErrorMessage
                v-else-if="searchState.status === 'more-error'"
                key="search-users-more-error"
                message="Failed to load more users. Please try again later!"
              />
            </Transition>
          </section>
        </div>
      </Transition>
    </section>
  </div>
</template>

<style scoped>
.search-view {
  padding: var(--space-4);
}

.search-view__form {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: start;
  gap: var(--space-2);
}

.search-view__results {
  margin-block-start: var(--space-4);
}
</style>
