<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import type { FollowStatus } from '@/features/follow/followTypes.ts'

withDefaults(
  defineProps<{
    userId: string
    name: string
    username: string
    initials: string
    summary: string
    verified?: boolean
    followStatus: FollowStatus | null
    showFollowButton: boolean
  }>(),
  {
    verified: false,
  },
)

const emit = defineEmits<{
  handleFollow: []
}>()
</script>

<template>
  <article class="user-result">
    <RouterLink :aria-label="`View ${name}'s profile`" :to="`/users/${userId}`">
      <BaseAvatar :initials="initials" :label="`${name}'s avatar`" />
    </RouterLink>
    <div class="user-result__copy">
      <div class="user-result__name-row">
        <RouterLink class="user-result__name" :to="`/users/${userId}`">{{ name }}</RouterLink>
        <AppIcon v-if="verified" class="user-result__verified" name="verified" />
      </div>
      <p class="user-result__username">@{{ username }}</p>
      <p class="user-result__summary">{{ summary }}</p>
    </div>

    <BaseButton
      v-if="showFollowButton"
      @click="emit('handleFollow')"
      size="compact"
      variant="secondary"
    >
      {{ followStatus == null ? 'Follow' : followStatus == 0 ? 'Pending' : 'Following' }}
    </BaseButton>
  </article>
</template>

<style scoped>
.user-result {
  display: grid;
  padding-block: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: start;
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.user-result > a {
  border-radius: var(--radius-full);
  text-decoration: none;
}

.user-result__copy {
  min-width: 0;
}

.user-result__name-row {
  display: flex;
  align-items: center;
  gap: var(--space-1);
}

.user-result__name {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  overflow: hidden;
  font-weight: var(--font-weight-semibold);
  text-overflow: ellipsis;
  white-space: nowrap;
  text-decoration: none;
}

.user-result__name:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.user-result__verified {
  width: 1rem;
  height: 1rem;
}

.user-result__username,
.user-result__summary {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  overflow-wrap: anywhere;
}

.user-result__summary {
  margin-block-start: var(--space-1);
}

@media (max-width: 24rem) {
  .user-result {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .user-result > :last-child {
    grid-column: 2;
    justify-self: start;
  }
}
</style>
