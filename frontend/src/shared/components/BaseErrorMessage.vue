<script setup lang="ts">
defineProps<{
  message: string
}>()
</script>

<template>
  <p class="base-error-message" role="alert">{{ message }}</p>
</template>

<style scoped>
.base-error-message {
  width: calc(100% - var(--space-8));
  margin: var(--space-8) auto 0;
  min-width: 0;
  padding: var(--space-3) var(--space-4);
  border: 1px solid color-mix(in srgb, var(--color-danger) 35%, var(--color-surface));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-danger) 12%, var(--color-surface));
  color: var(--color-danger);
  font-size: var(--font-size-support);
  line-height: 1.5;
  overflow-wrap: anywhere;
}
</style>
