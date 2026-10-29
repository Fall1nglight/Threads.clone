<script setup lang="ts">
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useAppStartupStore } from '@/app/stores/appStartupStore.ts'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseLoader from '@/shared/components/BaseLoader.vue'

const startupStore = useAppStartupStore()
const { status, isUnavailable } = storeToRefs(startupStore)

const loaderMessage = computed(() =>
  status.value === 'restoring-session'
    ? 'Restoring session, please wait...'
    : 'Service is waking up...',
)
</script>

<template>
  <main class="app-startup" :aria-busy="!isUnavailable">
    <section v-if="isUnavailable" class="app-startup__error">
      <h1>Service unavailable</h1>
      <p>Please try again later.</p>

      <BaseButton type="button" @clicked="startupStore.retry"> Retry </BaseButton>
    </section>

    <BaseLoader v-else :message="loaderMessage" />
  </main>
</template>

<style scoped>
.app-startup {
  display: grid;
  min-height: 100dvh;
  padding: var(--page-gutter);
  place-items: center;
  background: var(--color-page);
}

.app-startup__error {
  display: grid;
  width: min(100%, 28rem);
  padding: var(--space-8);
  justify-items: center;
  gap: var(--space-4);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
  color: var(--color-text);
  text-align: center;
  box-shadow: var(--shadow-panel);
}

.app-startup__error h1 {
  color: var(--color-danger);
  font-size: var(--font-size-page-title);
}

.app-startup__error p {
  color: var(--color-text-muted);
}
</style>
