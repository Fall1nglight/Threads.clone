<script setup lang="ts">
import { computed, ref, toRefs } from 'vue'
import { type RouteLocationRaw, RouterLink } from 'vue-router'
import type { PostDto } from '@/features/posts/postTypes.ts'
import BaseAvatar from '@/shared/components/BaseAvatar.vue'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import PostActionBar from '@/features/posts/components/PostActionBar.vue'
import { useDateFormatter } from '@/shared/composables/useDateFormatter.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const { post, isSubmitting } = defineProps<{
  post: PostDto
  isSubmitting: boolean
  validationErrorMessage: string | null
}>()

// post related getters
const postPage = computed<RouteLocationRaw>(() => ({
  name: 'post-detail',
  params: { postId: post.id },
}))

// author related getters
const authorPage = computed<RouteLocationRaw>(() => ({
  name: 'user-profile',
  params: { userId: post.user.id },
}))
const authorName = computed(() => post.user.username)
const authorPageDesc = computed(() => `View ${authorName.value}'s profile`)
const authorAvatarDesc = computed(() => `${authorName.value}'s avatar`)
const authorInitials = computed(() => getInitials(authorName.value))

const content = ref(post.content)
const isSaveDisabled = computed(() => isSubmitting || content.value === post.content)

const { createdAtUtc, updatedAtUtc } = toRefs(post)
const { formattedDate: formattedCreatedAtUtc } = useDateFormatter(createdAtUtc)
const { formattedDate: formattedUpdatedAtUtc } = useDateFormatter(updatedAtUtc)

const emit = defineEmits<{
  handleSave: [content: string]
}>()
</script>

<template>
  <article class="post-edit-card">
    <div class="post-edit-card__avatar-column">
      <RouterLink :to="authorPage" :aria-label="authorPageDesc">
        <BaseAvatar :initials="authorInitials" :label="authorAvatarDesc" />
      </RouterLink>
    </div>

    <form class="post-edit-card__form" @submit.prevent="emit('handleSave', content)">
      <header class="post-edit-card__header">
        <div class="post-edit-card__identity">
          <RouterLink :to="postPage" class="post-edit-card__author">
            {{ authorName }}
          </RouterLink>

          <span aria-hidden="true">·</span>

          <RouterLink v-if="formattedCreatedAtUtc" :to="postPage" class="post-edit-card__timestamp">
            {{ formattedCreatedAtUtc }}
          </RouterLink>
        </div>
      </header>

      <BaseTextAreaField
        v-model="content"
        @keydown.ctrl.enter.prevent="emit('handleSave', content)"
        :id="`post-edit-content-${post.id}`"
        :disabled="isSubmitting"
        :error-message="validationErrorMessage"
        class="post-edit-card__field"
        label="Post content"
        visually-hidden-label
      />

      <BaseButton
        class="post-edit-card__submit"
        :disabled="isSaveDisabled"
        :is-loading="isSubmitting"
        loading-text="Saving..."
        type="submit"
      >
        Save
      </BaseButton>

      <div class="post-edit-card__footer">
        <PostActionBar
          class="post-edit-card__actions"
          disabled
          :comment-count="post.commentCount"
          :is-liked-by-current-user="post.isLikedByCurrentUser"
          :like-count="post.likeCount"
        />

        <time v-if="formattedUpdatedAtUtc" class="post-edit-card__updated-at">
          Updated {{ formattedUpdatedAtUtc }}
        </time>
      </div>
    </form>
  </article>
</template>

<style scoped>
.post-edit-card {
  display: grid;
  padding: var(--space-6) var(--space-4) var(--space-2);
  grid-template-columns: auto minmax(0, 1fr);
  gap: var(--space-3);
  border-block-end: 1px solid var(--color-border);
}

.post-edit-card__avatar-column {
  display: flex;
  min-width: 2.75rem;
  align-items: center;
  flex-direction: column;
  gap: var(--space-2);
}

.post-edit-card__avatar-column a {
  border-radius: var(--radius-full);
  text-decoration: none;
}

.post-edit-card__form {
  display: grid;
  min-width: 0;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: var(--space-2) var(--space-3);
}

.post-edit-card__header {
  display: flex;
  min-height: 2.75rem;
  margin-block-start: calc(var(--space-1) * -1);
  grid-column: 1 / -1;
  align-items: center;
}

.post-edit-card__identity {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: var(--space-1);
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  white-space: nowrap;
}

.post-edit-card__author {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  overflow: hidden;
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
  text-overflow: ellipsis;
  text-decoration: none;
}

.post-edit-card__author:hover,
.post-edit-card__timestamp:hover {
  text-decoration: underline;
  text-underline-offset: 0.18em;
}

.post-edit-card__username {
  max-width: 8.5rem;
  overflow: hidden;
  text-overflow: ellipsis;
}

.post-edit-card__timestamp {
  display: inline-flex;
  min-width: var(--control-height);
  min-height: var(--control-height);
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  text-decoration: none;
}

.post-edit-card__field {
  min-width: 0;
  grid-column: 1;
}

.post-edit-card__submit {
  grid-column: 2;
  align-self: end;
}

.post-edit-card__footer {
  display: flex;
  grid-column: 1;
  align-items: center;
  gap: var(--space-2);
}

.post-edit-card__updated-at {
  margin-inline-start: auto;
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-align: end;
  white-space: nowrap;
}

@media (max-width: 28rem) {
  .post-edit-card__username {
    display: none;
  }
}

@media (max-width: 47.999rem) {
  .post-edit-card__form {
    grid-template-columns: minmax(0, 1fr) auto;
  }

  .post-edit-card__header,
  .post-edit-card__field {
    grid-column: 1 / -1;
  }

  .post-edit-card__submit {
    margin-block-start: var(--space-2);
    grid-column: 2;
    grid-row: 3;
    align-self: center;
    justify-self: end;
  }

  .post-edit-card__footer {
    display: contents;
  }

  .post-edit-card__actions {
    margin-block-start: var(--space-2);
    grid-column: 1;
    grid-row: 3;
  }

  .post-edit-card__updated-at {
    display: none;
  }
}
</style>
