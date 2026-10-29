<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppLogo from '@/app/components/AppLogo.vue'
import NavigationLink from '@/app/components/NavigationLink.vue'
import ActivityNavigationLink from '@/features/activity/components/ActivityNavigationLink.vue'
import CreateThreadButton from '@/app/components/CreateThreadButton.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import AuthSessionMenu from '@/features/auth/components/AuthSessionMenu.vue'
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
  <aside class="desktop-navigation">
    <RouterLink aria-label="Home" class="desktop-navigation__brand" to="/">
      <AppLogo />
    </RouterLink>

    <nav aria-label="Primary" class="desktop-navigation__links">
      <NavigationLink icon="home" label="Home" to="/" />
      <NavigationLink icon="search" label="Search" to="/search" />

      <CreateThreadButton />

      <ActivityNavigationLink />
      <NavigationLink v-if="isLoggedIn" icon="user" label="Profile" :to="loggedInUserPage" />
      <NavigationLink v-else icon="user" label="Login" to="/auth/login" />
    </nav>

    <AuthSessionMenu placement="desktop" />
  </aside>
</template>

<style scoped>
.desktop-navigation {
  position: fixed;
  inset-block: 0;
  inset-inline-start: 0;
  z-index: 20;
  display: none;
  width: var(--desktop-nav-width);
  padding: var(--space-4) var(--space-2);
  align-items: center;
  justify-content: space-between;
  background: var(--color-page);
}
.desktop-navigation__brand {
  display: grid;
  width: 3.5rem;
  height: 3.5rem;
  place-items: center;
  border-radius: var(--radius-lg);
  transition: background-color var(--transition-fast);
}
.desktop-navigation__brand:hover {
  background: var(--color-surface-subtle);
}
.desktop-navigation__brand :deep(.app-logo) {
  width: 2.75rem;
  height: 2.75rem;
}
.desktop-navigation__links {
  display: grid;
  width: 100%;
  justify-items: center;
  gap: var(--space-4);
}
.desktop-navigation__links > :deep(button) {
  width: 3.75rem;
  height: 3.25rem;
  border-radius: var(--radius-lg);
  background: var(--color-surface-subtle);
}
@media (min-width: 48rem) {
  .desktop-navigation {
    display: flex;
    flex-direction: column;
  }
}
</style>
