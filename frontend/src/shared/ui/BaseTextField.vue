<script setup lang="ts">
const content = defineModel<string>({ default: '' })

withDefaults(
  defineProps<{
    id: string
    label: string
    type?: 'text' | 'email' | 'password' | 'search'
    variant?: 'default' | 'subtle' | 'bare'
    visuallyHiddenLabel?: boolean
    placeholder?: string
    autocomplete?: string
    errorMessage?: string | null
  }>(),
  {
    type: 'text',
    variant: 'default',
    visuallyHiddenLabel: false,
    placeholder: '',
    autocomplete: 'off',
    errorMessage: null,
  },
)
</script>

<template>
  <label :class="['base-field', `base-field--${variant}`]" :for="id">
    <span
      :class="['base-field__label', { 'base-field__label--visually-hidden': visuallyHiddenLabel }]"
    >
      {{ label }}
    </span>
    <span class="base-field__control">
      <input
        v-model="content"
        :id="id"
        :class="['base-field__input', { 'base-field__input--invalid': errorMessage }]"
        :type="type"
        :placeholder="placeholder"
        :autocomplete="autocomplete"
        :aria-invalid="errorMessage ? 'true' : undefined"
        :aria-describedby="errorMessage ? `${id}-error` : undefined"
      />

      <span v-if="errorMessage" :id="`${id}-error`" class="base-field__error" role="alert">
        {{ errorMessage }}
      </span>
    </span>
  </label>
</template>

<style scoped>
.base-field {
  display: grid;
  min-width: 0;
  gap: var(--space-2);
}

.base-field__label {
  font-size: var(--font-size-support);
  font-weight: var(--font-weight-semibold);
}

.base-field__label--visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.base-field__control {
  display: grid;
  min-width: 0;
  gap: var(--space-1);
}

.base-field__input {
  width: 100%;
  min-height: var(--control-height);
  padding-inline: var(--space-4);
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  color: var(--color-text);
  font-size: var(--font-size-body);
}

.base-field__input::placeholder {
  color: var(--color-text-muted);
  opacity: 1;
}

.base-field__input:hover {
  border-color: var(--color-text);
}

.base-field--subtle .base-field__input {
  background: var(--color-surface-subtle);
}

.base-field--bare {
  gap: 0;
}

.base-field--bare .base-field__input {
  padding-inline: 0;
  border-color: transparent;
  border-radius: 0;
  background: transparent;
}

.base-field--bare .base-field__input:hover {
  border-color: transparent;
}

.base-field__input--invalid,
.base-field__input--invalid:hover,
.base-field--bare .base-field__input--invalid,
.base-field--bare .base-field__input--invalid:hover {
  border-color: var(--color-danger);
}

.base-field__error {
  color: var(--color-danger);
  font-size: var(--font-size-support);
  line-height: 1.25;
  overflow-wrap: anywhere;
}
</style>
