import { getApi } from '@/shared/api/baseApi.ts'
import type { PagedRequest, PagedResponse } from '@/shared/types/paginationTypes.ts'
import type {
  NotificationCategory,
  NotificationDto,
  NotificationStatus,
  UnseenNotificationCountDto,
} from '@/features/activity/activityTypes.ts'
import { getPaginatedUrl } from '@/shared/api/baseApi.ts'

const api = getApi('/notifications')

const paths = {
  getNotifications: (category: NotificationCategory | null, status: NotificationStatus | null) => {
    const searchParams = new URLSearchParams()
    if (category) searchParams.set('category', category)
    if (status) searchParams.set('status', status)

    const query = searchParams.toString()
    return query ? `?${query}` : ''
  },

  markNotificationAsSeen: (notificationId: string) => `/${notificationId}/seen`,

  getUnseenNotificationCount: '/unseen-count',
} as const

async function getNotifications(
  category: NotificationCategory | null,
  status: NotificationStatus | null,
  paginationDetails: PagedRequest = { pageSize: 5, cursor: null },
) {
  const url = getPaginatedUrl(paths.getNotifications(category, status), paginationDetails)
  const response = await api.get<PagedResponse<NotificationDto>>(url)
  return response.data
}

async function markNotificationAsSeen(notificationId: string) {
  await api.post(paths.markNotificationAsSeen(notificationId))
}

async function getUnseenNotificationCount() {
  const response = await api.get<UnseenNotificationCountDto>(paths.getUnseenNotificationCount)
  return response.data
}

export default { getNotifications, markNotificationAsSeen, getUnseenNotificationCount }
