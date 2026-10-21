<script setup lang="ts">
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import commentApi from '@/features/posts/comments/commentApi.ts'
import type { CommentDto, CreateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import createCommentSchema from '@/features/posts/comments/schemas/createCommentSchema.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const { postId } = defineProps<{
  postId: string
}>()

const emit = defineEmits<{
  commentCreated: [comment: CommentDto]
}>()

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
    modalStore.openAlertModal('Failed to create comment. Please try again later!')
  }
}
</script>

<template>
  <section aria-label="Reply to this thread" class="reply-composer">
    <div class="reply-composer__avatar-column">
      <BaseAvatar initials="SZ" label="Your profile avatar" />
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
  min-height: 10rem;
  padding: var(--space-4) var(--space-4);
  grid-template-columns: auto minmax(0, 1fr);
  align-content: center;
  gap: var(--space-3);
}

.reply-composer__avatar-column {
  display: flex;
  min-width: 2.75rem;
  align-items: center;
  flex-direction: column;
  gap: var(--space-2);
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
}

.reply-composer__submit {
  grid-column: 2;
  grid-row: 1;
  align-self: end;
}

@media (max-width: 47.999rem) {
  .reply-composer__form {
    grid-template-columns: minmax(0, 1fr) auto;
  }

  .reply-composer__field {
    grid-column: 1 / -1;
    grid-row: 1;
  }

  .reply-composer__submit {
    margin-block-start: var(--space-2);
    grid-column: 2;
    grid-row: 2;
    align-self: center;
    justify-self: end;
  }
}
</style>
