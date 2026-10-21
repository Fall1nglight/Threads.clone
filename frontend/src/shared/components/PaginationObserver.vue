<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

const triggerElement = ref<HTMLElement | null>(null)
let observer: IntersectionObserver | null = null

const emit = defineEmits<{
  visible: []
}>()

onMounted(() => {
  observer = new IntersectionObserver(
    ([entry]) => {
      if (entry?.isIntersecting) {
        emit('visible')
      }
    },
    {
      root: null,
      rootMargin: '300px 0px',
      threshold: 0,
    },
  )

  if (triggerElement.value) {
    observer.observe(triggerElement.value)
  }
})

onBeforeUnmount(() => observer?.disconnect())
</script>

<template>
  <div ref="triggerElement" class="feed-load-more-trigger" aria-hidden="true" />
</template>

<style scoped>
.feed-load-more-trigger {
  height: 1px;
}
</style>
