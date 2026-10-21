import Joi from 'joi'

const commentContentSchema = Joi.string().max(600).pattern(/\S/).required().messages({
  'any.required': 'Content is required.',
  'string.empty': 'Content is required.',
  'string.max': 'Content must not exceed {{#limit}} characters.',
  'string.pattern.base': 'Content is required.',
})

export { commentContentSchema }
