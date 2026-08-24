<script setup lang="ts">
defineProps<{
  title: string
  subtitle: string
}>()
</script>

<template>
  <section class="auth-card">
    <header class="auth-card__header">
      <h1>{{ title }}</h1>
      <p>{{ subtitle }}</p>
    </header>
    <slot />
  </section>
</template>

<style scoped>
.auth-card {
  display: grid;
  width: 100%;
  max-width: 26rem;
  min-width: 0;
  padding: var(--space-6);
  gap: var(--space-6);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
}

.auth-card__header {
  display: grid;
  gap: var(--space-3);
  text-align: center;
}

h1 {
  font-size: var(--font-size-page-title);
  line-height: 1.2;
  text-wrap: balance;
}

p {
  color: var(--color-text-muted);
  text-wrap: pretty;
}
</style>
