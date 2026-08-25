<script setup lang="ts">
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import commentApi from '@/features/posts/comments/commentApi.ts'
import type { CommentDto, CreateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import createCommentSchema from '@/features/posts/comments/schemas/createCommentSchema.ts'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'

const { postId } = defineProps<{
  postId: string
}>()

const emit = defineEmits<{
  commentCreated: [comment: CommentDto]
}>()

const { runOrShowLoginForm } = useLoginPrompt()

const initialCommentForm: CreateCommentDto = { content: '' }

const {
  payload: commentForm,
  errors: commentErrors,
  submit: submitComment,
  isSubmitting: isSubmittingComment,
} = usePayloadValidator<CreateCommentDto>(
  initialCommentForm,
  createCommentSchema,
  handleCommentSubmit,
)

const {
  error: createCommentApiError,
  errorMessage: createCommentApiErrorMessage,
  clearError: clearCreateCommentApiError,
} = useErrorHandler()

async function handleCommentSubmit(validComment: CreateCommentDto) {
  try {
    const createdComment = await commentApi.addComment(postId, validComment)
    emit('commentCreated', createdComment)
    commentForm.value.content = ''
  } catch (err) {
    createCommentApiError.value = err as Error
  }
}
</script>

<template>
  <section aria-label="Reply to this thread" class="reply-composer">
    <form class="reply-composer__form" @submit.prevent="runOrShowLoginForm(submitComment)">
      <BaseAvatar initials="SZ" label="Your profile avatar" />

      <div class="reply-composer__copy">
        <span class="reply-composer__label">Reply to this thread</span>
        <BaseTextField
          v-model="commentForm.content"
          :error-message="commentErrors.content"
          id="replyComposer-input"
          label="Add a comment"
          placeholder="Add to the conversation..."
          type="text"
          variant="bare"
          visually-hidden-label
        />
      </div>

      <BaseButton
        :disabled="!commentForm.content.trim() && !isSubmittingComment"
        :is-loading="isSubmittingComment"
        loading-text="Replying..."
        size="compact"
        type="submit"
        variant="secondary"
      >
        Reply
      </BaseButton>

      <AuthErrorAlert
        :message="createCommentApiErrorMessage"
        @close-alert="clearCreateCommentApiError"
      />
    </form>
  </section>
</template>

<style scoped>
.reply-composer {
  padding: var(--space-4);
  border-block-end: 1px solid var(--color-border);
}

.reply-composer__form {
  display: grid;
  width: 100%;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--space-3);
}

.reply-composer__copy {
  display: grid;
  min-width: 0;
  gap: var(--space-1);
}

.reply-composer__label {
  font-weight: var(--font-weight-semibold);
}
</style>
