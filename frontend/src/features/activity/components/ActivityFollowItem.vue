<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'
import { NotificationType, type NotificationDto } from '@/features/activity/activityTypes.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const props = withDefaults(
  defineProps<{
    notification: NotificationDto
    isMarkingSeen?: boolean
    isUpdatingRequest?: boolean
  }>(),
  {
    isMarkingSeen: false,
    isUpdatingRequest: false,
  },
)

const emit = defineEmits<{
  accept: []
  reject: []
  markSeen: []
}>()

const userPage = computed(() => ({
  name: 'user-profile',
  params: { userId: props.notification.triggeredByUser.id },
}))
const username = computed(() => props.notification.triggeredByUser.username)
const initials = computed(() => getInitials(username.value))
const createdAtUtc = computed(() => props.notification.createdAtUtc)
const { formattedDate } = useDateFormatter(createdAtUtc)

const eventText = computed(() => {
  if (props.notification.type === NotificationType.FollowRequestAccepted) {
    return 'has accepted your follow request.'
  }

  if (props.notification.type === NotificationType.FollowRequestReceived) {
    return 'sent you a follow request.'
  }

  return 'started following you.'
})

const isRequest = computed(() => props.notification.type === NotificationType.FollowRequestReceived)
const isBusy = computed(() => props.isMarkingSeen || props.isUpdatingRequest)
</script>

<template>
  <article class="activity-item">
    <RouterLink :aria-label="`View ${username}'s profile`" :to="userPage">
      <BaseAvatar :initials="initials" :label="`${username}'s avatar`" />
    </RouterLink>

    <div class="activity-item__copy">
      <p>
        <RouterLink class="activity-item__user" :to="userPage">{{ username }}&nbsp;</RouterLink>
        <span>{{ eventText }}</span>
      </p>
      <p class="activity-item__timestamp">{{ formattedDate }}</p>
    </div>

    <div class="activity-item__actions">
      <BaseIconButton
        v-if="isRequest"
        :disabled="isBusy"
        label="Accept follow request"
        title="Accept follow request"
        @click="emit('accept')"
      >
        <AppIcon name="check" />
      </BaseIconButton>
      <BaseIconButton
        v-if="isRequest"
        :disabled="isBusy"
        label="Reject follow request"
        title="Reject follow request"
        @click="emit('reject')"
      >
        <AppIcon name="close" />
      </BaseIconButton>
      <BaseButton
        :disabled="notification.seenAtUtc !== null || isUpdatingRequest"
        :is-loading="isMarkingSeen"
        loading-text="Marking..."
        size="compact"
        variant="ghost"
        @clicked="emit('markSeen')"
      >
        {{ notification.seenAtUtc === null ? 'Mark as seen' : 'Seen' }}
      </BaseButton>
    </div>
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
  padding-block-start: var(--space-2);
  overflow-wrap: anywhere;
}

.activity-item__user {
  font-weight: var(--font-weight-semibold);
  text-decoration: none;
}

.activity-item__user:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.activity-item__timestamp {
  margin-block-start: var(--space-1);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
}

.activity-item__actions {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

@media (max-width: 36rem) {
  .activity-item {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .activity-item__actions {
    grid-column: 2;
    flex-wrap: wrap;
  }
}
</style>
