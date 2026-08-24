<script setup lang="ts">
import NavigationLink from '@/app/components/NavigationLink.vue'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { computed } from 'vue'

const authStore = useAuthStore()
const { isAuthenticated, currentUserId } = storeToRefs(authStore)

const profileLink = computed(() => {
  if (isAuthenticated.value) return `/users/${currentUserId.value}`
  return '/auth/login'
})
</script>

<template>
  <nav aria-label="Mobile primary" class="mobile-navigation">
    <NavigationLink icon="home" label="Home" to="/" />

    <NavigationLink icon="search" label="Search" to="/search" />

    <BaseIconButton disabled label="Create a thread — coming later">
      <AppIcon name="plus" />
    </BaseIconButton>

    <NavigationLink icon="heart" label="Activity" to="/activity" />

    <NavigationLink icon="user" label="Profile" :to="profileLink" />
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
