<script setup lang="ts">
import { computed } from 'vue'
import { type RouteLocationRaw, RouterLink } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import { FollowStatus } from '@/features/follow/followTypes.ts'

const props = withDefaults(
  defineProps<{
    userId: string
    username: string
    initials: string
    bio: string | null
    followerCount: number
    followingCount: number
    verified?: boolean
    showButton: boolean
    followStatusWithCurrentUser: FollowStatus | null
  }>(),
  {
    initials: 'NA',
    bio: '',
    verified: false,
    showButton: false,
  },
)

// user-related computed properties
const userAvatarDesc = computed(() => `${props.username}'s avatar`)

// follow related computed properties
const followersPage = computed<RouteLocationRaw>(() => ({
  name: 'user-followers',
  params: { userId: props.userId },
}))
const followersPageDesc = computed(() => `View ${props.username}'s followers`)

const followingPage = computed<RouteLocationRaw>(() => ({
  name: 'user-following',
  params: { userId: props.userId },
}))
const followingPageDesc = computed(() => `View users followed by ${props.username}`)

const followButtonTitle = computed(() => {
  if (props.followStatusWithCurrentUser === null) return 'Follow user'
  return props.followStatusWithCurrentUser === FollowStatus.Pending
    ? 'Cancel follow request'
    : 'Unfollow user'
})

const followButtonText = computed(() => {
  if (props.followStatusWithCurrentUser === null) return 'Follow'
  return props.followStatusWithCurrentUser === FollowStatus.Pending ? 'Requested' : 'Following'
})

const emit = defineEmits<{
  handleFollow: []
}>()
</script>

<template>
  <header class="profile-header">
    <div v-if="$slots.actions" class="profile-header__actions">
      <slot name="actions" />
    </div>

    <BaseAvatar
      :initials="initials"
      :label="userAvatarDesc"
      class="profile-header__avatar"
      size="large"
    />

    <div class="profile-header__name-row">
      <div class="profile-header__identity">
        <h1>{{ username }}</h1>
        <p class="profile-header__handle">@{{ username }}</p>
      </div>

      <div v-if="verified" class="profile-header__name-adornments">
        <AppIcon v-if="verified" class="profile-header__verified" name="verified" />
      </div>
    </div>

    <p v-if="bio" class="profile-header__bio">{{ bio }}</p>

    <nav aria-label="Profile connections" class="profile-header__stats">
      <RouterLink
        :to="followersPage"
        :aria-label="followersPageDesc"
        class="profile-header__connection"
      >
        {{ followerCount }} followers
      </RouterLink>

      <RouterLink
        :to="followingPage"
        :aria-label="followingPageDesc"
        class="profile-header__connection"
      >
        {{ followingCount }} following
      </RouterLink>
    </nav>
    <BaseButton
      v-if="showButton"
      class="profile-header__follow"
      :variant="followStatusWithCurrentUser === null ? 'primary' : 'secondary'"
      :aria-label="followButtonTitle"
      @click="emit('handleFollow')"
      >{{ followButtonText }}</BaseButton
    >
  </header>
</template>

<style scoped>
.profile-header {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  padding: var(--space-8) var(--page-gutter) var(--space-4);
  gap: var(--space-3) var(--space-6);
  align-items: center;
}
.profile-header__identity {
  min-width: 0;
}
.profile-header__avatar {
  grid-column: 2;
  grid-row: 1 / span 2;
}
.profile-header__name-row {
  grid-column: 1;
  grid-row: 1;
  display: flex;
  align-items: center;
  gap: var(--space-2);
  min-width: 0;
}
h1 {
  font-size: 1.625rem;
  letter-spacing: -0.035em;
  line-height: 1.2;
  overflow-wrap: anywhere;
}
.profile-header__handle {
  margin-top: var(--space-2);
  font-size: var(--font-size-support);
  color: var(--color-text-muted);
}
.profile-header__name-adornments {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}
.profile-header__verified {
  width: 1.125rem;
  height: 1.125rem;
}
.profile-header__bio {
  grid-column: 1 / -1;
  max-width: 65ch;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}
.profile-header__stats {
  grid-column: 1 / -1;
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-4);
}
.profile-header__connection {
  display: inline-flex;
  min-height: 2.75rem;
  align-items: center;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-decoration: none;
}
.profile-header__connection:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}
.profile-header__actions {
  grid-column: 2;
  grid-row: 5;
  display: flex;
  justify-content: flex-end;
}
.profile-header__follow {
  grid-column: 1;
  grid-row: 5;
  width: 100%;
}
</style>
