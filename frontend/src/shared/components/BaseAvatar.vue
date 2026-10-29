<script setup lang="ts">
withDefaults(
  defineProps<{
    initials: string
    label: string
    size?: 'small' | 'medium' | 'large'
  }>(),
  {
    initials: 'NA',
    size: 'medium',
  },
)
</script>

<template>
  <span :aria-label="label" :class="['base-avatar', `base-avatar--${size}`]" role="img">
    {{ initials }}
  </span>
</template>

<style scoped>
.base-avatar {
  display: inline-grid;
  flex: 0 0 auto;
  place-items: center;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
  background: var(--color-surface-subtle);
  box-shadow: inset 0 0 0 3px var(--color-surface);
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
  letter-spacing: -0.02em;
}

.base-avatar--small {
  width: 2rem;
  height: 2rem;
  font-size: var(--font-size-micro);
}

.base-avatar--medium {
  width: 2.5rem;
  height: 2.5rem;
  font-size: var(--font-size-support);
}

.base-avatar--large {
  width: 5.25rem;
  height: 5.25rem;
  font-size: var(--font-size-title);
}
</style>
