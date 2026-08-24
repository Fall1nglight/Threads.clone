<script setup lang="ts">
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import { computed } from 'vue'
import { useFeedStore } from '@/features/posts/feed/feedStore.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import type { CreatePostDto } from '@/features/posts/postTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import CreatePostSchema from '@/features/posts/schemas/createPostSchema.ts'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'

const { runOrShowLoginForm } = useLoginPrompt()

const feedStore = useFeedStore()
const authStore = useAuthStore()
const { isAuthenticated, user } = storeToRefs(authStore)
const avatarInitials = computed<string>(() =>
  isAuthenticated.value ? user.value!.username.slice(0, 2) : 'You',
)

const initialCreatePostPayload: CreatePostDto = { content: '' }
const {
  payload,
  errors: validationError,
  submit: validateCreatePostPayload,
  isSubmitting: isCreatingPost,
} = usePayloadValidator<CreatePostDto>(initialCreatePostPayload, CreatePostSchema, handleCreatePost)

const { error, errorMessage, clearError } = useErrorHandler()

async function handleCreatePost(validPayload: CreatePostDto): Promise<void> {
  try {
    await feedStore.createPost(validPayload)
    payload.value.content = ''
  } catch (err) {
    error.value = err as Error
  }
}
</script>

<template>
  <section aria-label="Create a thread preview" class="composer-preview">
    <form
      class="composer-preview__form"
      @submit.prevent="runOrShowLoginForm(validateCreatePostPayload)"
    >
      <BaseAvatar :initials="avatarInitials" label="Your profile avatar" />
      <BaseTextField
        v-model="payload.content"
        :error-message="validationError.content"
        id="postComposer-input"
        label="Post content"
        placeholder="Start a thread..."
        type="text"
        variant="bare"
        visually-hidden-label
      />

      <BaseButton
        :is-loading="isCreatingPost"
        loading-text="Posting..."
        type="submit"
        size="compact"
        variant="secondary"
      >
        Post
      </BaseButton>
    </form>

    <div>
      <AuthErrorAlert :message="errorMessage" @close-alert="clearError" />
    </div>
  </section>
</template>

<style scoped>
.composer-preview {
  min-height: 5rem;
  padding: var(--space-4);
  border-block-end: 1px solid var(--color-border);
}

.composer-preview__form {
  display: grid;
  width: 100%;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--space-3);
}
</style>
