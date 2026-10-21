import { defineStore } from 'pinia'
import { type Component, computed, markRaw, ref, shallowRef } from 'vue'
import RedirectToLogin from '@/shared/modal/components/RedirectToLogin.vue'
import AlertMessage from '@/shared/modal/components/AlertMessage.vue'

export type ModalStatus = 'hidden' | 'active'

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
    view.value = markRaw(RedirectToLogin)
    props.value = {}
    status.value = 'active'
  }

  function openAlertModal(message: string) {
    view.value = markRaw(AlertMessage)
    props.value = { message }
    status.value = 'active'
  }

  function close() {
    status.value = 'hidden'
  }

  return { view, props, status, isOpen, open, openLoginForm, openAlertModal, close }
})
