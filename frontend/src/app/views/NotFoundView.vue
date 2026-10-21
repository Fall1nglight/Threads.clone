<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppLogo from '@/app/components/AppLogo.vue'
</script>

<template>
  <main class="not-found">
    <AppLogo size="large" />
    <section class="not-found__content">
      <p class="not-found__eyebrow">404</p>
      <h1>Page not found</h1>
      <p>The page may have moved, or the link might be incomplete.</p>
      <RouterLink class="not-found__link button-link" to="/">Return home</RouterLink>
    </section>
  </main>
</template>

<style scoped>
.not-found {
  display: grid;
  min-height: 100vh;
  padding: var(--space-8) var(--space-4);
  place-content: center;
  justify-items: center;
  gap: var(--space-8);
  background: var(--color-page);
}

.not-found__content {
  display: grid;
  max-width: 28rem;
  justify-items: center;
  gap: var(--space-4);
  text-align: center;
}

.not-found__eyebrow {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  font-weight: var(--font-weight-semibold);
}

h1 {
  font-size: clamp(1.75rem, 6vw, 2.5rem);
  line-height: 1;
  text-wrap: balance;
}

.not-found__content > p:not(.not-found__eyebrow) {
  color: var(--color-text-muted);
  text-wrap: pretty;
}

.not-found__link {
  display: inline-flex;
  min-height: var(--control-height);
  padding-inline: var(--space-4);
  align-items: center;
  border-radius: var(--radius-md);
  background: var(--color-action);
  color: var(--color-action-contrast);
  font-weight: var(--font-weight-semibold);
}

.not-found__link:hover {
  background: var(--color-action-hover);
}
</style>
