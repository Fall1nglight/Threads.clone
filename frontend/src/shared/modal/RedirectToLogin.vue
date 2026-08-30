<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const modalStore = useModalStore()
</script>

<template>
  <section class="redirect-to-login">
    <h1>Oops! In order to do that you have to log in.</h1>

    <RouterLink
      @click="modalStore.close"
      class="redirect-to-login__link button-link"
      to="/auth/login"
      >Log in</RouterLink
    >
  </section>
</template>

<style scoped>
.redirect-to-login {
  display: grid;
  width: 100%;
  max-width: 26rem;
  min-width: 0;
  padding: var(--space-6);
  justify-items: center;
  gap: var(--space-6);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
  text-align: center;
}

h1 {
  font-size: var(--font-size-page-title);
  line-height: 1.2;
  text-wrap: balance;
}

.redirect-to-login__link {
  display: inline-flex;
  min-height: var(--control-height);
  padding-inline: var(--space-4);
  align-items: center;
  border-radius: var(--radius-md);
  background: var(--color-action);
  color: var(--color-action-contrast);
  font-weight: var(--font-weight-semibold);
}

.redirect-to-login__link:hover {
  background: var(--color-action-hover);
}

.redirect-to-login__link:active {
  background: var(--color-action-active);
  transform: scale(0.98);
}
</style>
