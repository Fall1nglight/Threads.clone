<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, useTemplateRef } from 'vue'
import { NotificationCategory, NotificationStatus } from '@/features/activity/activityTypes.ts'
import AppIcon from '@/app/components/AppIcon.vue'

type FilterName = 'category' | 'status'
type FilterOption<TValue> = { value: TValue; label: string }

const category = defineModel<NotificationCategory | null>('category', { required: true })
const status = defineModel<NotificationStatus | null>('status', { required: true })

const categoryOptions: readonly FilterOption<NotificationCategory | null>[] = [
  { value: null, label: 'All' },
  { value: NotificationCategory.Follow, label: 'Follow' },
  { value: NotificationCategory.PostLiked, label: 'Likes' },
  { value: NotificationCategory.PostCommented, label: 'Comments' },
]

const statusOptions: readonly FilterOption<NotificationStatus | null>[] = [
  { value: null, label: 'All' },
  { value: NotificationStatus.Seen, label: 'Seen' },
  { value: NotificationStatus.Unseen, label: 'Unseen' },
]

const activeCategoryLabel = computed(
  () => categoryOptions.find((option) => option.value === category.value)?.label ?? 'All',
)
const activeStatusLabel = computed(
  () => statusOptions.find((option) => option.value === status.value)?.label ?? 'All',
)

const filterRoot = useTemplateRef<HTMLElement>('filterRoot')
const categoryTrigger = useTemplateRef<HTMLButtonElement>('categoryTrigger')
const statusTrigger = useTemplateRef<HTMLButtonElement>('statusTrigger')
const categoryMenu = useTemplateRef<HTMLElement>('categoryMenu')
const statusMenu = useTemplateRef<HTMLElement>('statusMenu')
const openFilter = ref<FilterName | null>(null)
const componentId = useId()
const categoryMenuId = `${componentId}-category-menu`
const statusMenuId = `${componentId}-status-menu`
const menuPosition = ref({ top: '0px', left: '0px', minWidth: '0px' })

function getTrigger(filter: FilterName) {
  return filter === 'category' ? categoryTrigger.value : statusTrigger.value
}

function getMenu(filter: FilterName) {
  return filter === 'category' ? categoryMenu.value : statusMenu.value
}

function positionMenu(filter: FilterName) {
  const trigger = getTrigger(filter)
  const menu = getMenu(filter)
  if (!trigger || !menu) return

  const viewportMargin = 8
  const menuGap = 8
  const triggerRect = trigger.getBoundingClientRect()
  const menuRect = menu.getBoundingClientRect()
  const left = Math.min(
    Math.max(viewportMargin, triggerRect.left),
    window.innerWidth - menuRect.width - viewportMargin,
  )
  const preferredTop = triggerRect.bottom + menuGap
  const top =
    preferredTop + menuRect.height <= window.innerHeight - viewportMargin
      ? preferredTop
      : Math.max(viewportMargin, triggerRect.top - menuRect.height - menuGap)

  menuPosition.value = {
    top: `${top}px`,
    left: `${Math.max(viewportMargin, left)}px`,
    minWidth: `${triggerRect.width}px`,
  }
}

function focusSelectedOption(filter: FilterName) {
  getMenu(filter)
    ?.querySelector<HTMLButtonElement>('[role="menuitemradio"][aria-checked="true"]')
    ?.focus()
}

async function toggleMenu(filter: FilterName) {
  if (openFilter.value === filter) {
    closeMenu(true)
    return
  }

  openFilter.value = filter
  await nextTick()
  positionMenu(filter)
  focusSelectedOption(filter)
}

function closeMenu(restoreFocus = false) {
  const closingFilter = openFilter.value
  if (!closingFilter) return

  openFilter.value = null
  if (restoreFocus) nextTick(() => getTrigger(closingFilter)?.focus())
}

function selectCategory(value: NotificationCategory | null) {
  category.value = value
  closeMenu(true)
}

function selectStatus(value: NotificationStatus | null) {
  status.value = value
  closeMenu(true)
}

function handleMenuKeydown(event: KeyboardEvent, filter: FilterName) {
  if (event.key === 'Escape') {
    event.preventDefault()
    event.stopPropagation()
    closeMenu(true)
    return
  }

  if (event.key === 'Tab') {
    requestAnimationFrame(() => closeMenu())
    return
  }

  if (!['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) return

  const options = Array.from(
    getMenu(filter)?.querySelectorAll<HTMLButtonElement>('[role="menuitemradio"]') ?? [],
  )
  if (!options.length) return

  event.preventDefault()
  const currentIndex = options.indexOf(document.activeElement as HTMLButtonElement)
  let nextIndex = currentIndex

  if (event.key === 'Home') nextIndex = 0
  else if (event.key === 'End') nextIndex = options.length - 1
  else if (event.key === 'ArrowDown') nextIndex = (currentIndex + 1) % options.length
  else nextIndex = (currentIndex - 1 + options.length) % options.length

  options[nextIndex]?.focus()
}

function handleDocumentPointerDown(event: PointerEvent) {
  if (!openFilter.value || !(event.target instanceof Node)) return
  if (filterRoot.value?.contains(event.target)) return

  closeMenu()
}

function handleDocumentFocusIn(event: FocusEvent) {
  if (!openFilter.value || !(event.target instanceof Node)) return
  if (filterRoot.value?.contains(event.target)) return

  closeMenu()
}

function handleViewportChange() {
  closeMenu()
}

onMounted(() => {
  document.addEventListener('pointerdown', handleDocumentPointerDown)
  document.addEventListener('focusin', handleDocumentFocusIn)
  window.addEventListener('resize', handleViewportChange)
  window.addEventListener('scroll', handleViewportChange, true)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', handleDocumentPointerDown)
  document.removeEventListener('focusin', handleDocumentFocusIn)
  window.removeEventListener('resize', handleViewportChange)
  window.removeEventListener('scroll', handleViewportChange, true)
})
</script>

<template>
  <div ref="filterRoot" aria-label="Activity filters" class="activity-filters" role="group">
    <div class="activity-filters__control">
      <button
        ref="categoryTrigger"
        :aria-controls="categoryMenuId"
        :aria-expanded="openFilter === 'category'"
        :class="[
          'activity-filters__trigger',
          { 'activity-filters__trigger--active': category !== null || openFilter === 'category' },
        ]"
        aria-haspopup="menu"
        type="button"
        @click="toggleMenu('category')"
      >
        <span>Category: {{ activeCategoryLabel }}</span>
        <AppIcon name="chevron-down" />
      </button>

      <div
        v-if="openFilter === 'category'"
        :id="categoryMenuId"
        ref="categoryMenu"
        :style="menuPosition"
        aria-label="Category filters"
        class="activity-filters__menu"
        role="menu"
        @keydown="handleMenuKeydown($event, 'category')"
      >
        <button
          v-for="option in categoryOptions"
          :key="option.label"
          :aria-checked="category === option.value"
          class="activity-filters__option"
          role="menuitemradio"
          tabindex="-1"
          type="button"
          @click="selectCategory(option.value)"
        >
          <span>{{ option.label }}</span>
          <AppIcon v-if="category === option.value" name="check" />
        </button>
      </div>
    </div>

    <div class="activity-filters__control">
      <button
        ref="statusTrigger"
        :aria-controls="statusMenuId"
        :aria-expanded="openFilter === 'status'"
        :class="[
          'activity-filters__trigger',
          { 'activity-filters__trigger--active': status !== null || openFilter === 'status' },
        ]"
        aria-haspopup="menu"
        type="button"
        @click="toggleMenu('status')"
      >
        <span>Status: {{ activeStatusLabel }}</span>
        <AppIcon name="chevron-down" />
      </button>

      <div
        v-if="openFilter === 'status'"
        :id="statusMenuId"
        ref="statusMenu"
        :style="menuPosition"
        aria-label="Status filters"
        class="activity-filters__menu"
        role="menu"
        @keydown="handleMenuKeydown($event, 'status')"
      >
        <button
          v-for="option in statusOptions"
          :key="option.label"
          :aria-checked="status === option.value"
          class="activity-filters__option"
          role="menuitemradio"
          tabindex="-1"
          type="button"
          @click="selectStatus(option.value)"
        >
          <span>{{ option.label }}</span>
          <AppIcon v-if="status === option.value" name="check" />
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.activity-filters {
  display: flex;
  padding: var(--space-3) var(--space-4);
  gap: var(--space-2);
  overflow-x: auto;
  border-block-end: 1px solid var(--color-border);
  scrollbar-width: none;
}

.activity-filters::-webkit-scrollbar {
  display: none;
}

.activity-filters__control {
  display: flex;
  flex: 0 0 auto;
}

.activity-filters__trigger {
  display: inline-flex;
  min-height: var(--control-height);
  padding-inline: var(--space-4) var(--space-3);
  gap: var(--space-2);
  align-items: center;
  justify-content: center;
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-full);
  background: var(--color-surface-subtle);
  color: var(--color-text);
  font: inherit;
  font-weight: var(--font-weight-medium);
  white-space: nowrap;
  cursor: pointer;
  transition:
    background-color var(--transition-fast),
    border-color var(--transition-fast),
    color var(--transition-fast);
}

.activity-filters__trigger :deep(.app-icon) {
  width: 1rem;
  height: 1rem;
}

.activity-filters__trigger:hover {
  background: var(--color-control-border);
  color: var(--color-action-contrast);
}

.activity-filters__trigger--active {
  border-color: var(--color-action);
  background: var(--color-action);
  color: var(--color-action-contrast);
  font-weight: var(--font-weight-semibold);
}

.activity-filters__trigger--active:hover {
  background: var(--color-action-hover);
}

.activity-filters__menu {
  position: fixed;
  z-index: 30;
  display: grid;
  width: max-content;
  max-width: calc(100vw - var(--space-4));
  padding: var(--space-1);
  gap: var(--space-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  box-shadow: 0 var(--space-2) var(--space-6) rgb(0 0 0 / 18%);
}

.activity-filters__option {
  display: flex;
  width: 100%;
  min-height: var(--control-height);
  padding-inline: var(--space-3);
  gap: var(--space-6);
  align-items: center;
  justify-content: space-between;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text);
  font: inherit;
  font-weight: var(--font-weight-medium);
  text-align: start;
  white-space: nowrap;
  cursor: pointer;
  transition: background-color var(--transition-fast);
}

.activity-filters__option:hover,
.activity-filters__option[aria-checked='true'] {
  background: var(--color-surface-subtle);
}

.activity-filters__option[aria-checked='true'] {
  font-weight: var(--font-weight-semibold);
}

.activity-filters__option :deep(.app-icon) {
  width: 1rem;
  height: 1rem;
}
</style>
