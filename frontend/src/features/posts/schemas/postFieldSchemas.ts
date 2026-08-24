import Joi from 'joi'

export const postContentSchema = Joi.string().max(600).pattern(/\S/).required().messages({
  'any.required': 'Content is required.',
  'string.empty': 'Content is required.',
  'string.max': 'Content must not exceed {{#limit}} characters.',
  'string.pattern.base': 'Content is required.',
})
