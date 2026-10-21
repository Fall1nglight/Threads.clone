<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'
import { NotificationSubjectKind, type NotificationDto } from '@/features/activity/activityTypes.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const props = withDefaults(
  defineProps<{
    notification: NotificationDto
    isMarkingSeen?: boolean
  }>(),
  { isMarkingSeen: false },
)

const emit = defineEmits<{ markSeen: [] }>()
const username = computed(() => props.notification.triggeredByUser.username)
const initials = computed(() => getInitials(username.value))
const userPage = computed(() => ({
  name: 'user-profile',
  params: { userId: props.notification.triggeredByUser.id },
}))
const postPage = computed(() =>
  props.notification.subjectKind === NotificationSubjectKind.Post && props.notification.subjectId
    ? { name: 'post-detail', params: { postId: props.notification.subjectId } }
    : null,
)
const createdAtUtc = computed(() => props.notification.createdAtUtc)
const { formattedDate } = useDateFormatter(createdAtUtc)
</script>

<template>
  <article class="activity-item">
    <RouterLink :aria-label="`View ${username}'s profile`" :to="userPage">
      <BaseAvatar :initials="initials" :label="`${username}'s avatar`" />
    </RouterLink>

    <div class="activity-item__copy">
      <p>
        <RouterLink class="activity-item__user" :to="userPage">{{ username }}&nbsp;</RouterLink>
        <span>has liked your </span>
        <RouterLink v-if="postPage" class="activity-item__post" :to="postPage">post</RouterLink>
        <span v-else>post</span><span>.</span>
      </p>
      <p class="activity-item__timestamp">{{ formattedDate }}</p>
    </div>

    <BaseButton
      :disabled="notification.seenAtUtc !== null"
      :is-loading="isMarkingSeen"
      loading-text="Marking..."
      size="compact"
      variant="ghost"
      @clicked="emit('markSeen')"
    >
      {{ notification.seenAtUtc === null ? 'Mark as seen' : 'Seen' }}
    </BaseButton>
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

.activity-item__user:hover,
.activity-item__post {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.activity-item__timestamp {
  margin-block-start: var(--space-1);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
}

@media (max-width: 36rem) {
  .activity-item {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .activity-item > :last-child {
    grid-column: 2;
    justify-self: start;
  }
}
</style>
