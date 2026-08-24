import Joi from 'joi'
import type { LoginRequestDto } from '@/features/auth/authApi.ts'
import { emailSchema, passwordSchema } from '@/features/auth/schemas/authFieldSchemas.ts'

const loginSchema = Joi.object<LoginRequestDto, true>({
  email: emailSchema,
  password: passwordSchema,
})

export default loginSchema
