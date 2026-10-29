<script setup lang="ts">
import { type RouteLocationRaw, RouterLink, useRouter } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import PostActionBar from '@/features/posts/components/PostActionBar.vue'
import { computed, onBeforeUnmount, ref, toRefs, useTemplateRef, watch } from 'vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'

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
    avatarInitials: 'NA',
    mediaLabel: '',
    threaded: false,
    detail: false,
    verified: false,
  },
)

const emit = defineEmits<{
  toggleLike: []
  deletePost: []
  showComments: []
}>()

const { createdAtUtc, updatedAtUtc } = toRefs(props)
const { formattedDate: formattedCreatedAtUtc } = useDateFormatter(createdAtUtc)
const { formattedDate: formattedUpdatedAtUtc } = useDateFormatter(updatedAtUtc)

const router = useRouter()
const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const isPostOwner = computed(() => loggedInUserId.value === props.userId)
const isPostActionsMenuOpen = ref(false)
const postActionsMenu = useTemplateRef<HTMLElement>('postActionsMenu')
const postActionsMenuPanelId = `post-actions-${props.postId}`

const postPage = computed<RouteLocationRaw>(() => ({
  name: 'post-detail',
  params: { postId: props.postId },
}))

const authorPage = computed<RouteLocationRaw>(() => ({
  name: 'user-profile',
  params: { userId: props.userId },
}))
const authorPageDesc = computed(() => `View ${props.authorName}'s profile`)
const authorAvatarDesc = computed(() => `${props.authorName}'s avatar`)

function togglePostActionsMenu() {
  isPostActionsMenuOpen.value = !isPostActionsMenuOpen.value
}

function closePostActionsMenu(restoreFocus = false) {
  if (!isPostActionsMenuOpen.value) return

  isPostActionsMenuOpen.value = false

  if (restoreFocus) {
    postActionsMenu.value?.querySelector<HTMLButtonElement>('button[aria-expanded]')?.focus()
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isPostActionsMenuOpen.value || !(event.target instanceof Node)) return
  if (postActionsMenu.value?.contains(event.target)) return

  closePostActionsMenu()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!isPostActionsMenuOpen.value || !(event.target instanceof Node)) return
  if (postActionsMenu.value?.contains(event.target)) return

  closePostActionsMenu()
}

function handleViewportResize() {
  closePostActionsMenu()
}

function handleEditPost() {
  closePostActionsMenu()
  void router.replace({ name: 'post-edit', params: { postId: props.postId } })
}

function handleDeletePost() {
  closePostActionsMenu()
  if (!confirm('Are you sure you want to delete this post?')) return
  emit('deletePost')
}

function removeOpenMenuListeners() {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportResize)
}

watch(isPostActionsMenuOpen, (isOpen) => {
  if (!isOpen) {
    removeOpenMenuListeners()
    return
  }

  document.addEventListener('pointerdown', handleDocumentPointerDown)
  document.addEventListener('focusin', handleDocumentFocusIn)
  window.addEventListener('resize', handleViewportResize)
})

watch(isPostOwner, (isOwner) => {
  if (!isOwner) closePostActionsMenu()
})

onBeforeUnmount(removeOpenMenuListeners)
</script>

<template>
  <article :class="['post-card', { 'post-card--detail': detail }]">
    <div class="post-card__avatar-column">
      <RouterLink :to="authorPage" :aria-label="authorPageDesc">
        <BaseAvatar :initials="avatarInitials" :label="authorAvatarDesc" />
      </RouterLink>
      <span v-if="threaded" aria-hidden="true" class="post-card__thread-line" />
    </div>

    <div class="post-card__body">
      <header class="post-card__header">
        <div class="post-card__identity">
          <RouterLink class="post-card__author" :to="authorPage" :aria-label="authorPageDesc">
            {{ authorName }}
          </RouterLink>

          <AppIcon v-if="verified" class="post-card__verified" name="verified" />

          <span aria-hidden="true">·</span>

          <RouterLink v-if="formattedCreatedAtUtc" :to="postPage" class="post-card__timestamp">
            {{ formattedCreatedAtUtc }}
          </RouterLink>
        </div>

        <div
          v-if="isPostOwner"
          ref="postActionsMenu"
          class="post-card__actions"
          @keydown.esc.stop.prevent="closePostActionsMenu(true)"
        >
          <BaseIconButton
            :aria-controls="postActionsMenuPanelId"
            :aria-expanded="isPostActionsMenuOpen"
            :active="isPostActionsMenuOpen"
            label="More thread options"
            @click="togglePostActionsMenu"
          >
            <AppIcon name="more" />
          </BaseIconButton>

          <div
            v-if="isPostActionsMenuOpen"
            :id="postActionsMenuPanelId"
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
              @click="handleDeletePost"
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

      <div class="post-card__footer">
        <PostActionBar
          :is-liked-by-current-user="isLikedByCurrentUser"
          :like-count="likeCount"
          :comment-count="commentCount"
          @show-comments="emit('showComments')"
          @toggle-like="emit('toggleLike')"
        />
        <time v-if="formattedUpdatedAtUtc" class="post-card__updated-at">
          Updated {{ formattedUpdatedAtUtc }}
        </time>
      </div>
    </div>
  </article>
</template>

<style scoped>
.post-card {
  display: grid;
  min-height: 8rem;
  padding: var(--space-4) var(--page-gutter);
  padding-block-end: var(--space-2);
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.post-card--detail {
  min-height: 10rem;
  padding-block: var(--space-6) var(--space-2);
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
  display: grid;
  min-width: 0;
  align-content: start;
}

.post-card__header {
  display: flex;
  min-height: 2rem;
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
  border-radius: var(--radius-lg);
  background: var(--color-surface-raised);
  box-shadow: var(--shadow-menu);
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
  min-height: 2rem;
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

.post-card__timestamp {
  display: inline-flex;
  min-width: var(--control-height);
  min-height: var(--control-height);
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  text-decoration: none;
}

.post-card__footer {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.post-card__updated-at {
  margin-inline-start: auto;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-align: end;
  overflow-wrap: anywhere;
}

.post-card__content {
  min-height: 0;
  max-width: 65ch;
  padding-block: var(--space-1) var(--space-2);
  overflow-wrap: anywhere;
  line-height: 1.5;
  text-wrap: pretty;
  white-space: pre-wrap;
}

.post-card__media {
  margin-block: var(--space-3) var(--space-2);
}

.post-card__media-window {
  display: grid;
  min-height: 13rem;
  padding: var(--space-4) var(--page-gutter);
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
</style>
