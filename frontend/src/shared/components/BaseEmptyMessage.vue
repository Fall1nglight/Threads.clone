<script setup lang="ts">
defineProps<{
  message: string
}>()
</script>

<template>
  <p class="base-empty-message" role="status" aria-live="polite">{{ message }}</p>
</template>

<style scoped>
.base-empty-message {
  width: calc(100% - var(--space-8));
  margin: var(--space-8) auto 0;
  min-width: 0;
  padding: var(--space-3) var(--space-4);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-subtle);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  line-height: 1.5;
  overflow-wrap: anywhere;
}
</style>
