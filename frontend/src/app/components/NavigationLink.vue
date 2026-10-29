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
  <RouterLink
    :aria-label="accessibleLabel ?? label"
    :title="label"
    class="navigation-link"
    :to="to"
  >
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
  width: 3.75rem;
  min-height: 3.25rem;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-lg);
  color: var(--color-text-muted);
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast);
}
.navigation-link:hover {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}
.navigation-link.router-link-exact-active {
  color: var(--color-text);
}
.navigation-link.router-link-exact-active :deep(.app-icon) {
  stroke-width: 2.5;
}
.navigation-link__icon {
  position: relative;
  display: inline-grid;
  flex: 0 0 auto;
  place-items: center;
}
.navigation-link__icon :deep(.app-icon) {
  width: 1.75rem;
  height: 1.75rem;
}
.navigation-link__label {
  position: absolute;
  left: calc(100% + 0.5rem);
  padding: 0.375rem 0.625rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  box-shadow: var(--shadow-panel);
  font-size: var(--font-size-support);
  opacity: 0;
  pointer-events: none;
  white-space: nowrap;
}
@media (min-width: 48rem) {
  .navigation-link:hover .navigation-link__label,
  .navigation-link:focus-visible .navigation-link__label {
    opacity: 1;
  }
}
@media (max-width: 47.999rem) {
  .navigation-link {
    width: 100%;
  }
  .navigation-link__label {
    display: none;
  }
}
</style>
