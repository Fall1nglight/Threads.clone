import { ref, watch } from 'vue'
import axios from 'axios'

const fallbackErrorMessage: string = 'Something went wrong. Please try again later.'

export function useErrorHandler() {
  const error = ref<Error>()
  const errorMessage = ref<string>('')

  watch(error, (newError) => {
    if (newError === undefined) {
      errorMessage.value = ''
      return
    }

    if (axios.isAxiosError(error.value)) {
      errorMessage.value = error.value.response?.data?.detail ?? fallbackErrorMessage
    } else {
      errorMessage.value = error.value?.message ?? fallbackErrorMessage
    }
  })

  function clearError(): void {
    error.value = undefined
  }

  return { error, errorMessage, clearError }
}
