<script setup lang="ts">
import { computed, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { type RouteLocationRaw, useRoute, useRouter } from 'vue-router'
import PageHeader from '@/app/components/PageHeader.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import PostEditCard from '@/features/posts/components/PostEditCard.vue'
import postApi from '@/features/posts/postApi.ts'
import type { UpdatePostDto } from '@/features/posts/postTypes.ts'
import { useFetch } from '@/shared/composables/useFetch.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import updatePostSchema from '@/features/posts/schemas/updatePostSchema.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'

const route = useRoute()
const router = useRouter()
const modalStore = useModalStore()
const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const postId = computed(() => String(route.params.postId))
const postPage = computed<RouteLocationRaw>(() => ({
  name: 'post-detail',
  params: { postId: postId.value },
}))

const { state: postState } = useFetch(postId, (currentPostId) => postApi.getPost(currentPostId))
const post = computed(() => (postState.value.status === 'success' ? postState.value.item : null))

const isPostOwner = computed(
  () => post.value !== null && loggedInUserId.value === post.value.user.id,
)

// Watch the post too so a loaded post redirects visitors even when isPostOwner stays false.
watch([isPostOwner, post], ([isOwner, newPost]) => {
  if (newPost && !isOwner) router.replace(postPage.value)
})

const initialPostForm: UpdatePostDto = {
  id: '',
  content: '',
}

const {
  payload: postForm,
  errors: postValidationErrors,
  submit: submitPost,
  isSubmitting: isSubmittingPost,
} = usePayloadValidator(initialPostForm, updatePostSchema, performUpdatePost)

function handleSave(content: string): void {
  postForm.value.content = content
  postForm.value.id = postId.value
  submitPost()
}

async function performUpdatePost(validPayload: UpdatePostDto): Promise<void> {
  try {
    await postApi.updatePost(validPayload)
    await router.push(postPage.value)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openAlertModal('Failed to update post. Please try again later!')
  }
}
</script>

<template>
  <div>
    <PageHeader :back-to="postPage" title="Edit thread" />

    <Transition name="async-fade" mode="out-in">
      <BaseLoader
        v-if="postState.status === 'loading'"
        key="post-edit-loader"
        message="Loading post..."
      />

      <BaseErrorMessage
        v-else-if="postState.status === 'error'"
        key="post-edit-error"
        message="Failed to load this post. Please try again later!"
      />

      <PostEditCard
        v-else-if="post && isPostOwner"
        key="post-edit-content"
        :post="post"
        :is-submitting="isSubmittingPost"
        :validation-error-message="postValidationErrors.content"
        @handle-save="handleSave"
      />
    </Transition>
  </div>
</template>
