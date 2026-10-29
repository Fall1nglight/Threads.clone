<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { RouterView } from 'vue-router'
import AppStartupView from '@/app/views/AppStartupView.vue'
import { useAppStartupStore } from '@/app/stores/appStartupStore.ts'
import BaseModal from '@/shared/modal/components/BaseModal.vue'

const startupStore = useAppStartupStore()
const { isReady } = storeToRefs(startupStore)
</script>

<template>
  <RouterView v-if="isReady" />
  <AppStartupView v-else />
  <BaseModal />
</template>
