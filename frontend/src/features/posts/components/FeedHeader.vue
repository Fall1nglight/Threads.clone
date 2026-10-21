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
    <h1>Home</h1>

    <div aria-label="Feed views" class="feed-header__tabs">
      <span
        v-for="option in options"
        :key="option.value"
        class="feed-header__tab"
        :class="{ 'feed-header__tab--active': selectedFeed == option.value }"
        @click="runOrShowLoginForm(() => (selectedFeed = option.value))"
      >
        {{ option.label }}
      </span>
    </div>
  </header>
</template>

<style scoped>
.feed-header {
  position: sticky;
  inset-block-start: var(--mobile-top-height);
  z-index: 10;
  display: grid;
  border-block-end: 1px solid var(--color-border);
  background: var(--color-surface);
}

h1 {
  padding: var(--space-4);
  font-size: var(--font-size-title);
  line-height: 1.2;
  text-align: center;
}

.feed-header__tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
}

.feed-header__tab {
  position: relative;
  min-height: 2.75rem;
  padding: var(--space-3);
  color: var(--color-text-muted);
  font-weight: var(--font-weight-medium);
  text-align: center;
  cursor: pointer;
}

.feed-header__tab--active {
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
}

.feed-header__tab--active::after {
  position: absolute;
  inset-block-end: 0;
  inset-inline: var(--space-4);
  height: 0.125rem;
  background: var(--color-text);
  content: '';
}

@media (min-width: 48rem) {
  .feed-header {
    inset-block-start: 0;
    border-start-start-radius: var(--radius-xl);
    border-start-end-radius: var(--radius-xl);
  }
}
</style>
