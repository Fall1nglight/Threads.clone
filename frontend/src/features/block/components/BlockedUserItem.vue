<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'

withDefaults(
  defineProps<{
    userId: string
    name: string
    username: string
    initials: string
    summary: string
    verified?: boolean
  }>(),
  {
    verified: false,
  },
)

const emit = defineEmits<{
  handleUnblock: []
}>()
</script>

<template>
  <article class="blocked-user-item">
    <RouterLink :aria-label="`View ${name}'s profile`" :to="`/users/${userId}`">
      <BaseAvatar :initials="initials" :label="`${name}'s avatar`" />
    </RouterLink>
    <div class="blocked-user-item__copy">
      <div class="blocked-user-item__name-row">
        <RouterLink class="blocked-user-item__name" :to="`/users/${userId}`">
          {{ name }}
        </RouterLink>
        <AppIcon v-if="verified" class="blocked-user-item__verified" name="verified" />
      </div>
      <p class="blocked-user-item__username">@{{ username }}</p>
      <p class="blocked-user-item__summary">{{ summary }}</p>
    </div>

    <BaseButton size="compact" variant="secondary" @click="emit('handleUnblock')">
      Unblock
    </BaseButton>
  </article>
</template>

<style scoped>
.blocked-user-item {
  display: grid;
  padding-block: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: start;
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.blocked-user-item > a {
  border-radius: var(--radius-full);
  text-decoration: none;
}

.blocked-user-item__copy {
  min-width: 0;
}

.blocked-user-item__name-row {
  display: flex;
  align-items: center;
  gap: var(--space-1);
}

.blocked-user-item__name {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  overflow: hidden;
  font-weight: var(--font-weight-semibold);
  text-overflow: ellipsis;
  white-space: nowrap;
  text-decoration: none;
}

.blocked-user-item__name:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.blocked-user-item__verified {
  width: 1rem;
  height: 1rem;
}

.blocked-user-item__username,
.blocked-user-item__summary {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  overflow-wrap: anywhere;
}

.blocked-user-item__summary {
  margin-block-start: var(--space-1);
}

@media (max-width: 24rem) {
  .blocked-user-item {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .blocked-user-item > :last-child {
    grid-column: 2;
    justify-self: start;
  }
}
</style>
