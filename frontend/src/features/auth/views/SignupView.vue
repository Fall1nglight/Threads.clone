<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import AuthCard from '@/features/auth/components/AuthCard.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import type { SignupRequestDto } from '@/features/auth/authTypes.ts'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import signupSchema from '@/features/auth/schemas/signupSchema.ts'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'

const router = useRouter()

const authStore = useAuthStore()
const { isAuthenticated } = storeToRefs(authStore)

const initialSignupForm: SignupRequestDto = {
  username: '',
  email: '',
  password: '',
  bio: '',
  isPrivate: false,
}

const {
  payload: signupForm,
  errors: signupValidationErrors,
  submit: submitSignup,
  isSubmitting: isSubmittingSignup,
} = usePayloadValidator(initialSignupForm, signupSchema, performSignup)

const {
  error: signupApiError,
  errorMessage: signupApiErrorMsg,
  clearError: clearApiError,
} = useErrorHandler()

const fallbackErrorMessage: string = 'Something went wrong during signup. Please try again later.'

async function performSignup(validPayload: SignupRequestDto) {
  try {
    await authStore.signup(validPayload)

    if (!isAuthenticated.value) {
      signupApiError.value = new Error(fallbackErrorMessage)
      return
    }

    router.push('/')
  } catch (err) {
    signupApiError.value = err as Error
  }
}
</script>

<template>
  <AuthCard
    subtitle="Join a place built for thoughtful, public conversation."
    title="Create an account"
  >
    <form aria-label="Signup details" class="auth-form" @submit.prevent="submitSignup">
      <BaseTextField
        v-model="signupForm.username"
        :error-message="signupValidationErrors.username"
        id="signup-username"
        autocomplete="username"
        label="Username"
        placeholder="your.username"
        variant="subtle"
      />

      <BaseTextField
        v-model="signupForm.email"
        :error-message="signupValidationErrors.email"
        id="signup-email"
        autocomplete="email"
        label="Email"
        placeholder="name@example.com"
        type="email"
        variant="subtle"
      />

      <BaseTextField
        v-model="signupForm.password"
        :error-message="signupValidationErrors.password"
        id="signup-password"
        autocomplete="new-password"
        label="Password"
        placeholder="Create a password"
        type="password"
        variant="subtle"
      />

      <BaseTextField
        v-model="signupForm.bio"
        :error-message="signupValidationErrors.bio"
        id="signup-bio"
        label="Bio"
        placeholder="Short introduction"
        type="text"
        variant="subtle"
      />

      <AuthErrorAlert :message="signupApiErrorMsg" @close-alert="clearApiError" />

      <BaseButton
        :is-loading="isSubmittingSignup"
        loading-text="Creating account..."
        class="auth-form__submit"
        type="submit"
        >Create account</BaseButton
      >
    </form>

    <p class="auth-switch">
      Already have an account?
      <RouterLink to="/auth/login">Log in</RouterLink>
    </p>
  </AuthCard>
</template>

<style scoped>
.auth-form {
  display: grid;
  gap: var(--space-4);
}

.auth-form__submit {
  justify-self: center;
  margin-block-start: var(--space-2);
}

.auth-switch {
  color: var(--color-text-muted);
  text-align: center;
}

.auth-switch a {
  display: inline-flex;
  min-height: var(--control-height);
  align-items: center;
  color: var(--color-text);
  font-weight: var(--font-weight-semibold);
}
</style>
