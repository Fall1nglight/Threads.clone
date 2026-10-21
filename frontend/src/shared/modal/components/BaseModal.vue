<script setup lang="ts">
import { useModalStore } from '@/shared/modal/modalStore.ts'

const modalStore = useModalStore()
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modalStore.isOpen" class="modal-overlay" @click.self="modalStore.close">
        <div class="modal-content">
          <button
            aria-label="Close modal"
            class="close-btn"
            type="button"
            @click="modalStore.close"
          >
            <span aria-hidden="true" class="close-btn__icon">&times;</span>
          </button>

          <component :is="modalStore.view" v-bind="modalStore.props" @close="modalStore.close" />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.modal-content {
  background: transparent;
  padding: var(--space-4);
  min-width: 300px;
  position: relative;
}

.close-btn {
  position: absolute;
  top: var(--space-6);
  right: var(--space-6);
  z-index: 1;
  display: flex;
  width: 2.25rem;
  height: 2.25rem;
  padding: 0;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
  background: var(--color-surface-subtle);
  color: var(--color-text);
  font-size: var(--font-size-title);
  line-height: 1;
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    transform 90ms ease-out;
}

.close-btn__icon {
  display: block;
  transform: translateY(-0.0625rem);
}

.close-btn:hover {
  background: var(--color-border);
}

.close-btn:active {
  transform: scale(0.98);
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
