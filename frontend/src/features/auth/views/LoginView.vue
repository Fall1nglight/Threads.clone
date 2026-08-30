<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import AuthCard from '@/features/auth/components/AuthCard.vue'
import BaseButton from '@/shared/ui/BaseButton.vue'
import BaseTextField from '@/shared/ui/BaseTextField.vue'
import { useAuthStore } from '@/features/auth/stores/authStore.ts'
import { storeToRefs } from 'pinia'
import { usePayloadValidator } from '@/shared/composables/usePayloadValidator.ts'
import loginSchema from '@/features/auth/schemas/loginSchema.ts'
import type { LoginRequestDto } from '@/features/auth/authTypes.ts'
import AuthErrorAlert from '@/features/auth/components/AuthErrorAlert.vue'
import { useErrorHandler } from '@/shared/composables/useErrorHandler.ts'

const router = useRouter()

const authStore = useAuthStore()
const { isAuthenticated } = storeToRefs(authStore)

const initialLoginForm: LoginRequestDto = { email: '', password: '' }

const {
  payload: loginForm,
  errors: loginValidationErrors,
  submit: submitLogin,
  isSubmitting: isSubmittingLogin,
} = usePayloadValidator(initialLoginForm, loginSchema, performLogin)

const {
  error: loginApiError,
  errorMessage: loginApiErrorMsg,
  clearError: clearApiError,
} = useErrorHandler()

const fallbackErrorMessage: string = 'Something went wrong during login. Please try again later.'

async function performLogin(validPayload: LoginRequestDto) {
  try {
    await authStore.login(validPayload)

    if (!isAuthenticated.value) {
      loginApiError.value = new Error(fallbackErrorMessage)
      return
    }

    await router.push('/')
  } catch (err) {
    loginApiError.value = err as Error
  }
}
</script>

<template>
  <AuthCard subtitle="Welcome back to the conversation." title="Log in">
    <form aria-label="Login details" class="auth-form" @submit.prevent="submitLogin">
      <BaseTextField
        v-model="loginForm.email"
        :error-message="loginValidationErrors.email"
        id="login-identity"
        label="Email"
        placeholder="name@example.com"
        type="text"
        variant="subtle"
      />

      <BaseTextField
        v-model="loginForm.password"
        :error-message="loginValidationErrors.password"
        id="login-password"
        label="Password"
        placeholder="Enter your password"
        type="password"
        variant="subtle"
      />

      <AuthErrorAlert :message="loginApiErrorMsg" @close-alert="clearApiError" />

      <BaseButton
        :is-loading="isSubmittingLogin"
        loading-text="Logging in..."
        class="auth-form__submit"
        type="submit"
        >Log in</BaseButton
      >
    </form>

    <p class="auth-switch">
      New to Threads?
      <RouterLink to="/auth/signup">Create an account</RouterLink>
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
