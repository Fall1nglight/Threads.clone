<script setup lang="ts">
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import userApi from '@/features/users/userApi.ts'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'
import { useFetch } from '@/shared/composables/useFetch.ts'

const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const { state: userState } = useFetch(loggedInUserId, (currentUserId) =>
  userApi.getUser(currentUserId!),
)

const user = computed(() => (userState.value.status === 'success' ? userState.value.item : null))

const createdAtUtc = computed(() => user.value?.createdAtUtc ?? null)
const updatedAtUtc = computed(() => user.value?.updatedAtUtc ?? null)
const { formattedDate: formattedCreatedAtUtc } = useDateFormatter(createdAtUtc)
const { formattedDate: formattedUpdatedAtUtc } = useDateFormatter(updatedAtUtc)
</script>

<template>
  <Transition name="async-fade" mode="out-in">
    <BaseLoader
      v-if="userState.status === 'loading'"
      key="profile-details-loader"
      message="Loading profile details..."
    />
    <BaseErrorMessage
      v-else-if="userState.status === 'error'"
      key="profile-details-error"
      message="Failed to load your profile details. Please try again later!"
    />
    <section
      v-else-if="user"
      key="profile-details-content"
      aria-labelledby="profile-details-heading"
      class="profile-details__section"
    >
      <header class="profile-details__section-header">
        <h2 id="profile-details-heading">Account details</h2>
        <p>Basic information associated with your account.</p>
      </header>

      <dl class="profile-details__details">
        <div class="profile-details__detail">
          <dt>User ID</dt>
          <dd class="profile-details__id">{{ user.id }}</dd>
        </div>

        <div class="profile-details__detail">
          <dt>Username</dt>
          <dd>{{ user.username }}</dd>
        </div>

        <div class="profile-details__detail">
          <dt>Created</dt>
          <dd>
            <time :datetime="user.createdAtUtc">
              {{ formattedCreatedAtUtc }}
            </time>
          </dd>
        </div>

        <div class="profile-details__detail">
          <dt>Last updated</dt>
          <dd>
            <time v-if="user.updatedAtUtc" :datetime="user.updatedAtUtc">
              {{ formattedUpdatedAtUtc }}
            </time>
            <span v-else>Never</span>
          </dd>
        </div>
      </dl>
    </section>
  </Transition>
</template>

<style scoped>
.profile-details__section {
  display: grid;
  padding: var(--space-6) var(--space-4);
  gap: var(--space-6);
  border-block-end: 1px solid var(--color-border);
}

.profile-details__section-header {
  display: grid;
  gap: var(--space-1);
}

.profile-details__section-header h2 {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.2;
}

.profile-details__section-header p {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-wrap: pretty;
}

.profile-details__details {
  display: grid;
}

.profile-details__detail {
  display: grid;
  min-width: 0;
  padding-block: var(--space-3);
  grid-template-columns: minmax(7rem, 10rem) minmax(0, 1fr);
  gap: var(--space-3);
  border-block-start: 1px solid var(--color-border);
}

.profile-details__detail:last-child {
  border-block-end: 1px solid var(--color-border);
}

.profile-details__detail dt {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  font-weight: var(--font-weight-semibold);
}

.profile-details__detail dd {
  min-width: 0;
  overflow-wrap: anywhere;
}

.profile-details__id {
  font-family: ui-monospace, SFMono-Regular, Consolas, 'Liberation Mono', monospace;
  font-size: var(--font-size-support);
}

@media (max-width: 35rem) {
  .profile-details__detail {
    grid-template-columns: 1fr;
    gap: var(--space-1);
  }
}
</style>
