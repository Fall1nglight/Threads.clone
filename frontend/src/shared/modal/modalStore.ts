import { defineStore } from 'pinia'
import { type Component, computed, markRaw, ref, shallowRef } from 'vue'
import LoginView from '@/features/auth/views/LoginView.vue'

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
    view.value = markRaw(LoginView)
    props.value = {}
    status.value = 'active'
  }

  function close() {
    status.value = 'hidden'
  }

  return { view, props, status, isOpen, open, openLoginForm, close }
})
