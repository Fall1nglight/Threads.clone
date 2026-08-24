<script setup lang="ts">
import { useModalStore } from '@/shared/modal/modalStore'

const modalStore = useModalStore()
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modalStore.isOpen" class="modal-overlay" @click.self="modalStore.close">
        <div class="modal-content">
          <button class="close-btn" @click="modalStore.close">&times;</button>

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
  padding: 20px;
  border-radius: 8px;
  min-width: 300px;
  position: relative;
}
.close-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  cursor: pointer;
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
