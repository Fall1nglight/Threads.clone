<script setup lang="ts">
import AppIcon from '@/shared/icons/AppIcon.vue'
</script>

<template>
  <label class="search-field" for="user-search">
    <span class="sr-only">Search people</span>
    <AppIcon name="search" />
    <input
      id="user-search"
      autocomplete="off"
      placeholder="Search"
      spellcheck="false"
      type="search"
    />
  </label>
</template>

<style scoped>
.search-field {
  display: grid;
  min-height: var(--control-height);
  grid-template-columns: auto 1fr;
  align-items: center;
  gap: var(--space-3);
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface-raised);
  color: var(--color-text-muted);
}

.search-field:focus-within {
  outline: 0.1875rem solid var(--color-focus);
  outline-offset: 0.125rem;
}

.search-field > :first-child {
  margin-inline-start: var(--space-3);
}

input {
  min-width: 0;
  min-height: var(--control-height);
  padding: 0 var(--space-3) 0 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--color-text);
  font-size: var(--font-size-body);
}

input::placeholder {
  color: var(--color-text-muted);
  opacity: 1;
}
</style>
