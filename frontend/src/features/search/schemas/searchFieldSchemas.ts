import Joi from 'joi'

const searchContentSchema = Joi.string()
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

export { searchContentSchema }
