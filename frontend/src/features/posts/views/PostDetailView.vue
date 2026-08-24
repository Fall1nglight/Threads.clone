<script setup lang="ts">
import PageHeader from '@/app/components/PageHeader.vue'
import PostCard from '@/features/posts/components/PostCard.vue'
import ReplyComposerPreview from '@/features/posts/components/ReplyComposerPreview.vue'
import { useFetch } from '@/shared/composables/fetch.ts'
import type { PostDto } from '@/features/posts/postTypes.ts'
import postApi from '@/features/posts/postApi.ts'
import { useRoute, useRouter } from 'vue-router'
import { computed } from 'vue'
import likeApi from '@/features/posts/likeApi.ts'
import commentApi from '@/features/posts/comments/commentApi.ts'
import CommentCard from '@/features/posts/comments/CommentCard.vue'
import FeedLoadMoreTrigger from '@/features/posts/components/FeedLoadMoreTrigger.vue'
import type { CommentDto, UpdateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import updateCommentSchema from '@/features/posts/comments/schemas/updateCommentSchema.ts'

const router = useRouter()
const route = useRoute()
const postId = computed<string>(() => String(route.params.postId))
const authStore = useAuthStore()
const { currentUserId } = storeToRefs(authStore)

const {
  status: postStatus,
  data: post,
  error: postError,
} = useFetch<PostDto>(() => postApi.getPost(postId.value))

async function handleLike(id: string, nextLikeState: boolean) {
  try {
    if (nextLikeState) {
      await likeApi.likePost(id)
    } else {
      await likeApi.unlikePost(id)
    }

    if (nextLikeState == post.value?.isLikedByCurrentUser) return

    post.value!.isLikedByCurrentUser = nextLikeState
    post.value!.likeCount = Math.max(0, post.value!.likeCount + (nextLikeState ? 1 : -1))
  } catch (err) {
    console.error(err)
  }
}

const {
  status: commentStatus,
  data: comments,
  error: commentError,
} = useFetch(() => commentApi.getComments(postId.value))

async function loadMore() {
  try {
    if (!comments.value?.hasMore) return
    if (!comments.value?.cursor) return

    const response = await commentApi.getComments(postId.value, {
      pageSize: 5,
      cursor: comments.value.cursor,
    })

    comments.value.items.push(...response.items)
    comments.value.hasMore = response.hasMore
    comments.value.cursor = response.cursor ?? null
  } catch (err) {
    console.error(err)
  }
}

function handleCommentCreated(comment: CommentDto) {
  if (comments.value?.items.some((existingComment) => existingComment.id === comment.id)) return

  if (post.value) post.value.commentCount += 1
  comments.value?.items.unshift(comment)
}

const { error, errorMessage, clearError } = useErrorHandler()

async function handleDeletePost(postId: string) {
  try {
    await postApi.deletePost(postId)
    router.push('/')
  } catch (err) {
    error.value = err as Error
  }
}

async function handleDeleteComment(commentId: string) {
  try {
    await commentApi.deleteComment(postId.value, commentId)

    if (!comments.value) return
    comments.value.items = comments.value.items.filter((comment) => comment.id !== commentId)

    if (!post.value) return
    post.value.commentCount = Math.max(0, post.value.commentCount - 1)
  } catch (err) {
    error.value = err as Error
  }
}

const initialUpdateCommentPayload: UpdateCommentDto = { id: '', content: '' }
const {
  payload: updateCommentPayload,
  errors: updateCommentValidationError,
  submit: validateCommentPayload,
  isSubmitting: isCommentSubmitting,
} = usePayloadValidator<UpdateCommentDto>(
  initialUpdateCommentPayload,
  updateCommentSchema,
  editComment,
)

function handleEditComment(commentId: string, newContent: string): void {
  updateCommentPayload.value.id = commentId
  updateCommentPayload.value.content = newContent
  validateCommentPayload()
}

async function editComment(validPayload: UpdateCommentDto) {
  try {
    await commentApi.updateComment(postId.value, validPayload)
    if (!comments.value) return
    comments.value.items = comments.value.items.map((comment) =>
      comment.id === validPayload.id ? { ...comment, content: validPayload.content } : comment,
    )
  } catch (err) {
    error.value = err as Error
  }
}
</script>

<template>
  <div>
    <PageHeader back-to="/" title="Thread" />
    <div v-if="postStatus == 'loading'">
      <h1>Loading...</h1>
    </div>
    <div v-else-if="postStatus == 'error'">
      <h1>Failed to load post.</h1>
    </div>
    <div v-else-if="postStatus == 'success' && post">
      <section aria-label="Thread conversation">
        <PostCard
          :content="post.content"
          :post-id="post.id"
          :user-id="post.user.id"
          :author-name="post.user.username"
          :username="post.user.username"
          :avatar-initials="post.user.username.slice(0, 2)"
          :createdAtUtc="post.createdAtUtc"
          :updated-at-utc="post.updatedAtUtc"
          :comment-count="post.commentCount"
          :like-count="post.likeCount"
          :is-liked-by-current-user="post.isLikedByCurrentUser"
          @toggle-like="handleLike"
          @delete-post="handleDeletePost"
          detailed
          threaded
        />

        <AuthErrorAlert :message="errorMessage" @close-alert="clearError" />

        <ReplyComposerPreview :post-id="post.id" @comment-created="handleCommentCreated" />

        <div v-if="comments?.items">
          <CommentCard
            v-for="comment in comments.items"
            :key="comment.id"
            :validation-error-message="updateCommentValidationError.content"
            :is-submitting="isCommentSubmitting"
            :id="comment.id"
            :username="comment.user.username"
            :user-id="comment.user.id"
            :author-name="comment.user.username"
            :content="comment.content"
            :avatar-initials="comment.user.username.slice(0, 2)"
            :created-at-utc="comment.createdAtUtc"
            @delete-comment="handleDeleteComment"
            @edit-comment="handleEditComment"
            :show-more-options="currentUserId === comment.user.id"
          />

          <FeedLoadMoreTrigger v-if="comments?.hasMore && comments?.cursor" @visible="loadMore" />
        </div>
      </section>
    </div>
  </div>
</template>
