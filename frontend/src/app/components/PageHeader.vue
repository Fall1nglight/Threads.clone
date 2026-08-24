<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'

defineProps<{
  title: string
  backTo?: string
}>()
</script>

<template>
  <header class="page-header">
    <RouterLink
      v-if="backTo"
      :aria-label="`Back to ${backTo === '/' ? 'home' : 'previous page'}`"
      class="page-header__back"
      :to="backTo"
    >
      <AppIcon name="back" />
    </RouterLink>
    <span v-else aria-hidden="true" class="page-header__spacer" />
    <h1 class="page-header__title">{{ title }}</h1>
    <span aria-hidden="true" class="page-header__spacer" />
  </header>
</template>

<style scoped>
.page-header {
  position: sticky;
  inset-block-start: var(--mobile-top-height);
  z-index: 10;
  display: grid;
  min-height: 3.75rem;
  padding-inline: var(--space-2);
  grid-template-columns: 2.75rem 1fr 2.75rem;
  align-items: center;
  border-block-end: 1px solid var(--color-border);
  background: var(--color-surface);
}

.page-header__back,
.page-header__spacer {
  display: grid;
  width: 2.75rem;
  height: 2.75rem;
  place-items: center;
  border-radius: var(--radius-full);
}

.page-header__back:hover {
  background: var(--color-surface-subtle);
}

.page-header__title {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.2;
  text-align: center;
}

@media (min-width: 48rem) {
  .page-header {
    inset-block-start: 0;
    border-start-start-radius: var(--radius-xl);
    border-start-end-radius: var(--radius-xl);
  }
}
</style>
