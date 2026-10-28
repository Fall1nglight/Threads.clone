import Joi from 'joi'

const usernameSchema = Joi.string()
  .min(3)
  .max(30)
  .pattern(/^[a-zA-Z0-9_.]+$/)
  .required()
  .messages({
    'any.required': 'Username is required.',
    'string.empty': 'Username is required.',
    'string.min': 'Username must be at least {{#limit}} characters long.',
    'string.max': 'Username must not exceed {{#limit}} characters.',
    'string.pattern.base': 'Username may only contain letters, numbers, underscores, and periods.',
  })

const emailSchema = Joi.string().email().max(256).required().messages({
  'any.required': 'Email is required.',
  'string.empty': 'Email is required.',
  'string.email': 'Enter a valid email address.',
  'string.max': 'Email must not exceed {{#limit}} characters.',
  '*': 'Email is invalid.',
})

const passwordSchema = Joi.string()
  .min(8)
  .max(100)
  .required()
  .messages({
    'any.required': 'Password is required.',
    'string.empty': 'Password is required.',
    'string.min': 'Password must be at least {{#limit}} characters long.',
    'string.max': 'Password must not exceed {{#limit}} characters.',
  })
  .pattern(/[A-Z]/)
  .message('Password must contain at least one uppercase letter.')
  .pattern(/[a-z]/)
  .message('Password must contain at least one lowercase letter.')
  .pattern(/[0-9]/)
  .message('Password must contain at least one number.')
  .pattern(/[^a-zA-Z0-9]/)
  .message('Password must contain at least one special character.')

const isPrivateSchema = Joi.boolean().required().messages({
  'any.required': 'Privacy setting is required.',
  'boolean.base': 'Privacy setting must be true or false.',
})

const bioSchema = Joi.string().max(300).allow('', null).optional().messages({
  'string.max': 'Bio must not exceed {{#limit}} characters.',
})

export { usernameSchema, emailSchema, passwordSchema, isPrivateSchema, bioSchema }
