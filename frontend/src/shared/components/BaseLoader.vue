<script setup lang="ts">
withDefaults(
  defineProps<{
    message?: string
  }>(),
  {
    message: 'Loading...',
  },
)
</script>

<template>
  <div class="base-loader" role="status" aria-live="polite">
    <span class="base-loader__spinner" aria-hidden="true"></span>
    <p class="base-loader__label">{{ message }}</p>
  </div>
</template>

<style scoped>
.base-loader {
  display: grid;
  width: 100%;
  padding: var(--space-6);
  justify-items: center;
  gap: var(--space-3);
  color: var(--color-text-muted);
  text-align: center;
}

.base-loader__spinner {
  width: var(--space-8);
  height: var(--space-8);
  border: 0.125rem solid currentColor;
  border-inline-end-color: transparent;
  border-radius: var(--radius-full);
  animation: base-loader-spin 700ms linear infinite;
}

.base-loader__label {
  font-size: var(--font-size-support);
  font-weight: var(--font-weight-medium);
}

@keyframes base-loader-spin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .base-loader__spinner {
    animation: none;
  }
}
</style>
