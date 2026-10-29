<script setup lang="ts">
import PageHeader from '@/app/components/PageHeader.vue'
import PostCard from '@/features/posts/components/PostCard.vue'
import ReplyComposerPreview from '@/features/posts/components/ReplyComposerPreview.vue'

import postApi from '@/features/posts/postApi.ts'
import { useRoute, useRouter } from 'vue-router'
import { computed, ref } from 'vue'
import commentApi from '@/features/posts/comments/commentApi.ts'
import CommentCard from '@/features/posts/comments/components/CommentCard.vue'
import PaginationObserver from '@/shared/components/PaginationObserver.vue'
import type { CommentDto, UpdateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import updateCommentSchema from '@/features/posts/comments/schemas/updateCommentSchema.ts'
import { useToggleLike } from '@/features/posts/composables/useToggleLike.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import { usePaginatedFetch } from '@/shared/composables/usePaginatedFetch.ts'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import { useFetch } from '@/shared/composables/useFetch.ts'
import BaseEmptyMessage from '@/shared/components/BaseEmptyMessage.vue'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const router = useRouter()
const route = useRoute()
const postId = computed(() => String(route.params.postId))
const authStore = useAuthStore()
const modalStore = useModalStore()
const { loggedInUserId } = storeToRefs(authStore)

// ========= POSTS =========
// FETCH POST
const { state: postState } = useFetch(postId, (currentPostId) => postApi.getPost(currentPostId))
const post = computed(() => (postState.value.status === 'success' ? postState.value.item : null))

// DELETE POST
async function handleDeletePostEvent(postId: string) {
  try {
    await postApi.deletePost(postId)
    await router.replace({ name: 'home' })
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openErrorModal('Failed to delete post. Please try again later!')
  }
}

// ========= LIKES =========
const { toggleLike } = useToggleLike()

// ========= COMMENTS =========
// FETCH COMMENTS
const {
  state: commentsState,
  canLoadMore: canLoadMoreComments,
  loadMore: loadMoreComments,
} = usePaginatedFetch(postId, (currentPostId, request) =>
  commentApi.getComments(currentPostId, request),
)

// Keep loaded comments mounted while fetching another page or displaying a pagination error.
const comments = computed(() => ('items' in commentsState.value ? commentsState.value.items : null))

// CREATE COMMENTS
function handleCreateCommentEvent(comment: CommentDto) {
  const _post = post.value
  const _comments = comments.value

  if (!_post || !_comments) return
  if (_comments.some((existingComment) => existingComment.id === comment.id)) return

  _post.commentCount += 1
  _comments.unshift(comment)
}

// EDIT COMMENTS
const initialUpdateCommentForm: UpdateCommentDto = { id: '', content: '' }
const {
  payload: updateCommentForm,
  errors: updateCommentValidationErrors,
  submit: submitUpdateComment,
  isSubmitting: isUpdatingComment,
} = usePayloadValidator(initialUpdateCommentForm, updateCommentSchema, performUpdateComment)

function handleEditCommentEvent(commentId: string, newContent: string): void {
  updateCommentForm.value.id = commentId
  updateCommentForm.value.content = newContent
  submitUpdateComment()
}

const updateCommentError = ref<boolean>(false)

async function performUpdateComment(validPayload: UpdateCommentDto) {
  if (commentsState.value.status !== 'success') return
  const _comments = commentsState.value

  try {
    await commentApi.updateComment(postId.value, validPayload)
    _comments.items = _comments.items.map((comment) =>
      comment.id === validPayload.id
        ? { ...comment, content: validPayload.content, updatedAtUtc: new Date().toUTCString() }
        : comment,
    )

    updateCommentError.value = false
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    updateCommentError.value = true
    modalStore.openErrorModal('Failed to update comment. Please try again later!')
  }
}

// DELETE COMMENTS
async function handleDeleteCommentEvent(commentId: string) {
  if (commentsState.value.status !== 'success') return

  const _post = post.value
  const _comments = commentsState.value

  if (!_post || !_comments) return

  try {
    await commentApi.deleteComment(postId.value, commentId)

    _comments.items = _comments.items.filter((comment) => comment.id !== commentId)
    _post.commentCount = Math.max(0, _post.commentCount - 1)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
  } catch (err) {
    modalStore.openErrorModal('Failed to delete comment. Please try again later!')
  }
}
</script>

<template>
  <PageHeader back-to="/" title="Thread" />

  <Transition name="async-fade" mode="out-in">
    <!-- Post related loader, error handler -->
    <BaseLoader
      v-if="postState.status === 'loading'"
      key="post-detail-loader"
      message="Loading post..."
    />

    <BaseErrorMessage
      v-else-if="postState.status === 'error'"
      key="post-detail-error"
      message="Failed to load this post. Please try again later!"
    />

    <section
      v-else-if="post"
      key="post-detail-content"
      aria-label="Thread conversation"
      class="post-detail"
    >
      <!-- Display post -->
      <PostCard
        class="post-detail__post"
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
        @toggle-like="toggleLike(post)"
        @delete-post="handleDeletePostEvent(postId)"
        detail
        threaded
      />

      <div class="post-detail__thread-flow">
        <div class="post-detail__thread-rail">
          <!-- Add post -->
          <ReplyComposerPreview
            class="post-detail__reply-composer"
            :continue-thread="Boolean(comments?.length)"
            :post-id="post.id"
            @comment-created="handleCreateCommentEvent"
          />

          <TransitionGroup name="async-list-fade" tag="div" class="post-detail__comments">
            <CommentCard
              v-for="comment in comments ?? []"
              :key="comment.id"
              class="post-detail__comment"
              :validation-error-message="updateCommentValidationErrors.content"
              :is-submitting="isUpdatingComment"
              :id="comment.id"
              :username="comment.user.username"
              :user-id="comment.user.id"
              :author-name="comment.user.username"
              :content="comment.content"
              :avatar-initials="getInitials(comment.user.username)"
              :created-at-utc="comment.createdAtUtc"
              :updated-at-utc="comment.updatedAtUtc"
              @delete-comment="handleDeleteCommentEvent"
              @edit-comment="handleEditCommentEvent"
              :show-more-options="loggedInUserId === comment.user.id"
              v-model:api-error="updateCommentError"
            />
          </TransitionGroup>
        </div>

        <div class="post-detail__comment-status">
          <Transition name="async-fade" mode="out-in">
            <BaseLoader
              v-if="commentsState.status === 'initial-loading'"
              key="comments-loader"
              message="Loading comments..."
            />

            <BaseErrorMessage
              v-else-if="commentsState.status === 'initial-error'"
              key="comments-error"
              message="Failed to load comments. Please try again later!"
            />

            <BaseEmptyMessage
              v-else-if="comments && !comments.length"
              key="comments-empty"
              message="There are no comments on this post. Be the first to comment!"
            />
          </Transition>

          <PaginationObserver v-if="canLoadMoreComments" @visible="loadMoreComments" />

          <Transition name="async-fade" mode="out-in">
            <BaseLoader
              v-if="commentsState.status === 'more-loading'"
              key="comments-more-loader"
              message="Loading more comments..."
            />

            <BaseErrorMessage
              v-else-if="commentsState.status === 'more-error'"
              key="comments-more-error"
              message="Failed to load more comments. Please try again later!"
            />
          </Transition>
        </div>
      </div>
    </section>
  </Transition>
</template>

<style scoped>
.post-detail__thread-flow {
  --thread-avatar-column-width: 2.75rem;
  --thread-line-width: 0.125rem;
  --thread-comment-indent: var(--space-8);
  --thread-rail-offset: calc(
    var(--page-gutter) + var(--thread-avatar-column-width) / 2 - var(--thread-comment-indent)
  );

  margin-top: 0;
}
.post-detail__post {
  border-bottom: 1px solid var(--color-border);
}
.post-detail__thread-rail {
  position: relative;
}
.post-detail__comments {
  position: relative;
  margin-inline-start: var(--thread-comment-indent);
}
.post-detail__comments::before {
  position: absolute;
  inset-block: 0;
  inset-inline-start: var(--thread-rail-offset);
  width: var(--thread-line-width);
  border-radius: var(--radius-full);
  background: var(--color-border);
  content: '';
  transform: translateX(-50%);
}
.post-detail__comment::before {
  position: absolute;
  z-index: 0;
  inset-block-start: calc(var(--space-4) + 1.25rem);
  inset-inline-start: var(--thread-rail-offset);
  width: calc(var(--page-gutter) - var(--thread-rail-offset));
  height: var(--thread-line-width);
  border-radius: var(--radius-full);
  background: var(--color-border);
  content: '';
  transform: translateY(-50%);
}
.post-detail__comment:last-child::after {
  position: absolute;
  z-index: 1;
  inset-block: calc(var(--space-4) + 1.25rem + 0.0625rem) 0;
  inset-inline-start: var(--thread-rail-offset);
  width: var(--thread-line-width);
  background: var(--color-surface);
  content: '';
  transform: translateX(-50%);
}
.post-detail__comment-status {
  padding-bottom: var(--space-6);
}
</style>
