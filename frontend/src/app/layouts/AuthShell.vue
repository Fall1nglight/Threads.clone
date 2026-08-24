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
  </main>
</template>

<style scoped>
.auth-shell {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  min-height: 100vh;
  padding: var(--space-8) var(--space-4);
  place-content: center;
  justify-items: center;
  gap: var(--space-8);
  background: var(--color-page);
}

.auth-shell__brand {
  display: grid;
  border-radius: var(--radius-full);
}
</style>
