<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'

defineProps<{
  to: string
  label: string
  icon: 'home' | 'search' | 'heart' | 'user'
}>()
</script>

<template>
  <RouterLink :aria-label="label" class="navigation-link" :to="to">
    <AppIcon :name="icon" />
    <span class="navigation-link__label">{{ label }}</span>
  </RouterLink>
</template>

<style scoped>
.navigation-link {
  position: relative;
  display: flex;
  min-width: 2.75rem;
  min-height: 2.75rem;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
  border-radius: var(--radius-full);
  color: var(--color-text-muted);
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast),
    transform 90ms ease-out;
}

.navigation-link:hover {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}

.navigation-link:active {
  transform: scale(0.96);
}

.navigation-link.router-link-exact-active {
  color: var(--color-text);
}

.navigation-link.router-link-exact-active::after {
  position: absolute;
  inset-block-end: 0;
  width: 0.375rem;
  height: 0.375rem;
  border-radius: var(--radius-full);
  background: currentColor;
  content: '';
}

.navigation-link__label {
  display: none;
  font-weight: var(--font-weight-semibold);
}

@media (min-width: 75rem) {
  .navigation-link {
    width: 100%;
    justify-content: flex-start;
    padding-inline: var(--space-4);
  }

  .navigation-link.router-link-exact-active::after {
    inset-block-end: auto;
    inset-inline-start: 0;
  }

  .navigation-link__label {
    display: inline;
  }
}
</style>
