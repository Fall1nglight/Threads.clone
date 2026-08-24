<script setup lang="ts">
withDefaults(
  defineProps<{
    initials: string
    label: string
    size?: 'small' | 'medium' | 'large'
  }>(),
  {
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
  width: 2.75rem;
  height: 2.75rem;
  font-size: var(--font-size-support);
}

.base-avatar--large {
  width: 5rem;
  height: 5rem;
  font-size: var(--font-size-title);
}
</style>
