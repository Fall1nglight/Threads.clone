<script setup lang="ts">
import PageHeader from '@/app/components/PageHeader.vue'
import ActivityFilterBar from '@/features/activity/components/ActivityFilterBar.vue'
import ActivityItem from '@/features/activity/components/ActivityItem.vue'
</script>

<template>
  <div>
    <PageHeader title="Activity" />
    <ActivityFilterBar />
    <section aria-label="Recent activity">
      <ActivityItem
        action="started following you"
        actor="Leo Park"
        initials="LP"
        show-follow-button
        timestamp="12m"
        user-id="leo"
        username="leopark"
      />
      <ActivityItem
        action="replied to your thread"
        actor="Maya Chen"
        details="The spacing rhythm is doing the heavy lifting here."
        initials="MC"
        timestamp="1h"
        user-id="maya"
        username="mayamakes"
      />
      <ActivityItem
        action="mentioned you in a thread"
        actor="Avery Stone"
        details="@demo-user this is the feature-first structure we discussed."
        initials="AS"
        timestamp="3h"
        user-id="avery"
        username="averystone"
      />
      <ActivityItem
        action="liked your thread"
        actor="Noah Williams"
        initials="NW"
        timestamp="5h"
        user-id="noah"
        username="buildwithnoah"
      />
    </section>
  </div>
</template>
