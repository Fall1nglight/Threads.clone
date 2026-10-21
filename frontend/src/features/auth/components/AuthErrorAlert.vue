<script setup lang="ts">
withDefaults(
  defineProps<{
    message: string
  }>(),
  {
    message: '',
  },
)

const emit = defineEmits<{
  closeAlert: []
}>()
</script>

<template>
  <div v-if="message.length" class="auth-error-alert" role="alert">
    <p class="auth-error-alert__message">{{ message }}</p>

    <button
      aria-label="Dismiss error"
      class="auth-error-alert__close"
      type="button"
      @click="emit('closeAlert')"
    >
      <span aria-hidden="true">&times;</span>
    </button>
  </div>
</template>

<style scoped>
.auth-error-alert {
  display: grid;
  min-width: 0;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: start;
  border: 1px solid color-mix(in srgb, var(--color-danger) 35%, var(--color-surface));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-danger) 12%, var(--color-surface));
  color: var(--color-danger);
}

.auth-error-alert__message {
  min-width: 0;
  padding: var(--space-3) 0 var(--space-3) var(--space-4);
  font-size: var(--font-size-support);
  line-height: 1.5;
  overflow-wrap: anywhere;
}

.auth-error-alert__close {
  display: grid;
  width: var(--control-height);
  height: var(--control-height);
  place-items: center;
  border-radius: var(--radius-md);
  background: transparent;
  color: currentColor;
  cursor: pointer;
  font-size: var(--font-size-title);
  line-height: 1;
  opacity: 0.72;
  transition:
    background-color var(--transition-fast),
    opacity var(--transition-fast);
}

.auth-error-alert__close:hover {
  background: color-mix(in srgb, var(--color-danger) 10%, transparent);
  opacity: 1;
}
</style>
