import Joi from 'joi'
import type { UpdateUserDto } from '@/features/users/userTypes.ts'
import { bioSchema, isPrivateSchema } from '@/features/users/schemas/userFieldSchemas.ts'

const updateUserSchema = Joi.object<UpdateUserDto, true>({
  isPrivate: isPrivateSchema,
  bio: bioSchema,
})

export default updateUserSchema
