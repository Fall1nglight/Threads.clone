<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { formatDistanceToNow } from 'date-fns'
import type { PostDto } from '@/features/posts/postTypes.ts'
import BaseAvatar from '@/shared/ui/BaseAvatar.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import PostActionBar from '@/features/posts/components/PostActionBar.vue'

const { post } = defineProps<{
  post: PostDto
  isSubmitting: boolean
  validationErrorMessage: string | null
}>()

const content = ref(post.content)

const emit = defineEmits<{
  handleSave: [content: string]
}>()
</script>

<template>
  <article class="post-edit-card">
    <div class="post-edit-card__avatar-column">
      <RouterLink
        :aria-label="`View ${post.user.username}'s profile`"
        :to="`/users/${post.user.id}`"
      >
        <BaseAvatar
          :initials="post.user.username.slice(0, 2)"
          :label="`${post.user.username}'s avatar`"
        />
      </RouterLink>
    </div>

    <form class="post-edit-card__form" @submit.prevent="emit('handleSave', content)">
      <header class="post-edit-card__header">
        <div class="post-edit-card__identity">
          <RouterLink class="post-edit-card__author" :to="`/users/${post.user.id}`">
            {{ post.user.username }}
          </RouterLink>
          <span class="post-edit-card__username">@{{ post.user.username }}</span>
          <span aria-hidden="true">·</span>
          <RouterLink class="post-edit-card__timestamp" :to="`/posts/${post.id}`">
            {{ formatDistanceToNow(post.createdAtUtc, { addSuffix: true }) }}
          </RouterLink>
          <template v-if="post.updatedAtUtc">
            <span aria-hidden="true">·</span>
            <span>{{ formatDistanceToNow(post.updatedAtUtc, { addSuffix: true }) }}</span>
          </template>
        </div>
      </header>

      <BaseTextField
        v-model="content"
        :id="`post-edit-content-${post.id}`"
        :error-message="validationErrorMessage"
        label="Post content"
        type="text"
        variant="bare"
        visually-hidden-label
      />

      <PostActionBar
        disabled
        :comment-count="post.commentCount"
        :is-liked-by-current-user="post.isLikedByCurrentUser"
        :like-count="post.likeCount"
      />

      <div class="post-edit-card__footer">
        <BaseButton :is-loading="isSubmitting" loading-text="Saving..." type="submit"
          >Save</BaseButton
        >
      </div>
    </form>
  </article>
</template>

<style scoped>
.post-edit-card {
  display: grid;
  padding: var(--space-6) var(--space-4) var(--space-4);
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
  gap: var(--space-2);
}

.post-edit-card__header {
  display: flex;
  min-height: 2.75rem;
  margin-block-start: calc(var(--space-1) * -1);
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

.post-edit-card__footer {
  display: flex;
  padding-block-start: var(--space-2);
  justify-content: flex-end;
}

@media (max-width: 28rem) {
  .post-edit-card__username {
    display: none;
  }
}
</style>
