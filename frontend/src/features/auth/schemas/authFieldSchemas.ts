import Joi from 'joi'

export const emailSchema = Joi.string().email().max(256).required().messages({
  'any.required': 'Email is required.',
  'string.empty': 'Email is required.',
  'string.email': 'Enter a valid email address.',
  'string.max': 'Email must not exceed {{#limit}} characters.',
  '*': 'Email is invalid.',
})

export const passwordSchema = Joi.string()
  .min(8)
  .required()
  .messages({
    'any.required': 'Password is required.',
    'string.empty': 'Password is required.',
    'string.min': 'Password must be at least {{#limit}} characters long.',
  })
  .pattern(/[A-Z]/)
  .message('Password must contain at least one uppercase letter.')
  .pattern(/[a-z]/)
  .message('Password must contain at least one lowercase letter.')
  .pattern(/[0-9]/)
  .message('Password must contain at least one number.')
  .pattern(/[^a-zA-Z0-9]/)
  .message('Password must contain at least one special character.')
