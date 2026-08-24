<script setup lang="ts">
withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost'
    size?: 'default' | 'compact'
    disabled?: boolean
    isLoading?: boolean
    loadingText?: string
    type?: 'button' | 'submit'
  }>(),
  {
    variant: 'primary',
    size: 'default',
    disabled: false,
    isLoading: false,
    loadingText: 'Loading...',
    type: 'button',
  },
)

const emit = defineEmits(['clicked'])
</script>

<template>
  <button
    @click="emit('clicked')"
    :aria-busy="isLoading"
    :class="[
      'base-button',
      `base-button--${variant}`,
      `base-button--${size}`,
      { 'base-button--loading': isLoading },
    ]"
    :disabled="disabled || isLoading"
    :type="type"
  >
    <template v-if="isLoading">
      <span aria-hidden="true" class="base-button__spinner"></span>
      <span>{{ loadingText }}</span>
    </template>
    <slot v-else />
  </button>
</template>

<style scoped>
.base-button {
  display: inline-flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
  min-width: max-content;
  min-height: var(--control-height);
  padding-inline: var(--space-4);
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  font-size: var(--font-size-body);
  font-weight: var(--font-weight-semibold);
  line-height: 1;
  white-space: nowrap;
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    border-color var(--transition-fast),
    color var(--transition-fast),
    transform 90ms ease-out;
}

.base-button--primary {
  background: var(--color-action);
  color: var(--color-action-contrast);
}

.base-button--primary:hover:not(:disabled) {
  background: var(--color-action-hover);
}

.base-button--primary:active:not(:disabled) {
  background: var(--color-action-active);
  transform: scale(0.98);
}

.base-button--secondary {
  border-color: var(--color-control-border);
  background: var(--color-surface);
  color: var(--color-text);
}

.base-button--secondary:hover:not(:disabled),
.base-button--ghost:hover:not(:disabled) {
  background: var(--color-surface-subtle);
}

.base-button--secondary:active:not(:disabled),
.base-button--ghost:active:not(:disabled) {
  background: var(--color-border);
  transform: scale(0.98);
}

.base-button--ghost {
  background: transparent;
  color: var(--color-text);
}

.base-button--compact {
  min-height: var(--control-height);
  padding-inline: var(--space-3);
  font-size: var(--font-size-support);
}

.base-button:disabled {
  border-color: var(--color-control-border);
  background: var(--color-disabled);
  color: var(--color-disabled-text);
  cursor: not-allowed;
}

.base-button--loading:disabled {
  cursor: progress;
}

.base-button--primary.base-button--loading:disabled {
  border-color: transparent;
  background: color-mix(in srgb, var(--color-action) 82%, var(--color-surface));
  color: var(--color-action-contrast);
}

.base-button--secondary.base-button--loading:disabled,
.base-button--ghost.base-button--loading:disabled {
  border-color: var(--color-control-border);
  background: var(--color-surface-subtle);
  color: var(--color-text-muted);
}

.base-button__spinner {
  width: 1rem;
  height: 1rem;
  flex: 0 0 auto;
  border: 0.125rem solid currentColor;
  border-inline-end-color: transparent;
  border-radius: var(--radius-full);
  animation: base-button-spin 700ms linear infinite;
}

@keyframes base-button-spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
