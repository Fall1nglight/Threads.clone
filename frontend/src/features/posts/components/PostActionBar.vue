<script setup lang="ts">
import AppIcon from '@/shared/icons/AppIcon.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'

const {
  commentCount,
  likeCount,
  isLikedByCurrentUser,
  disabled = false,
} = defineProps<{
  commentCount: number
  likeCount: number
  isLikedByCurrentUser: boolean
  disabled?: boolean
}>()

const emit = defineEmits<{
  showComments: []
  toggleLike: [nextLikeState: boolean]
  openShareDialog: []
}>()

const { runOrShowLoginForm } = useLoginPrompt()

function handleCommentButtonClick() {
  emit('showComments')
}

function handleLikeButtonClick() {
  emit('toggleLike', !isLikedByCurrentUser)
}

function handleShareButtonClick() {
  emit('openShareDialog')
}
</script>

<template>
  <div aria-label="Thread actions" class="post-actions">
    <button
      @click="runOrShowLoginForm(handleCommentButtonClick)"
      :aria-label="`Comment, ${commentCount} comments`"
      class="post-actions__button"
      :disabled="disabled"
      type="button"
    >
      <AppIcon name="reply" />
      <span>{{ commentCount }}</span>
    </button>

    <button
      @click="runOrShowLoginForm(handleLikeButtonClick)"
      :aria-label="`Like, ${likeCount} likes`"
      class="post-actions__button"
      :disabled="disabled"
      type="button"
    >
      <AppIcon :name="isLikedByCurrentUser ? 'heart-filled' : 'heart'" />
      <span>{{ likeCount }}</span>
    </button>

    <button
      @click="runOrShowLoginForm(handleShareButtonClick)"
      aria-label="Share thread"
      class="post-actions__button post-actions__button--icon"
      :disabled="disabled"
      type="button"
    >
      <AppIcon name="send" />
    </button>
  </div>
</template>

<style scoped>
.post-actions {
  display: flex;
  margin-inline-start: calc(var(--space-2) * -1);
  align-items: center;
  gap: var(--space-1);
}

.post-actions__button {
  display: inline-flex;
  min-width: 2.75rem;
  min-height: 2.75rem;
  padding-inline: var(--space-2);
  align-items: center;
  justify-content: center;
  gap: var(--space-1);
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast),
    transform 90ms ease-out;
}

.post-actions__button:hover:not(:disabled) {
  background: var(--color-surface-subtle);
  color: var(--color-text);
}

.post-actions__button:active:not(:disabled) {
  transform: scale(0.96);
}

.post-actions__button:disabled {
  color: var(--color-disabled-text);
  cursor: not-allowed;
}

.post-actions__button--icon {
  margin-inline-start: auto;
}
</style>
