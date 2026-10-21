<script setup lang="ts">
import { computed } from 'vue'
import { type RouteLocationRaw, RouterLink } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
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

const followButtonIcon = computed<'user-add' | 'user-pending' | 'user-accepted'>(() => {
  if (props.followStatusWithCurrentUser === null) return 'user-add'
  return props.followStatusWithCurrentUser === FollowStatus.Pending
    ? 'user-pending'
    : 'user-accepted'
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
      </div>

      <div v-if="verified || showButton" class="profile-header__name-adornments">
        <AppIcon v-if="verified" class="profile-header__verified" name="verified" />

        <BaseIconButton
          v-if="showButton"
          :label="followButtonTitle"
          :title="followButtonTitle"
          @click="emit('handleFollow')"
        >
          <AppIcon :name="followButtonIcon" />
        </BaseIconButton>
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
  </header>
</template>

<style scoped>
.profile-header {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--space-6) var(--space-4) var(--space-4);
  gap: var(--space-1);
  text-align: center;
}

.profile-header__identity {
  grid-column: 2;
  min-width: 0;
}

.profile-header__avatar {
  align-self: center;
}

.profile-header__name-row {
  display: grid;
  width: 100%;
  max-width: 100%;
  min-height: var(--control-height);
  grid-template-columns: minmax(var(--control-height), 1fr) minmax(0, auto) minmax(
      var(--control-height),
      1fr
    );
  align-items: center;
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
  max-width: 100%;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
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
  position: absolute;
  inset-block-start: var(--space-4);
  inset-inline-end: var(--space-4);
  display: flex;
  align-items: center;
}

.profile-header__name-adornments {
  display: flex;
  grid-column: 3;
  align-items: center;
  justify-self: start;
  margin-inline-start: var(--space-2);
  gap: var(--space-1);
}

.profile-header__verified {
  width: 1.125rem;
  height: 1.125rem;
}

.profile-header__bio {
  width: 100%;
  max-width: 65ch;
  overflow-wrap: anywhere;
  text-align: center;
  text-wrap: pretty;
}
</style>
