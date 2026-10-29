<script setup lang="ts">
import { nextTick } from 'vue'
import { useRouter } from 'vue-router'
import AppIcon from '@/app/components/AppIcon.vue'
import BaseIconButton from '@/shared/components/BaseIconButton.vue'
import { useLoginPrompt } from '@/features/auth/composables/useLoginPrompt'

const router = useRouter()
const { runOrShowLoginForm } = useLoginPrompt()

async function focusComposer() {
  await router.push({ name: 'home' })
  await nextTick()
  const composer = document.getElementById('postComposer-input')
  composer?.focus({ preventScroll: true })
  window.scrollTo({ top: 0, behavior: 'instant' })
}
</script>

<template>
  <BaseIconButton
    label="Create a thread"
    title="Create a thread"
    @click="runOrShowLoginForm(focusComposer)"
  >
    <AppIcon name="plus" />
  </BaseIconButton>
</template>
