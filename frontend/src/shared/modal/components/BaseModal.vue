<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, useTemplateRef, watch } from 'vue'
import AppIcon from '@/app/components/AppIcon.vue'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import MessageModal from '@/shared/modal/components/MessageModal.vue'

const modalStore = useModalStore()
const dialog = useTemplateRef<HTMLDialogElement>('dialog')
const isMessageModal = computed(() => modalStore.view === MessageModal)
let returnFocus: HTMLElement | null = null
let previousOverflow = ''

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',')

function handleKeydown(event: KeyboardEvent) {
  if (event.key !== 'Tab') return

  const focusableElements = Array.from(
    dialog.value?.querySelectorAll<HTMLElement>(focusableSelector) ?? [],
  ).filter((element) => element.getClientRects().length > 0)

  if (focusableElements.length === 0) {
    event.preventDefault()
    dialog.value?.focus()
    return
  }

  const firstElement = focusableElements[0]!
  const lastElement = focusableElements.at(-1)!
  const activeElement = document.activeElement

  if (event.shiftKey && activeElement === firstElement) {
    event.preventDefault()
    lastElement.focus()
  } else if (!event.shiftKey && activeElement === lastElement) {
    event.preventDefault()
    firstElement.focus()
  }
}

watch(
  () => modalStore.isOpen,
  async (isOpen) => {
    if (isOpen) {
      returnFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null
      previousOverflow = document.body.style.overflow
      document.body.style.overflow = 'hidden'
      await nextTick()
      if (modalStore.isOpen) dialog.value?.showModal()
    } else {
      dialog.value?.close()
      document.body.style.overflow = previousOverflow
      returnFocus?.focus({ preventScroll: true })
    }
  },
)

onBeforeUnmount(() => {
  if (modalStore.isOpen) document.body.style.overflow = previousOverflow
})
</script>

<template>
  <Teleport to="body">
    <dialog
      ref="dialog"
      class="modal"
      aria-label="Notification"
      @cancel.prevent="modalStore.close"
      @click.self="modalStore.close"
      @keydown="handleKeydown"
    >
      <div
        v-if="modalStore.isOpen"
        class="modal__content"
        :class="{ 'modal__content--message': isMessageModal }"
      >
        <button
          aria-label="Close modal"
          class="modal__close"
          type="button"
          @click="modalStore.close"
        >
          <AppIcon name="close" />
        </button>
        <component :is="modalStore.view" v-bind="modalStore.props" @close="modalStore.close" />
      </div>
    </dialog>
  </Teleport>
</template>

<style scoped>
.modal {
  width: min(28rem, calc(100% - 2rem));
  max-height: calc(100dvh - 2rem);
  padding: 0;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
  color: var(--color-text);
  box-shadow: var(--shadow-menu);
}
.modal::backdrop {
  background: rgb(0 0 0 / 55%);
}
.modal[open] {
  animation: modal-arrive 160ms ease-out;
}
.modal__content {
  position: relative;
  padding-top: 3.5rem;
}
.modal__content--message {
  padding-top: 0;
}
.modal__content :deep(section) {
  max-width: none;
  border: 0;
  box-shadow: none;
}
.modal__close {
  position: absolute;
  top: var(--space-2);
  right: var(--space-2);
  display: grid;
  width: 2.75rem;
  height: 2.75rem;
  place-items: center;
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--color-text);
  cursor: pointer;
}
.modal__close:hover {
  background: var(--color-surface-subtle);
}
@keyframes modal-arrive {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
