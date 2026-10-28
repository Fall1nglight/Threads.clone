<script setup lang="ts">
import { computed } from 'vue'
import authApi from '@/features/auth/authApi.ts'
import type { ChangePasswordDto } from '@/features/auth/authApiTypes.ts'
import changePasswordSchema from '@/features/auth/schemas/changePasswordSchema.ts'
import BaseButton from '@/shared/components/BaseButton.vue'
import BaseTextField from '@/shared/components/BaseTextField.vue'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import { useModalStore } from '@/shared/modal/modalStore.ts'

const modalStore = useModalStore()

const initialChangePasswordForm: ChangePasswordDto = {
  currentPassword: '',
  newPassword: '',
}

const {
  payload: changePasswordForm,
  errors: changePasswordValidationErrors,
  submit: submitChangePassword,
  isSubmitting: isSubmittingChangePassword,
} = usePayloadValidator(initialChangePasswordForm, changePasswordSchema, performChangePassword)

const isChangePasswordDisabled = computed(() => {
  const { currentPassword, newPassword } = changePasswordForm.value

  return currentPassword.length === 0 || newPassword.length === 0 || currentPassword === newPassword
})

async function performChangePassword(validPayload: ChangePasswordDto): Promise<void> {
  try {
    await authApi.changePassword(validPayload)

    changePasswordForm.value = {
      currentPassword: '',
      newPassword: '',
    }

    modalStore.openAlertModal('Your password has been changed successfully.')
  } catch {
    modalStore.openAlertModal('Failed to change your password. Please try again later!')
  }
}
</script>

<template>
  <form
    aria-labelledby="change-password-heading"
    class="change-password"
    @submit.prevent="submitChangePassword"
  >
    <h2 id="change-password-heading">Change Password</h2>

    <BaseTextField
      v-model="changePasswordForm.currentPassword"
      :disabled="isSubmittingChangePassword"
      :error-message="changePasswordValidationErrors.currentPassword"
      id="change-password-current"
      label="Current password"
      type="password"
      autocomplete="current-password"
    />

    <BaseTextField
      v-model="changePasswordForm.newPassword"
      :disabled="isSubmittingChangePassword"
      :error-message="changePasswordValidationErrors.newPassword"
      id="change-password-new"
      label="New password"
      type="password"
      autocomplete="new-password"
    />

    <BaseButton
      :disabled="isChangePasswordDisabled"
      :is-loading="isSubmittingChangePassword"
      class="change-password__submit"
      loading-text="Changing password..."
      type="submit"
    >
      Change password
    </BaseButton>
  </form>
</template>

<style scoped>
.change-password {
  display: grid;
  gap: var(--space-4);
  padding: var(--space-6) var(--space-4);
}

.change-password h2 {
  font-size: var(--font-size-title);
  font-weight: var(--font-weight-semibold);
  line-height: 1.2;
}

.change-password__submit {
  justify-self: end;
}
</style>
