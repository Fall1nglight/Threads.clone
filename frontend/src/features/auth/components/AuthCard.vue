<script setup lang="ts">
import NavigationLink from '@/app/components/NavigationLink.vue'

defineProps<{
  title: string
  subtitle: string
}>()
</script>

<template>
  <section class="auth-card">
    <div class="auth-card__intro">
      <NavigationLink
        class="auth-card__home-link"
        icon="home"
        label="Home"
        :to="{ name: 'home' }"
      />

      <header class="auth-card__header">
        <h1>{{ title }}</h1>
        <p>{{ subtitle }}</p>
      </header>
    </div>

    <slot />
  </section>
</template>

<style scoped>
.auth-card {
  position: relative;
  display: grid;
  width: 100%;
  max-width: 26rem;
  min-width: 0;
  padding: var(--space-8) var(--space-6);
  gap: var(--space-6);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-surface);
  box-shadow: var(--shadow-panel);
}
.auth-card__intro {
  display: grid;
}
.auth-card__intro > .auth-card__home-link {
  width: 2.75rem;
  min-height: 2.75rem;
  margin: calc(-1 * var(--space-3)) 0 var(--space-3);
  justify-self: start;
}
.auth-card__home-link :deep(.navigation-link__label) {
  display: none;
}
.auth-card__header {
  display: grid;
  gap: var(--space-2);
  text-align: center;
}
h1 {
  font-size: var(--font-size-title);
  line-height: 1.3;
  letter-spacing: -0.025em;
  text-wrap: balance;
}
p {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-wrap: pretty;
}
.auth-card :deep(.base-field__input) {
  min-height: 3.5rem;
}
@media (max-width: 30rem) {
  .auth-card {
    padding-inline: var(--space-4);
  }
}
</style>
