import { defineStore } from 'pinia'
import { type Component, computed, markRaw, ref, shallowRef } from 'vue'
import RedirectToLogin from '@/shared/modal/components/RedirectToLogin.vue'
import MessageModal, { type MessageType } from '@/shared/modal/components/MessageModal.vue'

export type ModalStatus = 'hidden' | 'active'

const MESSAGE_TITLES: Record<MessageType, string> = {
  error: 'Error',
  info: 'Info',
  success: 'Success',
}

export const useModalStore = defineStore('modalStore', () => {
  const view = shallowRef<Component | null>(null)
  const props = ref({})
  const status = ref<ModalStatus>('hidden')
  const isOpen = computed(() => status.value === 'active')

  function open(component: Component, componentProps = {}) {
    view.value = markRaw(component)
    props.value = componentProps
    status.value = 'active'
  }

  function openLoginForm() {
    open(RedirectToLogin)
  }

  function openMessageModal(message: string, type: MessageType, title = MESSAGE_TITLES[type]) {
    open(MessageModal, { title, message, type })
  }

  function openErrorModal(message: string, title?: string) {
    openMessageModal(message, 'error', title)
  }

  function openInfoModal(message: string, title?: string) {
    openMessageModal(message, 'info', title)
  }

  function openSuccessModal(message: string, title?: string) {
    openMessageModal(message, 'success', title)
  }

  function openServiceUnavailableModal(onRetry: () => void) {
    open(MessageModal, {
      title: 'Error',
      message: 'Service unavailable, please try again later.',
      type: 'error',
      actionLabel: 'Retry',
      onAction: onRetry,
    })
  }

  function close() {
    status.value = 'hidden'
  }

  return {
    view,
    props,
    status,
    isOpen,
    open,
    openLoginForm,
    openErrorModal,
    openInfoModal,
    openSuccessModal,
    openServiceUnavailableModal,
    close,
  }
})
