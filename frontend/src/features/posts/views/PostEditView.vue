<script setup lang="ts">
import { computed, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/app/components/PageHeader.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import PostEditCard from '@/features/posts/components/PostEditCard.vue'
import postApi from '@/features/posts/postApi.ts'
import type { PostDto, UpdatePostDto } from '@/features/posts/postTypes.ts'
import { useFetch } from '@/shared/composables/fetch.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import updatePostSchema from '@/features/posts/schemas/updatePostSchema.ts'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { currentUserId, sessionStatus } = storeToRefs(authStore)
const postId = computed(() => String(route.params.postId))
const postDetailPath = computed(() => `/posts/${postId.value}`)
const isSessionPending = computed(
  () => sessionStatus.value === 'uninitialized' || sessionStatus.value === 'restoring',
)

const { status: postStatus, data: post } = useFetch<PostDto>(() => postApi.getPost(postId.value))

const isPostOwner = computed(
  () =>
    sessionStatus.value === 'authenticated' &&
    post.value !== null &&
    currentUserId.value === post.value.user.id,
)

watch(
  [postStatus, sessionStatus, post, currentUserId],
  ([status, currentSessionStatus, currentPost, userId]) => {
    if (currentSessionStatus === 'uninitialized' || currentSessionStatus === 'restoring') {
      return
    }

    if (currentSessionStatus !== 'authenticated') {
      void router.replace({ name: 'post-detail', params: { postId: postId.value } })
      return
    }

    if (status === 'success' && currentPost !== null && userId !== currentPost.user.id) {
      void router.replace({ name: 'post-detail', params: { postId: postId.value } })
    }
  },
  { immediate: true },
)

const initialPayload: UpdatePostDto = {
  id: '',
  content: '',
}

const {
  payload,
  errors: validationError,
  submit,
  isSubmitting,
} = usePayloadValidator<UpdatePostDto>(initialPayload, updatePostSchema, handleUpdate)

const { error, errorMessage, clearError } = useErrorHandler()

function handleSave(content: string): void {
  payload.value.content = content
  payload.value.id = postId.value
  submit()
}

async function handleUpdate(validPayload: UpdatePostDto): Promise<void> {
  try {
    await postApi.updatePost(validPayload)
    await router.push(postDetailPath.value)
  } catch (err) {
    error.value = err as Error
  }
}
</script>

<template>
  <div>
    <PageHeader :back-to="postDetailPath" title="Edit thread" />

    <div v-if="postStatus === 'loading' || isSessionPending">
      <h1>Loading...</h1>
    </div>
    <div v-else-if="postStatus === 'error'">
      <h1>Failed to load post.</h1>
    </div>
    <PostEditCard
      v-else-if="postStatus === 'success' && post && isPostOwner"
      :post="post"
      :is-submitting="isSubmitting"
      :validation-error-message="validationError.content"
      @handle-save="handleSave"
    />

    <AuthErrorAlert :message="errorMessage" @close-alert="clearError" />
  </div>
</template>
