<script setup lang="ts">
import NavigationLink from '@/app/components/NavigationLink.vue'
import ActivityNavigationLink from '@/features/activity/components/ActivityNavigationLink.vue'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { computed } from 'vue'

const authStore = useAuthStore()
const { isLoggedIn, loggedInUserId } = storeToRefs(authStore)

const loggedInUserPage = computed(() => ({
  name: 'user-profile',
  params: { userId: loggedInUserId.value },
}))
</script>

<template>
  <nav aria-label="Mobile primary" class="mobile-navigation">
    <NavigationLink icon="home" label="Home" to="/" />
    <NavigationLink icon="search" label="Search" to="/search" />

    <BaseIconButton disabled label="Create a thread — coming later">
      <AppIcon name="plus" />
    </BaseIconButton>

    <ActivityNavigationLink />
    <NavigationLink v-if="isLoggedIn" icon="user" label="Profile" :to="loggedInUserPage" />
    <NavigationLink v-else icon="user" label="Login" to="/auth/login" />
  </nav>
</template>

<style scoped>
.mobile-navigation {
  position: fixed;
  inset-block-end: 0;
  inset-inline: 0;
  z-index: 20;
  display: grid;
  min-height: calc(var(--mobile-bottom-height) + env(safe-area-inset-bottom));
  padding: var(--space-2) var(--space-3) env(safe-area-inset-bottom);
  grid-template-columns: repeat(5, 1fr);
  align-items: center;
  justify-items: center;
  border-block-start: 1px solid var(--color-border);
  background: var(--color-surface);
}

@media (min-width: 48rem) {
  .mobile-navigation {
    display: none;
  }
}
</style>
