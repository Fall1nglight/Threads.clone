<script setup lang="ts">
import { RouterLink } from 'vue-router'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'

withDefaults(
  defineProps<{
    userId: string
    actor: string
    username: string
    initials: string
    action: string
    createdAtUtc: string
    details?: string
    showFollowButton?: boolean
  }>(),
  {
    details: '',
    showFollowButton: false,
  },
)
</script>

<template>
  <article class="activity-item">
    <RouterLink :aria-label="`View ${actor}'s profile`" :to="`/users/${userId}`">
      <BaseAvatar :initials="initials" :label="`${actor}'s avatar`" />
    </RouterLink>
    <div class="activity-item__copy">
      <p>
        <RouterLink :to="`/users/${userId}`">{{ actor }}</RouterLink>
        <span class="activity-item__username"> @{{ username }}</span>
        <span aria-hidden="true"> · </span>
        <span class="activity-item__timestamp">{{ createdAtUtc }}</span>
      </p>
      <p class="activity-item__action">{{ action }}</p>
      <p v-if="details" class="activity-item__details">{{ details }}</p>
    </div>
    <BaseButton v-if="showFollowButton" size="compact" variant="secondary">Follow</BaseButton>
  </article>
</template>

<style scoped>
.activity-item {
  display: grid;
  padding: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: start;
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.activity-item > a {
  border-radius: var(--radius-full);
  text-decoration: none;
}

.activity-item__copy {
  min-width: 0;
}

.activity-item__copy a {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  font-weight: var(--font-weight-semibold);
  text-decoration: none;
}

.activity-item__copy a:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.activity-item__username,
.activity-item__timestamp {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
}

.activity-item__action {
  margin-block-start: var(--space-1);
}

.activity-item__details {
  margin-block-start: var(--space-2);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  overflow-wrap: anywhere;
}

@media (max-width: 24rem) {
  .activity-item {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .activity-item > :last-child:not(.activity-item__copy) {
    grid-column: 2;
    justify-self: start;
  }
}
</style>
