<script setup lang="ts">
defineProps<{
  name:
    | 'home'
    | 'search'
    | 'plus'
    | 'heart'
    | 'heart-filled'
    | 'user'
    | 'menu'
    | 'reply'
    | 'repost'
    | 'send'
    | 'back'
    | 'more'
    | 'verified'
}>()
</script>

<template>
  <svg
    aria-hidden="true"
    class="app-icon"
    fill="none"
    viewBox="0 0 24 24"
    xmlns="http://www.w3.org/2000/svg"
  >
    <path
      v-if="name === 'home'"
      d="M3.5 10.5 12 3l8.5 7.5v9a1.5 1.5 0 0 1-1.5 1.5h-4.5v-6h-5v6H5a1.5 1.5 0 0 1-1.5-1.5v-9Z"
    />
    <path
      v-else-if="name === 'search'"
      d="m20.5 20.5-4.4-4.4m2.4-5.1a7.5 7.5 0 1 1-15 0 7.5 7.5 0 0 1 15 0Z"
    />
    <path v-else-if="name === 'plus'" d="M12 4v16M4 12h16" />
    <path
      v-else-if="name === 'heart'"
      d="M20.8 5.8a5.2 5.2 0 0 0-7.4 0L12 7.2l-1.4-1.4a5.2 5.2 0 0 0-7.4 7.4L12 22l8.8-8.8a5.2 5.2 0 0 0 0-7.4Z"
    />
    <path
      v-else-if="name === 'heart-filled'"
      d="M20.8 5.8a5.2 5.2 0 0 0-7.4 0L12 7.2l-1.4-1.4a5.2 5.2 0 0 0-7.4 7.4L12 22l8.8-8.8a5.2 5.2 0 0 0 0-7.4Z"
      fill="currentColor"
      stroke="none"
    />
    <path
      v-else-if="name === 'user'"
      d="M20 21a8 8 0 0 0-16 0m12-13a4 4 0 1 1-8 0 4 4 0 0 1 8 0Z"
    />
    <path v-else-if="name === 'menu'" d="M4 7h16M4 12h16M4 17h16" />
    <path
      v-else-if="name === 'reply'"
      d="M21 15a4 4 0 0 1-4 4H8l-5 3V7a4 4 0 0 1 4-4h10a4 4 0 0 1 4 4v8Z"
    />
    <path
      v-else-if="name === 'repost'"
      d="m17 2 4 4-4 4M3 11V9a3 3 0 0 1 3-3h15M7 22l-4-4 4-4m14-3v4a3 3 0 0 1-3 3H3"
    />
    <path v-else-if="name === 'send'" d="m22 2-7 20-4-9-9-4 20-7Zm-11 11L22 2" />
    <path v-else-if="name === 'back'" d="m15 18-6-6 6-6" />
    <path v-else-if="name === 'more'" d="M5 12h.01M12 12h.01M19 12h.01" />
    <path
      v-else
      d="m12 2 2.2 2 3-.2.7 2.9 2.6 1.5-1.1 2.8 1.1 2.8-2.6 1.5-.7 2.9-3-.2-2.2 2-2.2-2-3 .2-.7-2.9-2.6-1.5L4.6 11 3.5 8.2l2.6-1.5.7-2.9 3 .2L12 2Zm-2.1 10.1 1.4 1.4 3.2-3.2"
    />
  </svg>
</template>

<style scoped>
.app-icon {
  width: 1.5rem;
  height: 1.5rem;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.8;
}
</style>
