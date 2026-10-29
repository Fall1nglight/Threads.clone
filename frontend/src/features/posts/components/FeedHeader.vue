<script setup lang="ts">
import type { SelectableFeedKind } from '@/features/posts/feed/feedTypes.ts'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'

const { runOrShowLoginForm } = useLoginPrompt()

const selectedFeed = defineModel<SelectableFeedKind>('selectedFeed', {
  required: true,
})

type FeedOptions = {
  value: SelectableFeedKind
  label: string
}

const options: FeedOptions[] = [
  {
    value: 'global',
    label: 'For you',
  },
  {
    value: 'personal',
    label: 'Following',
  },
]
</script>

<template>
  <header class="feed-header">
    <h1 class="sr-only">Home</h1>

    <div role="group" aria-label="Feed views" class="feed-header__tabs">
      <button
        type="button"
        :aria-pressed="selectedFeed === option.value"
        v-for="option in options"
        :key="option.value"
        class="feed-header__tab"
        :class="{ 'feed-header__tab--active': selectedFeed == option.value }"
        @click="runOrShowLoginForm(() => (selectedFeed = option.value))"
      >
        {{ option.label }}
      </button>
    </div>
  </header>
</template>

<style scoped>
.feed-header {
  position: sticky;
  top: var(--mobile-top-height);
  z-index: 10;
  border-bottom: 1px solid var(--color-border);
  background: var(--color-surface);
}
.feed-header__tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  border-radius: inherit;
}
.feed-header__tab:first-child {
  border-start-start-radius: inherit;
}
.feed-header__tab:last-child {
  border-start-end-radius: inherit;
}
.feed-header__tab {
  position: relative;
  min-height: 3.75rem;
  padding: var(--space-4);
  background: transparent;
  color: var(--color-text-muted);
  font-weight: var(--font-weight-semibold);
  cursor: pointer;
  transition:
    color var(--transition-fast),
    background-color var(--transition-fast);
}
.feed-header__tab:hover {
  color: var(--color-text);
  background: var(--color-surface-subtle);
}
.feed-header__tab--active {
  color: var(--color-text);
}
.feed-header__tab--active::after {
  position: absolute;
  bottom: -1px;
  inset-inline: 0;
  height: 2px;
  background: var(--color-text);
  content: '';
}
@media (min-width: 48rem) {
  .feed-header {
    top: 0;
    border-start-start-radius: var(--radius-xl);
    border-start-end-radius: var(--radius-xl);
  }
  .feed-header__tab {
    min-height: 4rem;
  }
}
</style>
