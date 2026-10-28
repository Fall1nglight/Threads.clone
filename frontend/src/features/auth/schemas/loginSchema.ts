import Joi from 'joi'
import type { LoginRequestDto } from '@/features/auth/authApiTypes.ts'
import { emailSchema, passwordSchema } from '@/features/users/schemas/userFieldSchemas.ts'

const loginSchema = Joi.object<LoginRequestDto, true>({
  email: emailSchema,
  password: passwordSchema,
})

export default loginSchema
