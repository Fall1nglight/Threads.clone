<script setup lang="ts">
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import commentApi, { type CreateCommentDto } from '@/features/posts/comments/commentApi.ts'
import type { CommentDto } from '@/features/posts/comments/commentTypes.ts'
import { ref } from 'vue'

const { postId } = defineProps<{
  postId: string
}>()

const emit = defineEmits<{
  commentCreated: [comment: CommentDto]
}>()

const { runOrShowLoginForm } = useLoginPrompt()
const payload = ref<CreateCommentDto>({ content: '' })
const isSubmitting = ref(false)

async function handleSubmit() {
  const content = payload.value.content.trim()

  if (!content || isSubmitting.value) return

  isSubmitting.value = true

  try {
    const comment = await commentApi.addComment(postId, { content })
    payload.value.content = ''
    emit('commentCreated', comment)
  } catch (err) {
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <section aria-label="Reply to this thread" class="reply-composer">
    <form class="reply-composer__form" @submit.prevent="runOrShowLoginForm(handleSubmit)">
      <BaseAvatar initials="SZ" label="Your profile avatar" />

      <div class="reply-composer__copy">
        <span class="reply-composer__label">Reply to this thread</span>
        <BaseTextField
          v-model="payload.content"
          id="replyComposer-input"
          label="Add a comment"
          placeholder="Add to the conversation..."
          type="text"
          variant="bare"
          visually-hidden-label
        />
      </div>

      <BaseButton
        :disabled="!payload.content.trim() || isSubmitting"
        size="compact"
        type="submit"
        variant="secondary"
      >
        {{ isSubmitting ? 'Replying...' : 'Reply' }}
      </BaseButton>
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
