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
  display: grid;
  min-height: 12rem;
  margin: 0;
  padding: var(--space-8) var(--page-gutter);
  place-content: center;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  line-height: 1.5;
  text-align: center;
  overflow-wrap: anywhere;
}
</style>
