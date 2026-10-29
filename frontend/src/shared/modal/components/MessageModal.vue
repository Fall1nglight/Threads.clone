<script lang="ts">
export type MessageType = 'error' | 'info' | 'success'
</script>

<script setup lang="ts">
import { computed, type CSSProperties } from 'vue'
import BaseButton from '@/shared/components/BaseButton.vue'

const props = defineProps<{
  title: string
  message: string
  type: MessageType
  actionLabel?: string
  onAction?: () => void
}>()

const styles: Record<MessageType, CSSProperties> = {
  error: {
    color: 'var(--color-danger)',
    backgroundColor: 'var(--color-surface)',
    borderColor: 'var(--color-danger)',
  },
  info: {
    color: 'var(--color-info-text)',
    backgroundColor: 'var(--color-info-background)',
    borderColor: 'var(--color-info-border)',
  },
  success: {
    color: 'var(--color-success)',
    backgroundColor: 'var(--color-surface)',
    borderColor: 'var(--color-success)',
  },
}

const titleStyle = computed<CSSProperties>(() => styles[props.type])
const role = computed(() => (props.type === 'error' ? 'alert' : 'status'))
const ariaLive = computed(() => (props.type === 'error' ? 'assertive' : 'polite'))
</script>

<template>
  <section class="message-modal" :role="role" :aria-live="ariaLive">
    <p class="message-modal__title" :style="titleStyle">
      {{ title }}
    </p>

    <p class="message-modal__text">
      {{ message }}
    </p>

    <BaseButton v-if="actionLabel && onAction" type="button" @clicked="onAction">
      {{ actionLabel }}
    </BaseButton>
  </section>
</template>

<style scoped>
.message-modal {
  display: grid;
  width: 100%;
  max-width: 26rem;
  min-width: 0;
  padding: var(--space-2) var(--space-6) var(--space-6);
  justify-items: center;
  gap: var(--space-6);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
  color: var(--color-text);
  text-align: center;
}

.message-modal__title {
  min-width: 7rem;
  padding: var(--space-2) var(--space-4);
  border-width: 1px;
  border-style: solid;
  border-radius: var(--radius-md);
  font-weight: var(--font-weight-bold);
  line-height: 1.5;
}

.message-modal__text {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.5;
  overflow-wrap: anywhere;
  text-wrap: balance;
}
</style>
