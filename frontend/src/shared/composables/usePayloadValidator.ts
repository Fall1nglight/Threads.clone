import { ref } from 'vue'
import type { ObjectSchema, ValidationErrorItem, ValidationResult } from 'joi'
import type { Ref } from 'vue'

export type FieldErrors<T extends object> = {
  [Key in keyof T]: string | null
}

function sleep(milliseconds: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, milliseconds))
}

export function usePayloadValidator<T extends object>(
  initialPayload: T,
  schema: ObjectSchema<T>,
  onValid: (payload: T) => void | Promise<void>,
) {
  const payload = ref(initialPayload) as Ref<T>
  const errors = ref(createEmptyErrors(initialPayload)) as Ref<FieldErrors<T>>
  const isSubmitting = ref(false)

  async function submit(): Promise<void> {
    if (isSubmitting.value) return

    errors.value = createEmptyErrors(payload.value)

    const result: ValidationResult<T> = schema.validate(payload.value, {
      abortEarly: false,
    })

    if (result.error) {
      // sort error messages to ensure that error messages containing information about length are shown first
      result.error.details.sort((a, b): number => {
        if (a.type.endsWith('.min') || a.type.endsWith('.max')) return -1
        return 0
      })

      for (const detail of result.error.details) {
        const fieldName = detail.path[0] as keyof T

        // only save the first message
        if (errors.value[fieldName] !== null) continue

        errors.value[fieldName] = detail.message
      }

      return
    }

    isSubmitting.value = true

    try {
      await sleep(2000)
      await onValid(result.value)
    } finally {
      isSubmitting.value = false
    }
  }

  return {
    payload,
    errors,
    isSubmitting,
    submit,
  }
}

function createEmptyErrors<T extends object>(payload: T): FieldErrors<T> {
  const keys = Object.keys(payload) as (keyof T)[]
  return Object.fromEntries(keys.map((key) => [key, null])) as FieldErrors<T>
}
