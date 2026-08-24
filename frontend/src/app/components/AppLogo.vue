<script setup lang="ts">
withDefaults(
  defineProps<{
    size?: 'small' | 'large'
  }>(),
  {
    size: 'small',
  },
)
</script>

<template>
  <svg
    aria-label="Threads clone"
    :class="['app-logo', `app-logo--${size}`]"
    fill="none"
    role="img"
    viewBox="0 0 48 48"
    xmlns="http://www.w3.org/2000/svg"
  >
    <path
      d="M34.4 20.5c-1.2-7-5.2-10.4-10.9-10.4-7.1 0-12 5.5-12 13.7 0 8.7 4.8 14.1 12.7 14.1 6.4 0 11.7-3.4 11.7-9.1 0-4.7-3.5-7.5-9.2-7.5-5.2 0-8.5 2.5-8.5 6.2 0 3.1 2.5 5.1 5.8 5.1 4.8 0 8.6-3.7 8.6-9.1 0-4.4-2.1-7.6-5.8-9.1"
    />
  </svg>
</template>

<style scoped>
.app-logo {
  color: var(--color-text);
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}

.app-logo--small {
  width: 2rem;
  height: 2rem;
}

.app-logo--large {
  width: 3.5rem;
  height: 3.5rem;
}
</style>
