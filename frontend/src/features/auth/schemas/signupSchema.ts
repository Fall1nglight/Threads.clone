import Joi from 'joi'
import type { SignupRequestDto } from '@/features/auth/authApiTypes.ts'
import {
  bioSchema,
  emailSchema,
  isPrivateSchema,
  passwordSchema,
  usernameSchema,
} from '@/features/users/schemas/userFieldSchemas.ts'

const signupSchema = Joi.object<SignupRequestDto, true>({
  username: usernameSchema,
  email: emailSchema,
  password: passwordSchema,
  isPrivate: isPrivateSchema,
  bio: bioSchema,
})

export default signupSchema
