<script setup lang="ts">
import { type RouteLocationRaw, RouterLink } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'

defineProps<{
  to: RouteLocationRaw
  label: string
  accessibleLabel?: string
  icon: 'home' | 'search' | 'heart' | 'user'
}>()
</script>

<template>
  <RouterLink :aria-label="accessibleLabel ?? label" class="navigation-link" :to="to">
    <span class="navigation-link__icon">
      <AppIcon :name="icon" />
      <slot name="badge" />
    </span>
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

.navigation-link__icon {
  position: relative;
  display: inline-grid;
  flex: 0 0 auto;
  place-items: center;
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
