import { reactive } from 'vue'
import followApi from '@/features/follow/followApi.ts'
import { FollowStatus } from '@/features/follow/followTypes.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

type FollowableUser = {
  id: string
  followStatusWithCurrentUser: FollowStatus | null
}

export type ToggleFollowResult =
  | {
      action: 'followed'
      status: FollowStatus
    }
  | {
      action: 'removed'
      previousStatus: FollowStatus
    }

export function useToggleFollow() {
  const modalStore = useModalStore()
  const pendingFollowIds = reactive(new Set<string>())

  function isFollowPending(userId: string): boolean {
    return pendingFollowIds.has(userId)
  }

  async function toggleFollow(user: FollowableUser): Promise<ToggleFollowResult | undefined> {
    if (pendingFollowIds.has(user.id)) return

    pendingFollowIds.add(user.id)

    const previousStatus = user.followStatusWithCurrentUser

    try {
      if (previousStatus === null) {
        const result = await followApi.sendFollowRequest(user.id)
        user.followStatusWithCurrentUser = result.status

        return {
          action: 'followed',
          status: result.status,
        }
      }

      if (previousStatus === FollowStatus.Accepted) {
        await followApi.deleteFollowing(user.id)
      }

      if (previousStatus === FollowStatus.Pending) {
        await followApi.cancelOutgoingFollowRequest(user.id)
      }

      user.followStatusWithCurrentUser = null

      return {
        action: 'removed',
        previousStatus,
      }
    } catch {
      modalStore.openAlertModal('Failed to update the follow status. Please try again later!')
    } finally {
      pendingFollowIds.delete(user.id)
    }
  }

  return {
    toggleFollow,
    isFollowPending,
  }
}
