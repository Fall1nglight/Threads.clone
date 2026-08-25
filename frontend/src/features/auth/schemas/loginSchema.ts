import Joi from 'joi'
import { emailSchema, passwordSchema } from '@/features/auth/schemas/authFieldSchemas.ts'
import type { LoginRequestDto } from '@/features/auth/authTypes.ts'

const loginSchema = Joi.object<LoginRequestDto, true>({
  email: emailSchema,
  password: passwordSchema,
})

export default loginSchema
