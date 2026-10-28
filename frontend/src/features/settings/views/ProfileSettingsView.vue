<script setup lang="ts">
import { computed, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import userApi from '@/features/users/userApi.ts'
import type { UpdateUserDto } from '@/features/users/userTypes.ts'
import updateUserSchema from '@/features/users/schemas/updateUserSchema.ts'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseErrorMessage from '@/shared/components/BaseErrorMessage.vue'
import BaseLoader from '@/shared/components/BaseLoader.vue'
import BaseTextAreaField from '@/shared/components/BaseTextAreaField.vue'
import { useFetch } from '@/shared/composables/useFetch.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const router = useRouter()
const modalStore = useModalStore()
const authStore = useAuthStore()
const { loggedInUserId } = storeToRefs(authStore)

const { state: userState } = useFetch(loggedInUserId, (currentUserId) =>
  userApi.getUser(currentUserId!),
)

const user = computed(() => (userState.value.status === 'success' ? userState.value.item : null))

const initialSettingsForm: UpdateUserDto = {
  isPrivate: false,
  bio: null,
}

const {
  payload: settingsForm,
  errors: settingsValidationErrors,
  submit: submitSettings,
  isSubmitting: isSubmittingSettings,
} = usePayloadValidator(initialSettingsForm, updateUserSchema, performUpdateUser)

const isSettingsFormUnchanged = computed(() => {
  if (!user.value) return true

  return (
    settingsForm.value.isPrivate === user.value.isPrivate &&
    settingsForm.value.bio === user.value.bio
  )
})

const isSaveDisabled = computed(() => user.value === null || isSettingsFormUnchanged.value)

watch(
  user,
  (loadedUser) => {
    if (!loadedUser) return

    settingsForm.value = {
      isPrivate: loadedUser.isPrivate,
      bio: loadedUser.bio,
    }
  },
  { immediate: true },
)

async function performUpdateUser(validPayload: UpdateUserDto): Promise<void> {
  try {
    await userApi.updateUser(loggedInUserId.value!, validPayload)

    await router.replace({
      name: 'user-profile',
      params: { userId: loggedInUserId.value! },
    })
  } catch {
    modalStore.openAlertModal('Failed to update your profile. Please try again later!')
  }
}
</script>

<template>
  <div>
    <Transition name="async-fade" mode="out-in">
      <BaseLoader
        v-if="userState.status === 'loading'"
        key="profile-settings-loader"
        message="Loading profile settings..."
      />

      <BaseErrorMessage
        v-else-if="userState.status === 'error'"
        key="profile-settings-error"
        message="Failed to load your profile settings. Please try again later!"
      />

      <form
        v-else-if="user"
        key="profile-settings-content"
        aria-label="Profile settings"
        class="profile-settings"
        @submit.prevent="submitSettings"
      >
        <section aria-labelledby="editable-profile-heading" class="profile-settings__section">
          <header class="profile-settings__section-header">
            <h2 id="editable-profile-heading">Editable profile</h2>
            <p>Control what people see and how new followers connect with you.</p>
          </header>

          <BaseTextAreaField
            v-model="settingsForm.bio"
            :disabled="isSubmittingSettings"
            :error-message="settingsValidationErrors.bio"
            id="profile-settings-bio"
            label="Bio"
            placeholder="Write a short introduction"
          />

          <label class="profile-settings__privacy" for="profile-settings-private">
            <span class="profile-settings__privacy-copy">
              <strong>Private profile</strong>
              <span id="profile-settings-private-description">
                New followers need your approval while this setting is enabled.
              </span>
            </span>

            <input
              v-model="settingsForm.isPrivate"
              :disabled="isSubmittingSettings"
              id="profile-settings-private"
              aria-describedby="profile-settings-private-description"
              class="profile-settings__switch"
              role="switch"
              type="checkbox"
            />
          </label>

          <p
            v-if="settingsValidationErrors.isPrivate"
            class="profile-settings__privacy-error"
            role="alert"
          >
            {{ settingsValidationErrors.isPrivate }}
          </p>
        </section>

        <footer class="profile-settings__actions">
          <BaseButton
            :disabled="isSaveDisabled"
            :is-loading="isSubmittingSettings"
            loading-text="Saving..."
            type="submit"
          >
            Save changes
          </BaseButton>
        </footer>
      </form>
    </Transition>
  </div>
</template>

<style scoped>
.profile-settings {
  display: grid;
}

.profile-settings__section {
  display: grid;
  padding: var(--space-6) var(--space-4);
  gap: var(--space-6);
  border-block-end: 1px solid var(--color-border);
}

.profile-settings__section-header {
  display: grid;
  gap: var(--space-1);
}

.profile-settings__section-header h2 {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.2;
}

.profile-settings__section-header p {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-wrap: pretty;
}

.profile-settings__privacy {
  display: flex;
  min-height: var(--control-height);
  padding: var(--space-4);
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface-raised);
  cursor: pointer;
}

.profile-settings__privacy-copy {
  display: grid;
  min-width: 0;
  gap: var(--space-1);
}

.profile-settings__privacy-copy strong {
  font-weight: var(--font-weight-semibold);
}

.profile-settings__privacy-copy span {
  color: var(--color-text-muted);
  font-size: var(--font-size-support);
  text-wrap: pretty;
}

.profile-settings__switch {
  position: relative;
  width: 3rem;
  height: 1.75rem;
  flex: 0 0 auto;
  margin: 0;
  border: 1px solid var(--color-control-border);
  border-radius: var(--radius-full);
  appearance: none;
  background: var(--color-surface-subtle);
  cursor: pointer;
  transition:
    border-color var(--transition-fast),
    background-color var(--transition-fast);
}

.profile-settings__switch::before {
  position: absolute;
  inset-block-start: 0.1875rem;
  inset-inline-start: 0.1875rem;
  width: 1.25rem;
  height: 1.25rem;
  border-radius: var(--radius-full);
  background: var(--color-control-border);
  content: '';
  transition:
    background-color var(--transition-fast),
    transform var(--transition-fast);
}

.profile-settings__switch:checked {
  border-color: var(--color-action);
  background: var(--color-action);
}

.profile-settings__switch:checked::before {
  background: var(--color-action-contrast);
  transform: translateX(1.25rem);
}

.profile-settings__switch:disabled {
  cursor: not-allowed;
  opacity: 0.65;
}

.profile-settings__privacy-error {
  margin-block-start: calc(var(--space-4) * -1);
  color: var(--color-danger);
  font-size: var(--font-size-support);
}

.profile-settings__actions {
  display: flex;
  padding: var(--space-4);
  justify-content: flex-end;
}

@media (max-width: 35rem) {
  .profile-settings__privacy {
    align-items: flex-start;
  }
}
</style>
