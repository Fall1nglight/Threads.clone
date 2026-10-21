<script setup lang="ts">
import { computed, ref } from 'vue'
import PageHeader from '@/app/components/PageHeader.vue'
import ActivityFilterBar from '@/features/activity/components/ActivityFilterBar.vue'
import ActivityFollowItem from '@/features/activity/components/ActivityFollowItem.vue'
import ActivityPostLikeItem from '@/features/activity/components/ActivityPostLikeItem.vue'
import ActivityCommentItem from '@/features/activity/components/ActivityCommentItem.vue'
import notificationApi from '@/features/activity/notificationApi.ts'
import followApi from '@/features/follow/followApi.ts'
import {
  NotificationType,
  type NotificationCategory,
  type NotificationDto,
  NotificationStatus,
} from '@/features/activity/activityTypes.ts'
import { usePaginatedFetch } from '@/shared/composables/usePaginatedFetch.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import { useNotificationCount } from '@/features/activity/composables/useNotificationCount.ts'

const modalStore = useModalStore()
const { decrementUnseenCount } = useNotificationCount()
const activeCategory = ref<NotificationCategory | null>(null)
const activeStatus = ref<NotificationStatus | null>(null)

const {
  state: notificationsState,
  canLoadMore: canLoadMoreNotifications,
  loadMore: loadMoreNotifications,
} = usePaginatedFetch([activeCategory, activeStatus] as const, ([category, status], request) =>
  notificationApi.getNotifications(category, status, request),
)

const notifications = computed(() =>
  'items' in notificationsState.value ? notificationsState.value.items : null,
)

const markingSeenIds = ref(new Set<string>())
const updatingRequestIds = ref(new Set<string>())

function setPending(ids: typeof markingSeenIds, notificationId: string, isPending: boolean) {
  const nextIds = new Set(ids.value)
  if (isPending) nextIds.add(notificationId)
  else nextIds.delete(notificationId)
  ids.value = nextIds
}

function isFollowNotification(notification: NotificationDto) {
  return (
    notification.type === NotificationType.Followed ||
    notification.type === NotificationType.FollowRequestReceived ||
    notification.type === NotificationType.FollowRequestAccepted
  )
}

async function handleMarkAsSeen(notification: NotificationDto) {
  if (notification.seenAtUtc || markingSeenIds.value.has(notification.id)) return

  setPending(markingSeenIds, notification.id, true)
  try {
    await notificationApi.markNotificationAsSeen(notification.id)
    notification.seenAtUtc = new Date().toISOString()
    decrementUnseenCount()

    if (activeStatus.value === NotificationStatus.Unseen) {
      const currentNotifications = notifications.value
      const index = currentNotifications?.findIndex((item) => item.id === notification.id) ?? -1
      if (index >= 0) currentNotifications?.splice(index, 1)
    }
  } catch {
    modalStore.openAlertModal('Failed to mark this notification as seen. Please try again later!')
  } finally {
    setPending(markingSeenIds, notification.id, false)
  }
}

async function handleAcceptRequest(notification: NotificationDto) {
  if (
    notification.type !== NotificationType.FollowRequestReceived ||
    updatingRequestIds.value.has(notification.id)
  ) {
    return
  }

  setPending(updatingRequestIds, notification.id, true)
  try {
    await followApi.acceptIncomingFollowRequest(notification.triggeredByUser.id)
    notification.type = NotificationType.Followed
  } catch {
    modalStore.openAlertModal('Failed to accept this follow request. Please try again later!')
  } finally {
    setPending(updatingRequestIds, notification.id, false)
  }
}

async function handleRejectRequest(notification: NotificationDto) {
  if (
    notification.type !== NotificationType.FollowRequestReceived ||
    updatingRequestIds.value.has(notification.id)
  ) {
    return
  }

  setPending(updatingRequestIds, notification.id, true)
  try {
    const wasUnseen = notification.seenAtUtc === null
    await followApi.rejectIncomingFollowRequest(notification.triggeredByUser.id)

    if (wasUnseen) decrementUnseenCount()

    const currentNotifications = notifications.value
    const index = currentNotifications?.findIndex((item) => item.id === notification.id) ?? -1
    if (index >= 0) currentNotifications?.splice(index, 1)
  } catch {
    modalStore.openAlertModal('Failed to reject this follow request. Please try again later!')
  } finally {
    setPending(updatingRequestIds, notification.id, false)
  }
}
</script>

<template>
  <div>
    <PageHeader title="Activity" />
    <ActivityFilterBar v-model:category="activeCategory" v-model:status="activeStatus" />

    <section aria-label="Recent activity">
      <Transition name="async-fade" mode="out-in">
        <BaseLoader
          v-if="notificationsState.status === 'initial-loading'"
          key="activity-loader"
          message="Loading activity..."
        />

        <BaseErrorMessage
          v-else-if="notificationsState.status === 'initial-error'"
          key="activity-error"
          message="Failed to load activity. Please try again later!"
        />

        <div v-else-if="notifications" key="activity-content">
          <TransitionGroup name="async-list-fade" tag="div">
            <template v-for="notification in notifications" :key="notification.id">
              <ActivityFollowItem
                v-if="isFollowNotification(notification)"
                :is-marking-seen="markingSeenIds.has(notification.id)"
                :is-updating-request="updatingRequestIds.has(notification.id)"
                :notification="notification"
                @accept="handleAcceptRequest(notification)"
                @mark-seen="handleMarkAsSeen(notification)"
                @reject="handleRejectRequest(notification)"
              />

              <ActivityPostLikeItem
                v-else-if="notification.type === NotificationType.PostLiked"
                :is-marking-seen="markingSeenIds.has(notification.id)"
                :notification="notification"
                @mark-seen="handleMarkAsSeen(notification)"
              />

              <ActivityCommentItem
                v-else-if="notification.type === NotificationType.PostCommented"
                :is-marking-seen="markingSeenIds.has(notification.id)"
                :notification="notification"
                @mark-seen="handleMarkAsSeen(notification)"
              />
            </template>
          </TransitionGroup>

          <BaseEmptyMessage
            v-if="!notifications.length"
            :message="
              activeCategory === null && activeStatus === null
                ? 'You have no activity yet.'
                : 'No activity matches these filters.'
            "
          />

          <PaginationObserver v-if="canLoadMoreNotifications" @visible="loadMoreNotifications" />

          <Transition name="async-fade" mode="out-in">
            <BaseLoader
              v-if="notificationsState.status === 'more-loading'"
              key="activity-more-loader"
              message="Loading more activity..."
            />

            <BaseErrorMessage
              v-else-if="notificationsState.status === 'more-error'"
              key="activity-more-error"
              message="Failed to load more activity. Please try again later!"
            />
          </Transition>
        </div>
      </Transition>
    </section>
  </div>
</template>
