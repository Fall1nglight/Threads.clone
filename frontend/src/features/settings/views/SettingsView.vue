<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { RouterLink, RouterView } from 'vue-router'
import PageHeader from '@/app/components/PageHeader.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'

const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const settingsNavigations = [
  { label: 'Profile Details', routeName: 'profile-details' },
  { label: 'Profile Settings', routeName: 'profile-settings' },
  { label: 'Change Password', routeName: 'change-password' },
] as const
</script>

<template>
  <div>
    <PageHeader
      title="Settings"
      :back-to="{ name: 'user-profile', params: { userId: loggedInUserId } }"
    />

    <div class="settings">
      <aside class="settings__sidebar">
        <nav aria-label="Settings" class="settings__navigation">
          <RouterLink
            v-for="navigation in settingsNavigations"
            :key="navigation.routeName"
            :to="{ name: navigation.routeName }"
            class="settings__link"
            exact-active-class="settings__link--active"
          >
            {{ navigation.label }}
          </RouterLink>
        </nav>
      </aside>

      <div class="settings__content">
        <RouterView />
      </div>
    </div>
  </div>
</template>

<style scoped>
.settings {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
}
.settings__sidebar {
  padding: var(--space-3) var(--page-gutter);
  border-bottom: 1px solid var(--color-border);
}
.settings__navigation {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}
.settings__link {
  display: flex;
  flex: 1 1 auto;
  min-height: var(--control-height);
  align-items: center;
  justify-content: center;
  padding: var(--space-2) var(--space-3);
  border-radius: var(--radius-md);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-align: center;
}
.settings__link:hover {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}
.settings__link--active {
  background: var(--color-action);
  color: var(--color-action-contrast);
  font-weight: var(--font-weight-semibold);
}
.settings__content {
  min-width: 0;
}
</style>
