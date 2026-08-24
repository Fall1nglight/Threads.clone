<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import { type FollowStatus } from '@/features/follow/followTypes.ts'

withDefaults(
  defineProps<{
    userId: string
    username: string
    initials: string
    bio: string
    followerCount: string
    followingCount: string
    verified?: boolean
    showFollowButton: boolean
    followStatus: FollowStatus | null
  }>(),
  {
    verified: false,
    showFollowButton: false,
  },
)

const emit = defineEmits<{
  handleFollow: []
}>()
</script>

<template>
  <header class="profile-header">
    <div class="profile-header__top">
      <div class="profile-header__identity">
        <div class="profile-header__name-row">
          <h1>{{ username }}</h1>
          <AppIcon v-if="verified" class="profile-header__verified" name="verified" />
        </div>
      </div>
      <BaseAvatar
        class="profile-header__avatar"
        :initials="initials"
        :label="`${username}'s avatar`"
        size="large"
      />
    </div>

    <p class="profile-header__bio">{{ bio }}</p>

    <div class="profile-header__footer">
      <nav aria-label="Profile connections" class="profile-header__stats">
        <RouterLink
          class="profile-header__connection"
          :to="{ name: 'user-followers', params: { userId } }"
        >
          {{ followerCount }} followers
        </RouterLink>
        <RouterLink
          class="profile-header__connection"
          :to="{ name: 'user-following', params: { userId } }"
        >
          {{ followingCount }} following
        </RouterLink>
      </nav>

      <div v-if="showFollowButton || $slots.actions" class="profile-header__actions">
        <BaseButton v-if="showFollowButton" @click="emit('handleFollow')" variant="secondary">
          {{ followStatus == null ? 'Follow' : followStatus == 0 ? 'Pending' : 'Following' }}
        </BaseButton>

        <slot name="actions" />
      </div>
    </div>
  </header>
</template>

<style scoped>
.profile-header {
  display: grid;
  padding: var(--space-6) var(--space-4) var(--space-4);
  gap: var(--space-4);
}

.profile-header__top,
.profile-header__footer {
  display: flex;
  align-items: center;
  gap: var(--space-4);
}

.profile-header__top {
  justify-content: space-between;
}

.profile-header__footer {
  justify-content: space-between;
  flex-wrap: wrap;
  row-gap: var(--space-2);
}

.profile-header__identity {
  flex: 1 1 auto;
  min-width: 0;
}

.profile-header__avatar {
  flex: 0 0 auto;
}

.profile-header__name-row {
  display: flex;
  align-items: center;
  gap: var(--space-1);
}

h1 {
  overflow: hidden;
  font-size: var(--font-size-page-title);
  line-height: 1.2;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.profile-header__connection {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
}

.profile-header__stats {
  display: flex;
  flex: 1 1 auto;
  min-width: 0;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.profile-header__connection {
  display: inline-flex;
  min-width: var(--control-height);
  min-height: var(--control-height);
  align-items: center;
  justify-content: center;
  text-decoration: none;
  white-space: nowrap;
}

.profile-header__connection:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.profile-header__actions {
  display: flex;
  flex: 0 0 auto;
  align-items: center;
  margin-inline-start: auto;
  gap: var(--space-2);
}

.profile-header__verified {
  width: 1.125rem;
  height: 1.125rem;
}

.profile-header__bio {
  max-width: 65ch;
  overflow-wrap: anywhere;
  text-wrap: pretty;
}

@media (max-width: 24rem) {
  .profile-header__stats {
    flex-basis: 100%;
  }

  .profile-header__actions {
    margin-inline-start: auto;
  }
}
</style>
