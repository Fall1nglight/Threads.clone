import Joi from 'joi'

export const guidContentSchema = Joi.string()
  .guid()
  .invalid('00000000-0000-0000-0000-000000000000')
  .required()
  .messages({
    'any.invalid': 'Post ID is required.',
    'any.required': 'Post ID is required.',
    'string.empty': 'Post ID is required.',
    'string.guid': 'Post ID must be a valid GUID.',
  })
