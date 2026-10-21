<script setup lang="ts">
import { type RouteLocationRaw, RouterLink } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import { computed, onBeforeUnmount, ref, toRefs, useId, useTemplateRef, watch } from 'vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'

const props = withDefaults(
  defineProps<{
    id: string
    userId: string
    authorName: string
    username: string
    avatarInitials: string
    createdAtUtc: string
    updatedAtUtc: string | null
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

const { createdAtUtc, updatedAtUtc, isSubmitting, validationErrorMessage } = toRefs(props)
const { formattedDate: formattedCreatedAtUtc } = useDateFormatter(createdAtUtc)
const { formattedDate: formattedUpdatedAtUtc } = useDateFormatter(updatedAtUtc)

const authorPage = computed<RouteLocationRaw>(() => ({
  name: 'user-profile',
  params: { userId: props.userId },
}))
const authorPageDesc = computed(() => `View ${props.authorName}'s profile`)
const authorAvatarDesc = computed(() => `${props.authorName}'s avatar`)

const emit = defineEmits<{
  deleteComment: [commentId: string]
  editComment: [commentId: string, content: string]
}>()

const isCommentActionsMenuOpen = ref(false)
const commentActionsMenu = useTemplateRef<HTMLElement>('commentActionsMenu')
const commentActionsMenuPanelId = `comment-actions-${useId()}`

function toggleCommentActionsMenu() {
  isCommentActionsMenuOpen.value = !isCommentActionsMenuOpen.value
}

function handleActionsButtonClick() {
  if (isEditing.value) {
    isEditing.value = false
    return
  }

  toggleCommentActionsMenu()
}

function closeCommentActionsMenu(restoreFocus = false) {
  if (!isCommentActionsMenuOpen.value) return

  isCommentActionsMenuOpen.value = false

  if (restoreFocus) {
    commentActionsMenu.value?.querySelector<HTMLButtonElement>('button[aria-expanded]')?.focus()
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isCommentActionsMenuOpen.value || !(event.target instanceof Node)) return
  if (commentActionsMenu.value?.contains(event.target)) return

  closeCommentActionsMenu()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!isCommentActionsMenuOpen.value || !(event.target instanceof Node)) return
  if (commentActionsMenu.value?.contains(event.target)) return

  closeCommentActionsMenu()
}

function handleViewportResize() {
  closeCommentActionsMenu()
}

function removeOpenMenuListeners() {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportResize)
}

watch(isCommentActionsMenuOpen, (isOpen) => {
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
    if (!showMoreOptions) closeCommentActionsMenu()
  },
)

onBeforeUnmount(removeOpenMenuListeners)

function handleDeleteComment(commentId: string) {
  closeCommentActionsMenu()
  if (!confirm('Are you sure you want to delete this comment?')) return
  emit('deleteComment', commentId)
}

function toggleIsEditing() {
  closeCommentActionsMenu()
  isEditing.value = !isEditing.value
}
const isEditing = ref<boolean>(false)
const newContent = ref<string>(props.content)
const apiError = defineModel<boolean>('apiError', {
  default: false,
})

watch(
  [isSubmitting, validationErrorMessage, apiError],
  ([newIsSubmitting, newValidationErrorMessage, newApiError]) => {
    if (isEditing.value && !newIsSubmitting && !newValidationErrorMessage && !newApiError) {
      isEditing.value = false
    }
  },
)
</script>

<template>
  <article
    :class="[
      'comment-card',
      {
        'comment-card--editing': isEditing,
        'comment-card--has-actions': showMoreOptions,
        'comment-card--menu-open': isCommentActionsMenuOpen,
      },
    ]"
  >
    <RouterLink :to="authorPage" :aria-label="authorPageDesc" class="comment-card__avatar-link">
      <BaseAvatar :initials="avatarInitials" :label="authorAvatarDesc" />
    </RouterLink>

    <div class="comment-card__body">
      <header class="comment-card__header">
        <div class="comment-card__identity">
          <RouterLink :to="authorPage" :aria-label="authorPageDesc" class="comment-card__author">
            {{ authorName }}
          </RouterLink>

          <AppIcon v-if="verified" class="comment-card__verified" name="verified" />

          <span aria-hidden="true">·</span>

          <time v-if="formattedCreatedAtUtc" class="comment-card__timestamp">
            {{ formattedCreatedAtUtc }}
          </time>
        </div>

        <time v-if="formattedUpdatedAtUtc && !isEditing" class="comment-card__updated-at">
          Updated {{ formattedUpdatedAtUtc }}
        </time>

        <div
          v-if="showMoreOptions"
          ref="commentActionsMenu"
          class="comment-card__actions"
          @keydown.esc.stop.prevent="closeCommentActionsMenu(true)"
        >
          <BaseIconButton
            :aria-controls="isEditing ? undefined : commentActionsMenuPanelId"
            :aria-expanded="isEditing ? undefined : isCommentActionsMenuOpen"
            :active="isCommentActionsMenuOpen"
            :label="isEditing ? 'Cancel comment editing' : 'More comment options'"
            @click="handleActionsButtonClick"
          >
            <AppIcon :name="isEditing ? 'close' : 'more'" />
          </BaseIconButton>

          <div
            v-if="isCommentActionsMenuOpen"
            :id="commentActionsMenuPanelId"
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

      <div class="comment-card__content-row">
        <div v-if="isEditing" class="comment-card__editor">
          <form
            class="comment-card__editor-form"
            @submit.prevent="emit('editComment', id, newContent)"
          >
            <BaseTextAreaField
              v-model="newContent"
              @keydown.ctrl.enter.prevent="emit('editComment', id, newContent)"
              :id="`update-comment-content-${id}`"
              :disabled="isSubmitting"
              :error-message="validationErrorMessage"
              class="comment-card__editor-field"
              label="Comment content"
              visually-hidden-label
            />

            <BaseButton
              class="comment-card__editor-submit"
              :disabled="isSubmitting || !newContent.trim() || newContent === content"
              :is-loading="isSubmitting"
              loading-text="Saving..."
              size="compact"
              type="submit"
              variant="secondary"
            >
              Save changes
            </BaseButton>
          </form>
        </div>
        <p v-else class="comment-card__content">{{ content }}</p>
      </div>
    </div>
  </article>
</template>

<style scoped>
.comment-card {
  --comment-content-offset: 0.75rem;
  --comment-spacing: 2rem;

  position: relative;
  display: grid;
  height: 7.5rem;
  padding: var(--space-4);
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
}

.comment-card + .comment-card {
  margin-block-start: var(--comment-spacing);
}

.comment-card--editing {
  height: auto;
  min-height: 7.5rem;
}

.comment-card--menu-open {
  z-index: 1;
}

.comment-card__avatar-link {
  align-self: start;
  border-radius: var(--radius-full);
  text-decoration: none;
}

.comment-card__body {
  display: grid;
  min-width: 0;
  grid-template-rows: auto minmax(0, 1fr);
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
  position: absolute;
  inset-block-start: calc(50% + var(--space-2));
  inset-inline-end: var(--space-4);
  display: flex;
  flex: 0 0 auto;
  transform: translateY(-50%);
}

.comment-card--editing .comment-card__actions {
  position: relative;
  inset: auto;
  margin-inline-start: auto;
  transform: none;
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

.comment-card__timestamp {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
}

.comment-card__content-row {
  display: flex;
  min-height: 0;
  margin-block-start: var(--comment-content-offset);
  align-items: center;
  gap: var(--space-2);
}

.comment-card--has-actions:not(.comment-card--editing) .comment-card__content-row {
  padding-inline-end: calc(var(--control-height) + var(--space-4));
}

.comment-card__editor {
  min-width: 0;
  flex: 0 0 100%;
}

.comment-card__editor-form {
  display: grid;
  min-width: 0;
  grid-template-columns: minmax(0, 1fr) auto;
  align-content: start;
  gap: var(--space-2) var(--space-3);
}

.comment-card__editor-field {
  min-width: 0;
  grid-column: 1;
  grid-row: 1;
}

.comment-card__editor-field :deep(.base-field__input) {
  height: 6.25rem;
  min-height: 6.25rem;
}

.comment-card__editor-submit {
  grid-column: 2;
  grid-row: 1;
  align-self: end;
}

.comment-card__updated-at {
  margin-inline-start: auto;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-align: end;
  white-space: nowrap;
}

.comment-card__content {
  min-width: min(100%, 12rem);
  max-width: 65ch;
  flex: 1 1 12rem;
  overflow-wrap: anywhere;
  line-height: 1.5;
  text-wrap: pretty;
  white-space: pre-wrap;
}

@media (max-width: 47.999rem) {
  .comment-card {
    height: auto;
    min-height: 7.5rem;
  }

  .comment-card__header,
  .comment-card__identity {
    min-width: 0;
  }

  .comment-card__identity {
    overflow: hidden;
  }

  .comment-card__actions {
    position: relative;
    inset: auto;
    margin-inline-start: auto;
    transform: none;
  }

  .comment-card--has-actions:not(.comment-card--editing) .comment-card__content-row {
    padding-inline-end: 0;
  }

  .comment-card__updated-at {
    display: none;
  }

  .comment-card__editor-form {
    grid-template-columns: minmax(0, 1fr) auto;
  }

  .comment-card__editor-field {
    grid-column: 1 / -1;
    grid-row: 1;
  }

  .comment-card__editor-submit {
    margin-block-start: var(--space-2);
    grid-column: 2;
    grid-row: 2;
    align-self: center;
    justify-self: end;
  }
}
</style>
