<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import PostActionBar from '@/features/posts/components/PostActionBar.vue'
import { formatDistanceToNow } from 'date-fns'
import { computed, onBeforeUnmount, ref, useTemplateRef, watch } from 'vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'

const props = withDefaults(
  defineProps<{
    postId: string
    userId: string
    authorName: string
    username: string
    avatarInitials: string
    createdAtUtc: string
    updatedAtUtc: string | null
    content: string
    commentCount: number
    likeCount: number
    mediaLabel?: string
    threaded?: boolean
    detail?: boolean
    verified?: boolean
    isLikedByCurrentUser: boolean
  }>(),
  {
    mediaLabel: '',
    threaded: false,
    detail: false,
    verified: false,
  },
)

const emit = defineEmits<{
  toggleLike: [postId: string, nextLikeState: boolean]
  deletePost: [postId: string]
  showComments: [postId: string]
}>()

const router = useRouter()
const authStore = useAuthStore()
const { currentUserId } = storeToRefs(authStore)
const isPostOwner = computed(() => currentUserId.value === props.userId)
const isPostActionsOpen = ref(false)
const postActions = useTemplateRef<HTMLElement>('postActions')
const postActionsPanelId = `post-actions-${props.postId}`

function togglePostActions() {
  isPostActionsOpen.value = !isPostActionsOpen.value
}

function closePostActions(restoreFocus = false) {
  if (!isPostActionsOpen.value) return

  isPostActionsOpen.value = false

  if (restoreFocus) {
    postActions.value?.querySelector<HTMLButtonElement>('button[aria-expanded]')?.focus()
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isPostActionsOpen.value || !(event.target instanceof Node)) return
  if (postActions.value?.contains(event.target)) return

  closePostActions()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!isPostActionsOpen.value || !(event.target instanceof Node)) return
  if (postActions.value?.contains(event.target)) return

  closePostActions()
}

function handleViewportResize() {
  closePostActions()
}

function handleEditPost() {
  closePostActions()
  void router.push({ name: 'post-edit', params: { postId: props.postId } })
}

function handleDeletePost(postId: string) {
  closePostActions()
  if (!confirm('Are you sure you want to delete this post?')) return
  emit('deletePost', postId)
}

function removeOpenMenuListeners() {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportResize)
}

watch(isPostActionsOpen, (isOpen) => {
  if (!isOpen) {
    removeOpenMenuListeners()
    return
  }

  document.addEventListener('pointerdown', handleDocumentPointerDown)
  document.addEventListener('focusin', handleDocumentFocusIn)
  window.addEventListener('resize', handleViewportResize)
})

watch(isPostOwner, (isOwner) => {
  if (!isOwner) closePostActions()
})

onBeforeUnmount(removeOpenMenuListeners)
</script>

<template>
  <article :class="['post-card', { 'post-card--detail': detail }]">
    <div class="post-card__avatar-column">
      <RouterLink :aria-label="`View ${authorName}'s profile`" :to="`/users/${userId}`">
        <BaseAvatar :initials="avatarInitials" :label="`${authorName}'s avatar`" />
      </RouterLink>
      <span v-if="threaded" aria-hidden="true" class="post-card__thread-line" />
    </div>

    <div class="post-card__body">
      <header class="post-card__header">
        <div class="post-card__identity">
          <RouterLink class="post-card__author" :to="`/users/${userId}`">
            {{ authorName }}
          </RouterLink>
          <AppIcon v-if="verified" class="post-card__verified" name="verified" />
          <span class="post-card__username">@{{ username }}</span>
          <span aria-hidden="true">·</span>
          <RouterLink class="post-card__timestamp" :to="`/posts/${postId}`">
            {{ formatDistanceToNow(createdAtUtc, { addSuffix: true }) }}
          </RouterLink>
          <span aria-hidden="true">·</span>
          <span v-if="updatedAtUtc">
            {{ formatDistanceToNow(updatedAtUtc, { addSuffix: true }) }}
          </span>
        </div>
        <div
          v-if="isPostOwner"
          ref="postActions"
          class="post-card__actions"
          @keydown.esc.stop.prevent="closePostActions(true)"
        >
          <BaseIconButton
            :aria-controls="postActionsPanelId"
            :aria-expanded="isPostActionsOpen"
            :active="isPostActionsOpen"
            label="More thread options"
            @click="togglePostActions"
          >
            <AppIcon name="more" />
          </BaseIconButton>

          <div
            v-if="isPostActionsOpen"
            :id="postActionsPanelId"
            aria-label="Post actions"
            class="post-card__actions-panel"
            role="group"
          >
            <button class="post-card__action-item" type="button" @click="handleEditPost">
              Edit
            </button>
            <button
              class="post-card__action-item post-card__action-item--danger"
              type="button"
              @click="handleDeletePost(postId)"
            >
              Delete
            </button>
          </div>
        </div>
      </header>

      <p class="post-card__content">{{ content }}</p>

      <figure v-if="mediaLabel" class="post-card__media">
        <div class="post-card__media-window">
          <span class="post-card__media-label">{{ mediaLabel }}</span>
          <div class="post-card__media-grid">
            <span>Tokens</span>
            <span>Components</span>
            <span>Views</span>
          </div>
        </div>
        <figcaption class="sr-only">A compact interface system preview</figcaption>
      </figure>

      <PostActionBar
        :is-liked-by-current-user="isLikedByCurrentUser"
        :like-count="likeCount"
        :comment-count="commentCount"
        @show-comments="emit('showComments', postId)"
        @toggle-like="(nextLikeState: boolean) => emit('toggleLike', postId, nextLikeState)"
      />
    </div>
  </article>
</template>

<style scoped>
.post-card {
  display: grid;
  padding: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.post-card--detail {
  padding-block-start: var(--space-6);
}

.post-card__avatar-column {
  display: flex;
  min-width: 2.75rem;
  align-items: center;
  flex-direction: column;
  gap: var(--space-2);
}

.post-card__avatar-column a {
  border-radius: var(--radius-full);
  text-decoration: none;
}

.post-card__thread-line {
  width: 0.125rem;
  min-height: 3rem;
  flex: 1;
  border-radius: var(--radius-full);
  background: var(--color-border);
}

.post-card__body {
  min-width: 0;
}

.post-card__header {
  display: flex;
  min-height: 2.75rem;
  margin-block-start: calc(var(--space-1) * -1);
  align-items: center;
  justify-content: space-between;
  gap: var(--space-2);
}

.post-card__identity {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: var(--space-1);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  white-space: nowrap;
}

.post-card__actions {
  position: relative;
  display: flex;
  flex: 0 0 auto;
}

.post-card__actions-panel {
  position: absolute;
  z-index: 10;
  inset-block-start: calc(100% + var(--space-1));
  inset-inline-end: 0;
  display: grid;
  width: max-content;
  min-width: 12rem;
  max-width: calc(100vw - var(--space-8));
  padding: var(--space-1);
  gap: var(--space-2);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  box-shadow: 0 var(--space-2) var(--space-6) rgb(0 0 0 / 18%);
}

.post-card__action-item {
  display: flex;
  width: 100%;
  min-height: var(--control-height);
  padding-inline: var(--space-3);
  align-items: center;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text);
  font: inherit;
  font-weight: var(--font-weight-semibold);
  text-align: start;
  white-space: nowrap;
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    color var(--transition-fast);
}

.post-card__action-item--danger {
  color: var(--color-danger);
}

.post-card__action-item:hover {
  background: var(--color-surface-subtle);
}

.post-card__action-item:active {
  background: var(--color-border);
}

.post-card__author {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  overflow: hidden;
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
  text-overflow: ellipsis;
  text-decoration: none;
}

.post-card__author:hover,
.post-card__timestamp:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.post-card__verified {
  width: 1rem;
  height: 1rem;
  color: var(--color-text);
}

.post-card__username {
  max-width: 8.5rem;
  overflow: hidden;
  text-overflow: ellipsis;
}

.post-card__timestamp {
  display: inline-flex;
  min-width: var(--control-height);
  min-height: var(--control-height);
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  text-decoration: none;
}

.post-card__content {
  max-width: 65ch;
  margin-block-end: var(--space-2);
  overflow-wrap: anywhere;
  line-height: 1.5;
  text-wrap: pretty;
}

.post-card__media {
  margin-block: var(--space-3) var(--space-2);
}

.post-card__media-window {
  display: grid;
  min-height: 13rem;
  padding: var(--space-4);
  align-content: space-between;
  gap: var(--space-6);
  overflow: hidden;
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface-subtle);
  color: var(--color-media-ink);
}

.post-card__media-label {
  max-width: 14rem;
  font-size: clamp(1.5rem, 7vw, 2rem);
  font-weight: var(--font-weight-bold);
  line-height: 1;
  text-wrap: balance;
}

.post-card__media-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-2);
}

.post-card__media-grid span {
  display: grid;
  min-height: 3rem;
  padding: var(--space-2);
  place-items: center;
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  font-size: var(--font-size-micro);
  font-weight: var(--font-weight-semibold);
}

@media (max-width: 28rem) {
  .post-card__username {
    display: none;
  }
}
</style>
