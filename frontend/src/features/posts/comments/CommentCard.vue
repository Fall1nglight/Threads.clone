<script setup lang="ts">
import { RouterLink } from 'vue-router'
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import { onBeforeUnmount, ref, toRefs, useId, useTemplateRef, watch } from 'vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'

const props = withDefaults(
  defineProps<{
    id: string
    userId: string
    authorName: string
    username: string
    avatarInitials: string
    createdAtUtc: string
    content: string
    verified?: boolean
    showMoreOptions?: boolean
    isSubmitting: boolean
    validationErrorMessage: string | null
  }>(),
  {
    verified: false,
    showMoreOptions: true,
  },
)

const { isSubmitting, validationErrorMessage } = toRefs(props)

const isEditing = ref<boolean>(false)

const emit = defineEmits<{
  deleteComment: [commentId: string]
  editComment: [commentId: string, content: string]
}>()

const isCommentActionsOpen = ref(false)
const commentActions = useTemplateRef<HTMLElement>('commentActions')
const commentActionsPanelId = `comment-actions-${useId()}`

function toggleCommentActions() {
  isCommentActionsOpen.value = !isCommentActionsOpen.value
}

function closeCommentActions(restoreFocus = false) {
  if (!isCommentActionsOpen.value) return

  isCommentActionsOpen.value = false

  if (restoreFocus) {
    commentActions.value?.querySelector<HTMLButtonElement>('button[aria-expanded]')?.focus()
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isCommentActionsOpen.value || !(event.target instanceof Node)) return
  if (commentActions.value?.contains(event.target)) return

  closeCommentActions()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!isCommentActionsOpen.value || !(event.target instanceof Node)) return
  if (commentActions.value?.contains(event.target)) return

  closeCommentActions()
}

function handleViewportResize() {
  closeCommentActions()
}

function removeOpenMenuListeners() {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportResize)
}

watch(isCommentActionsOpen, (isOpen) => {
  if (!isOpen) {
    removeOpenMenuListeners()
    return
  }

  document.addEventListener('pointerdown', handleDocumentPointerDown)
  document.addEventListener('focusin', handleDocumentFocusIn)
  window.addEventListener('resize', handleViewportResize)
})

watch(
  () => props.showMoreOptions,
  (showMoreOptions) => {
    if (!showMoreOptions) closeCommentActions()
  },
)

onBeforeUnmount(removeOpenMenuListeners)

function handleDeleteComment(commentId: string) {
  closeCommentActions()
  if (!confirm('Are you sure you want to delete this comment?')) return
  emit('deleteComment', commentId)
}

function toggleIsEditing() {
  closeCommentActions()
  isEditing.value = !isEditing.value
}

const newContent = ref(props.content)

watch([isSubmitting, validationErrorMessage], ([newIsSubmitting, newValidationErrorMessage]) => {
  if (isEditing.value && !newIsSubmitting && !newValidationErrorMessage) {
    isEditing.value = false
  }
})
</script>

<template>
  <article class="comment-card">
    <RouterLink
      :aria-label="`View ${authorName}'s profile`"
      class="comment-card__avatar-link"
      :to="`/users/${userId}`"
    >
      <BaseAvatar :initials="avatarInitials" :label="`${authorName}'s avatar`" />
    </RouterLink>

    <div class="comment-card__body">
      <header class="comment-card__header">
        <div class="comment-card__identity">
          <RouterLink class="comment-card__author" :to="`/users/${userId}`">
            {{ authorName }}
          </RouterLink>
          <AppIcon v-if="verified" class="comment-card__verified" name="verified" />
          <span class="comment-card__username">@{{ username }}</span>
          <span aria-hidden="true">·</span>
          <time class="comment-card__timestamp">{{ createdAtUtc }}</time>
        </div>

        <div
          v-if="showMoreOptions"
          ref="commentActions"
          class="comment-card__actions"
          @keydown.esc.stop.prevent="closeCommentActions(true)"
        >
          <BaseIconButton
            :aria-controls="commentActionsPanelId"
            :aria-expanded="isCommentActionsOpen"
            :active="isCommentActionsOpen"
            label="More comment options"
            @click="toggleCommentActions"
          >
            <AppIcon name="more" />
          </BaseIconButton>

          <div
            v-if="isCommentActionsOpen"
            :id="commentActionsPanelId"
            aria-label="Comment actions"
            class="comment-card__actions-panel"
            role="group"
          >
            <button class="comment-card__action-item" type="button" @click="toggleIsEditing">
              Edit comment
            </button>
            <button
              class="comment-card__action-item comment-card__action-item--danger"
              type="button"
              @click="handleDeleteComment(id)"
            >
              Delete comment
            </button>
          </div>
        </div>
      </header>

      <div v-if="isEditing">
        <form @submit.prevent="emit('editComment', id, newContent)">
          <BaseTextField
            v-model="newContent"
            :error-message="validationErrorMessage"
            id="update-comment-content"
            label=""
          />

          <BaseButton
            :is-loading="isSubmitting"
            loading-text="Saving..."
            type="submit"
            variant="primary"
            >Save changes</BaseButton
          >
        </form>
      </div>
      <div v-else>
        <p class="comment-card__content">{{ content }}</p>
      </div>
    </div>
  </article>
</template>

<style scoped>
.comment-card {
  display: grid;
  padding: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.comment-card__avatar-link {
  align-self: start;
  border-radius: var(--radius-full);
  text-decoration: none;
}

.comment-card__body {
  min-width: 0;
}

.comment-card__header {
  display: flex;
  min-height: 2.75rem;
  margin-block-start: calc(var(--space-1) * -1);
  align-items: center;
  justify-content: space-between;
  gap: var(--space-2);
}

.comment-card__identity {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: var(--space-1);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  white-space: nowrap;
}

.comment-card__actions {
  position: relative;
  display: flex;
  flex: 0 0 auto;
}

.comment-card__actions-panel {
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

.comment-card__action-item {
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

.comment-card__action-item--danger {
  color: var(--color-danger);
}

.comment-card__action-item:hover {
  background: var(--color-surface-subtle);
}

.comment-card__action-item:active {
  background: var(--color-border);
}

.comment-card__author {
  display: inline-flex;
  min-width: 0;
  min-height: var(--control-height);
  align-items: center;
  overflow: hidden;
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
  text-overflow: ellipsis;
  text-decoration: none;
}

.comment-card__author:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.comment-card__verified {
  width: 1rem;
  height: 1rem;
  flex: 0 0 auto;
  color: var(--color-text);
}

.comment-card__username,
.comment-card__timestamp {
  overflow: hidden;
  text-overflow: ellipsis;
}

.comment-card__username {
  max-width: 8.5rem;
}

.comment-card__timestamp {
  min-width: 0;
}

.comment-card__content {
  max-width: 65ch;
  margin-block-end: var(--space-2);
  overflow-wrap: anywhere;
  line-height: 1.5;
  text-wrap: pretty;
}

@media (max-width: 28rem) {
  .comment-card__username {
    display: none;
  }
}
</style>
