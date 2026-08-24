<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppLogo from '@/app/components/AppLogo.vue'
import NavigationLink from '@/app/components/NavigationLink.vue'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import AuthSessionMenu from '@/features/auth/components/AuthSessionMenu.vue'
import { storeToRefs } from 'pinia'

const authStore = useAuthStore()
const { isAuthenticated, currentUserId } = storeToRefs(authStore)
</script>

<template>
  <aside class="desktop-navigation">
    <RouterLink aria-label="Home" class="desktop-navigation__brand" to="/">
      <AppLogo />
    </RouterLink>

    <nav aria-label="Primary" class="desktop-navigation__links">
      <NavigationLink icon="home" label="Home" to="/" />

      <NavigationLink icon="search" label="Search" to="/search" />

      <BaseIconButton disabled label="Create a thread — coming later">
        <AppIcon name="plus" />
      </BaseIconButton>

      <NavigationLink icon="heart" label="Activity" to="/activity" />

      <NavigationLink
        v-if="isAuthenticated"
        icon="user"
        label="Profile"
        :to="'/users/' + currentUserId"
      />

      <NavigationLink v-if="!isAuthenticated" icon="user" label="Login" to="/auth/login" />
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
  padding: var(--space-6) var(--space-4);
  align-items: center;
  justify-content: space-between;
  border-inline-end: 1px solid var(--color-border);
  background: var(--color-page);
}

.desktop-navigation__brand {
  display: grid;
  width: 2.75rem;
  height: 2.75rem;
  place-items: center;
  border-radius: var(--radius-full);
}

.desktop-navigation__links {
  display: grid;
  width: 100%;
  justify-items: center;
  gap: var(--space-3);
}

@media (min-width: 48rem) {
  .desktop-navigation {
    display: flex;
    flex-direction: column;
  }
}

@media (min-width: 75rem) {
  .desktop-navigation {
    width: 13rem;
    align-items: stretch;
  }

  .desktop-navigation__brand {
    margin-inline-start: var(--space-2);
  }

  .desktop-navigation__links {
    justify-items: stretch;
  }
}
</style>
