import Joi from 'joi'
import type { UpdatePostDto } from '@/features/posts/postTypes.ts'
import { postContentSchema } from '@/features/posts/schemas/postFieldSchemas.ts'
import { guidContentSchema } from '@/shared/schemas/guidFieldSchema.ts'

const updatePostSchema = Joi.object<UpdatePostDto, true>({
  id: guidContentSchema,
  content: postContentSchema,
})

export default updatePostSchema
