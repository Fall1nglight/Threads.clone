import { computed, type Ref } from 'vue'
import { formatDistanceToNow } from 'date-fns'

export function useDateFormatter(timestamp: Ref<string | null>, addSuffix: boolean = true) {
  const formattedDate = computed<string | null>(() => {
    if (!timestamp.value) return null

    return formatDistanceToNow(timestamp.value, { addSuffix })
  })

  return { formattedDate }
}
