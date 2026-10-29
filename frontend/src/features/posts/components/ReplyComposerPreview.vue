<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/features/auth/stores/authStore'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import commentApi from '@/features/posts/comments/commentApi.ts'
import type { CommentDto, CreateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import createCommentSchema from '@/features/posts/comments/schemas/createCommentSchema.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const { postId, continueThread = true } = defineProps<{
  postId: string
  continueThread?: boolean
}>()

const emit = defineEmits<{
  commentCreated: [comment: CommentDto]
}>()

const { loggedInUserInitials, loggedInUsername } = storeToRefs(useAuthStore())
const modalStore = useModalStore()
const { runOrShowLoginForm } = useLoginPrompt()
const initialCommentForm: CreateCommentDto = { content: '' }

const {
  payload: commentForm,
  errors: commentValidationErrors,
  submit: submitComment,
  isSubmitting: isCreatingComment,
} = usePayloadValidator(initialCommentForm, createCommentSchema, performCreateComment)

async function performCreateComment(validComment: CreateCommentDto) {
  try {
    const createdComment = await commentApi.addComment(postId, validComment)
    emit('commentCreated', createdComment)
    commentForm.value.content = ''
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openErrorModal('Failed to create comment. Please try again later!')
  }
}
</script>

<template>
  <section
    aria-label="Reply to this thread"
    :class="['reply-composer', { 'reply-composer--thread-end': !continueThread }]"
  >
    <div class="reply-composer__avatar-column">
      <BaseAvatar
        :initials="loggedInUserInitials ?? 'NA'"
        :label="loggedInUsername ? `${loggedInUsername}'s avatar` : 'Your profile avatar'"
      />
    </div>

    <form class="reply-composer__form" @submit.prevent="runOrShowLoginForm(submitComment)">
      <BaseTextAreaField
        v-model="commentForm.content"
        @keydown.ctrl.enter.prevent="runOrShowLoginForm(submitComment)"
        :disabled="isCreatingComment"
        :error-message="commentValidationErrors.content"
        class="reply-composer__field"
        id="replyComposer-input"
        label="Add a comment"
        placeholder="Reply to this thread..."
        variant="bare"
        visually-hidden-label
      />

      <BaseButton
        :disabled="!commentForm.content.trim() || isCreatingComment"
        :is-loading="isCreatingComment"
        class="reply-composer__submit"
        loading-text="Replying..."
        size="compact"
        type="submit"
        variant="secondary"
      >
        Reply
      </BaseButton>
    </form>
  </section>
</template>

<style scoped>
.reply-composer {
  display: grid;
  min-height: 8rem;
  padding: var(--space-6) var(--page-gutter);
  grid-template-columns: auto minmax(0, 1fr);
  align-content: center;
  gap: var(--space-3);
}

.reply-composer__avatar-column {
  position: relative;
  display: flex;
  min-width: 2.75rem;
  align-items: center;
  flex-direction: column;
  gap: var(--space-2);
}

.reply-composer__avatar-column::before {
  position: absolute;
  z-index: 0;
  inset-block: calc((var(--space-6) + 1px) * -1);
  inset-inline-start: 50%;
  width: 0.125rem;
  border-radius: var(--radius-full);
  background: var(--color-border);
  content: '';
  transform: translateX(-50%);
}

.reply-composer__avatar-column :deep(.base-avatar) {
  position: relative;
  z-index: 1;
}

.reply-composer--thread-end .reply-composer__avatar-column::before {
  inset-block-end: auto;
  height: calc(var(--space-6) + 1px + 1.25rem);
}

.reply-composer__form {
  display: grid;
  min-width: 0;
  grid-template-columns: minmax(0, 1fr) auto;
  align-content: start;
  gap: var(--space-2) var(--space-3);
}

.reply-composer__field {
  min-width: 0;
  grid-column: 1;
  grid-row: 1;
}

.reply-composer__field :deep(.base-field__input) {
  height: 6.25rem;
  min-height: 6.25rem;
  padding-inline: var(--space-4);
  border-radius: var(--radius-lg);
  background: var(--color-surface-raised);
}

.reply-composer__submit {
  grid-column: 2;
  grid-row: 1;
  align-self: center;
}

@media (max-width: 47.999rem) {
  .reply-composer__form {
    grid-template-columns: minmax(0, 1fr);
  }

  .reply-composer__field {
    grid-column: 1 / -1;
    grid-row: 1;
  }

  .reply-composer__submit {
    margin-block-start: var(--space-2);
    grid-column: 1;
    grid-row: 2;
    align-self: center;
    justify-self: end;
  }
}
</style>
