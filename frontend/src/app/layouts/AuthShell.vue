<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppLogo from '@/app/components/AppLogo.vue'
</script>

<template>
  <a class="skip-link" href="#auth-content">Skip to account form</a>
  <main id="auth-content" class="auth-shell" tabindex="-1">
    <RouterLink aria-label="Return home" class="auth-shell__brand" to="/">
      <AppLogo size="large" />
    </RouterLink>

    <RouterView></RouterView>
    <slot />
    <p class="auth-shell__footer">Threads clone · A place for conversations</p>
  </main>
</template>

<style scoped>
.auth-shell {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  min-height: 100dvh;
  padding: 4rem var(--space-4);
  align-content: center;
  justify-items: center;
  gap: var(--space-8);
  background: var(--color-page);
}
.auth-shell__brand {
  display: grid;
  border-radius: var(--radius-full);
}
.auth-shell__brand :deep(.app-logo) {
  width: 4rem;
  height: 4rem;
}
.auth-shell__footer {
  color: var(--color-text-muted);
  font-size: var(--font-size-micro);
  text-align: center;
}
</style>
