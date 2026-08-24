<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppLogo from '@/app/components/AppLogo.vue'
import AuthSessionMenu from '@/features/auth/components/AuthSessionMenu.vue'
</script>

<template>
  <header class="mobile-top-bar">
    <RouterLink aria-label="Home" class="mobile-top-bar__brand" to="/">
      <AppLogo />
    </RouterLink>
    <AuthSessionMenu placement="mobile" />
  </header>
</template>

<style scoped>
.mobile-top-bar {
  position: fixed;
  inset-block-start: 0;
  inset-inline: 0;
  z-index: 20;
  display: flex;
  height: var(--mobile-top-height);
  padding-inline: var(--space-3);
  align-items: center;
  justify-content: space-between;
  border-block-end: 1px solid var(--color-border);
  background: var(--color-surface);
}

.mobile-top-bar__brand {
  display: grid;
  width: 2.75rem;
  height: 2.75rem;
  place-items: center;
  border-radius: var(--radius-full);
}

@media (min-width: 48rem) {
  .mobile-top-bar {
    display: none;
  }
}
</style>
