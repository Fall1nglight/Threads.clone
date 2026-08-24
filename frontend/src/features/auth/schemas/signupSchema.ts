import Joi from 'joi'
import type { SignupRequestDto } from '@/features/auth/authApi.ts'
import { emailSchema, passwordSchema } from '@/features/auth/schemas/authFieldSchemas.ts'

const signupSchema = Joi.object<SignupRequestDto, true>({
  username: Joi.string()
    .min(3)
    .max(30)
    .pattern(/^[a-zA-Z0-9_.]+$/)
    .required()
    .messages({
      'any.required': 'Username is required.',
      'string.empty': 'Username is required.',
      'string.min': 'Username must be at least {{#limit}} characters long.',
      'string.max': 'Username must not exceed {{#limit}} characters.',
      'string.pattern.base':
        'Username may only contain letters, numbers, underscores, and periods.',
    }),

  email: emailSchema,

  // todo | revise backend dto contract as signup and login dtos differ
  password: passwordSchema.max(100).messages({
    'string.max': 'Password must not exceed {{#limit}} characters.',
  }),

  isPrivate: Joi.boolean().required().messages({
    'any.required': 'Privacy setting is required.',
    'boolean.base': 'Privacy setting must be true or false.',
  }),

  bio: Joi.string().max(300).allow('').optional().messages({
    'string.max': 'Bio must not exceed {{#limit}} characters.',
  }),
})

export default signupSchema
