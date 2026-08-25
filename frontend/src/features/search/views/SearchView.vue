<script setup lang="ts">
import PageHeader from '@/app/components/PageHeader.vue'
import SearchField from '@/features/search/components/SearchField.vue'
import UserResultItem from '@/features/search/components/UserResultItem.vue'
</script>

<template>
  <div>
    <PageHeader title="Search" />
    <section aria-label="Search people" class="search-view">
      <SearchField />
      <div aria-label="Suggested people">
        <UserResultItem
          initials="MC"
          name="Maya Chen"
          summary="Product designer · 12.4K followers"
          user-id="maya"
          username="mayamakes"
          verified
          :follow-status="null"
          :show-follow-button="false"
        />

        <UserResultItem
          initials="NW"
          name="Noah Williams"
          summary="Building thoughtful software in public"
          user-id="noah"
          username="buildwithnoah"
          :follow-status="null"
          :show-follow-button="false"
        />

        <UserResultItem
          initials="AS"
          name="Avery Stone"
          summary="Writing about community and digital culture"
          user-id="avery"
          username="averystone"
          :follow-status="null"
          :show-follow-button="false"
        />

        <UserResultItem
          initials="LP"
          name="Leo Park"
          summary="Frontend systems, accessibility, and type"
          user-id="leo"
          username="leopark"
          :follow-status="null"
          :show-follow-button="false"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.search-view {
  padding: var(--space-4);
}
</style>
