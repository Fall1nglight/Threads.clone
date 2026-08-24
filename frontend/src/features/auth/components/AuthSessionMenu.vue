<script setup lang="ts">
import AppIcon from '@/shared/icons/AppIcon.vue'
import BaseIconButton from '@/shared/ui/BaseIconButton.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { onBeforeUnmount, onMounted, ref, useTemplateRef, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

type PendingAction = 'logout' | 'logout-all'

const props = defineProps<{
  placement: 'desktop' | 'mobile'
}>()

const authStore = useAuthStore()
const { isAuthenticated } = storeToRefs(authStore)
const route = useRoute()
const router = useRouter()

const isOpen = ref(false)
const pendingAction = ref<PendingAction | null>(null)
const errorMessage = ref<string | null>(null)
const menuRoot = useTemplateRef<HTMLElement>('menuRoot')
const panelId = `auth-session-menu-${props.placement}`

function toggleMenu() {
  if (!isAuthenticated.value || pendingAction.value) return

  errorMessage.value = null
  isOpen.value = !isOpen.value
}

function closeMenu(restoreFocus = false) {
  if (!isOpen.value) return

  isOpen.value = false
  errorMessage.value = null

  if (restoreFocus) {
    menuRoot.value?.querySelector<HTMLButtonElement>('button[aria-expanded]')?.focus()
  }
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!isOpen.value || !(event.target instanceof Node)) return
  if (menuRoot.value?.contains(event.target)) return

  closeMenu()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!isOpen.value || !(event.target instanceof Node)) return
  if (menuRoot.value?.contains(event.target)) return

  closeMenu()
}

function handleViewportResize() {
  closeMenu()
}

async function handleLogout(action: PendingAction) {
  if (pendingAction.value) return

  pendingAction.value = action
  errorMessage.value = null

  try {
    if (action === 'logout') {
      await authStore.logout()
    } else {
      await authStore.logoutAll()
    }
  } catch (error) {
    console.error('Logout request failed', error)

    if (!authStore.isAuthenticated) {
      closeMenu()
      await router.replace({ name: 'home' })
      return
    }

    errorMessage.value = "Couldn't log out. Please try again."
    return
  } finally {
    pendingAction.value = null
  }

  closeMenu()
  await router.replace({ name: 'home' })
}

onMounted(() => {
  document.addEventListener('pointerdown', handleDocumentPointerDown)
  document.addEventListener('focusin', handleDocumentFocusIn)
  window.addEventListener('resize', handleViewportResize)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportResize)
})

watch(
  () => route.fullPath,
  () => closeMenu(),
)

watch(isAuthenticated, (authenticated) => {
  if (!authenticated) closeMenu()
})
</script>

<template>
  <div
    ref="menuRoot"
    :class="['auth-session-menu', `auth-session-menu--${placement}`]"
    @keydown.esc.stop.prevent="closeMenu(true)"
  >
    <BaseIconButton
      :aria-controls="panelId"
      :aria-expanded="isOpen"
      :active="isOpen"
      :disabled="!isAuthenticated || pendingAction !== null"
      :label="isAuthenticated ? 'More options' : 'More options — log in to use'"
      @click="toggleMenu"
    >
      <AppIcon name="menu" />
    </BaseIconButton>

    <div
      v-if="isOpen && isAuthenticated"
      :id="panelId"
      :aria-busy="pendingAction !== null"
      aria-label="Session actions"
      :class="['auth-session-menu__panel', `auth-session-menu__panel--${placement}`]"
      role="group"
    >
      <button
        class="auth-session-menu__item"
        :disabled="pendingAction !== null"
        type="button"
        @click="handleLogout('logout')"
      >
        {{ pendingAction === 'logout' ? 'Logging out…' : 'Log out' }}
      </button>

      <button
        class="auth-session-menu__item auth-session-menu__item--danger"
        :disabled="pendingAction !== null"
        type="button"
        @click="handleLogout('logout-all')"
      >
        {{
          pendingAction === 'logout-all'
            ? 'Logging out from all devices…'
            : 'Log out from all devices'
        }}
      </button>

      <p v-if="errorMessage" class="auth-session-menu__error" role="alert">
        {{ errorMessage }}
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-session-menu {
  display: flex;
}

.auth-session-menu--mobile {
  position: relative;
}

.auth-session-menu__panel {
  position: absolute;
  z-index: 10;
  display: grid;
  width: max-content;
  min-width: 15rem;
  max-width: calc(100vw - var(--space-8));
  padding: var(--space-1);
  gap: var(--space-2);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  box-shadow: 0 var(--space-2) var(--space-6) rgb(0 0 0 / 18%);
}

.auth-session-menu__panel--desktop {
  inset-block-end: var(--space-6);
  inset-inline-start: calc(100% + var(--space-2));
}

.auth-session-menu__panel--mobile {
  inset-block-start: calc(100% + var(--space-2));
  inset-inline-end: 0;
}

.auth-session-menu__item {
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

.auth-session-menu__item--danger {
  color: var(--color-danger);
}

.auth-session-menu__item:hover:not(:disabled) {
  background: var(--color-surface-subtle);
}

.auth-session-menu__item:active:not(:disabled) {
  background: var(--color-border);
}

.auth-session-menu__item:disabled {
  cursor: wait;
  opacity: 0.72;
}

.auth-session-menu__error {
  max-width: 15rem;
  padding: 0 var(--space-3) var(--space-2);
  color: var(--color-danger);
  font-size: var(--font-size-support);
  line-height: 1.4;
}
</style>
