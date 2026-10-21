<script setup lang="ts">
import { computed } from 'vue'
import NavigationLink from '@/app/components/NavigationLink.vue'
import { useNotificationCount } from '@/features/activity/composables/useNotificationCount.ts'

const { unseenCount } = useNotificationCount()

const badgeText = computed(() => {
  if (unseenCount.value === null || unseenCount.value === 0) return null
  return unseenCount.value > 99 ? '99+' : String(unseenCount.value)
})

const accessibleLabel = computed(() => {
  if (unseenCount.value === null || unseenCount.value === 0) return 'Activity'

  const notificationLabel = unseenCount.value === 1 ? 'notification' : 'notifications'
  return `Activity, ${unseenCount.value} unseen ${notificationLabel}`
})
</script>

<template>
  <NavigationLink :accessible-label="accessibleLabel" icon="heart" label="Activity" to="/activity">
    <template #badge>
      <span v-if="badgeText" aria-hidden="true" class="activity-navigation-link__badge">
        {{ badgeText }}
      </span>
    </template>
  </NavigationLink>
</template>

<style scoped>
.activity-navigation-link__badge {
  position: absolute;
  inset-block-start: calc(var(--space-2) * -1);
  inset-inline-end: calc(var(--space-2) * -1);
  z-index: 1;
  display: inline-grid;
  min-width: 1.25rem;
  height: 1.25rem;
  padding-inline: var(--space-1);
  place-items: center;
  border-radius: var(--radius-full);
  background: var(--color-danger);
  color: var(--color-action-contrast);
  font-size: var(--font-size-micro);
  font-variant-numeric: tabular-nums;
  font-weight: var(--font-weight-bold);
  line-height: 1;
  pointer-events: none;
}
</style>
