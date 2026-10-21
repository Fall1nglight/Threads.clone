<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string
    disabled?: boolean
    active?: boolean
  }>(),
  {
    disabled: false,
    active: false,
  },
)
</script>

<template>
  <button
    :aria-label="label"
    :class="['base-icon-button', { 'base-icon-button--active': active }]"
    :disabled="disabled"
    type="button"
  >
    <slot />
  </button>
</template>

<style scoped>
.base-icon-button {
  display: inline-grid;
  flex: 0 0 auto;
  width: 2.75rem;
  height: 2.75rem;
  place-items: center;
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast),
    transform 90ms ease-out;
}

.base-icon-button:hover:not(:disabled) {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}

.base-icon-button:active:not(:disabled) {
  transform: scale(0.96);
}

.base-icon-button--active {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}

.base-icon-button:disabled {
  color: var(--color-disabled-text);
  cursor: not-allowed;
  opacity: 0.72;
}
</style>
