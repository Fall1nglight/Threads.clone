<script setup lang="ts">
defineProps<{
  message: string
}>()
</script>

<template>
  <section class="alert-message" aria-live="assertive" role="alert">
    <p class="alert-message__badge">Error</p>
    <p class="alert-message__text">{{ message }}</p>
  </section>
</template>

<style scoped>
.alert-message {
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
  color: var(--color-text);
  text-align: center;
}

.alert-message__badge {
  min-width: 7rem;
  padding: var(--space-2) var(--space-4);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-danger);
  font-weight: var(--font-weight-bold);
  line-height: 1.5;
}

.alert-message__text {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.5;
  overflow-wrap: anywhere;
  text-wrap: balance;
}
</style>
