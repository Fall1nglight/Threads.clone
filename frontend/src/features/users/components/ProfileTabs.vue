<template>
  <div aria-label="Profile sections" class="profile-tabs">
    <span aria-current="page" class="profile-tabs__item profile-tabs__item--active">Threads</span>
    <span class="profile-tabs__item">Replies</span>
    <span class="profile-tabs__item">Reposts</span>
  </div>
</template>

<style scoped>
.profile-tabs {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  border-block-end: 1px solid var(--color-border);
}

.profile-tabs__item {
  position: relative;
  min-height: 2.75rem;
  padding: var(--space-3);
  color: var(--color-text-muted);
  font-weight: var(--font-weight-medium);
  text-align: center;
}

.profile-tabs__item--active {
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
}

.profile-tabs__item--active::after {
  position: absolute;
  inset-block-end: 0;
  inset-inline: var(--space-3);
  height: 0.125rem;
  background: var(--color-text);
  content: '';
}
</style>
