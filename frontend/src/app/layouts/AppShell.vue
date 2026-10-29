<script setup lang="ts">
import { RouterView } from 'vue-router'
import DesktopNavigation from '@/app/components/DesktopNavigation.vue'
import MobileNavigation from '@/app/components/MobileNavigation.vue'
import MobileTopBar from '@/app/components/MobileTopBar.vue'
import { provideNotificationCount } from '@/features/activity/composables/useNotificationCount.ts'

provideNotificationCount()
</script>

<template>
  <a class="skip-link" href="#main-content">Skip to main content</a>
  <div class="app-shell">
    <DesktopNavigation />
    <MobileTopBar />
    <main id="main-content" class="app-shell__content" tabindex="-1">
      <RouterView />
    </main>
    <MobileNavigation />
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100dvh;
  padding-block: var(--mobile-top-height)
    calc(var(--mobile-bottom-height) + env(safe-area-inset-bottom));
}
.app-shell__content {
  width: 100%;
  min-height: calc(100dvh - var(--mobile-top-height) - var(--mobile-bottom-height));
  margin-inline: auto;
  background: var(--color-surface);
}
@media (min-width: 48rem) {
  .app-shell {
    padding: var(--space-4) calc(var(--desktop-nav-width) + var(--space-4)) var(--space-8);
  }
  .app-shell__content {
    max-width: var(--app-content-width);
    min-height: calc(100dvh - var(--space-8));
    border: 1px solid var(--color-border);
    border-radius: var(--radius-xl);
    box-shadow: var(--shadow-panel);
  }
}
</style>
