<script setup lang="ts">
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt.ts'
import { computed } from 'vue'
import { useFeedStore } from '@/features/posts/feed/feedStore.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import type { CreatePostDto } from '@/features/posts/postTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import createPostSchema from '@/features/posts/schemas/createPostSchema.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const { runOrShowLoginForm } = useLoginPrompt()

const modalStore = useModalStore()
const feedStore = useFeedStore()
const authStore = useAuthStore()
const { isLoggedIn, loggedInUsername } = storeToRefs(authStore)

const loggedInUserInitials = computed(() =>
  isLoggedIn.value ? getInitials(loggedInUsername.value!) : 'NA',
)

const loggedInUserAvatarDesc = computed(() =>
  isLoggedIn.value ? `${loggedInUsername.value}'s avatar` : 'Your profile avatar',
)

const initialPostForm: CreatePostDto = { content: '' }
const {
  payload: postForm,
  errors: postValidationErrors,
  submit: submitPost,
  isSubmitting: isCreatingPost,
} = usePayloadValidator(initialPostForm, createPostSchema, performCreatePost)

async function performCreatePost(validPayload: CreatePostDto) {
  try {
    await feedStore.createPost(validPayload)
    postForm.value.content = ''
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openAlertModal('Failed to create post. Please try again later!')
  }
}
</script>

<template>
  <section aria-label="Create a thread preview" class="composer-preview">
    <form class="composer-preview__form" @submit.prevent="runOrShowLoginForm(submitPost)">
      <BaseAvatar
        class="composer-preview__avatar"
        :initials="loggedInUserInitials"
        :label="loggedInUserAvatarDesc"
        :aria-label="loggedInUserAvatarDesc"
      />

      <BaseTextAreaField
        v-model="postForm.content"
        @keydown.ctrl.enter.prevent="runOrShowLoginForm(submitPost)"
        :disabled="isCreatingPost"
        :error-message="postValidationErrors.content"
        id="postComposer-input"
        label="Post content"
        placeholder="Start a thread..."
        visually-hidden-label
      />

      <BaseButton
        class="composer-preview__submit"
        :disabled="!postForm.content.length"
        :is-loading="isCreatingPost"
        loading-text="Posting..."
        type="submit"
        size="compact"
        variant="secondary"
      >
        Post
      </BaseButton>
    </form>
  </section>
</template>

<style scoped>
.composer-preview {
  display: flex;
  min-height: 7rem;
  padding: var(--space-8) var(--space-4);
  align-items: center;
  border-block-end: 1px solid var(--color-border);
}

.composer-preview__form {
  display: grid;
  width: 100%;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: var(--space-3);
}

.composer-preview__avatar {
  align-self: start;
}

.composer-preview__submit {
  align-self: end;
}
</style>
