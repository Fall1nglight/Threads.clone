<template>
  <div aria-label="Activity filters" class="activity-filters">
    <span aria-current="page" class="activity-filters__item activity-filters__item--active"
      >All</span
    >
    <span class="activity-filters__item">Follows</span>
    <span class="activity-filters__item">Replies</span>
    <span class="activity-filters__item">Mentions</span>
  </div>
</template>

<style scoped>
.activity-filters {
  display: flex;
  padding: var(--space-3) var(--space-4);
  gap: var(--space-2);
  overflow-x: auto;
  border-block-end: 1px solid var(--color-border);
  scrollbar-width: none;
}

.activity-filters::-webkit-scrollbar {
  display: none;
}

.activity-filters__item {
  display: inline-grid;
  min-height: 2.75rem;
  padding-inline: var(--space-4);
  place-items: center;
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-full);
  color: var(--color-text-muted);
  font-weight: var(--font-weight-medium);
  white-space: nowrap;
}

.activity-filters__item--active {
  border-color: var(--color-action);
  background: var(--color-action);
  color: var(--color-action-contrast);
  font-weight: var(--font-weight-semibold);
}
</style>
