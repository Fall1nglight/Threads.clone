import Joi from 'joi'
import type { UpdateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { guidContentSchema } from '@/shared/schemas/guidFieldSchema.ts'
import { commentContentSchema } from '@/features/posts/comments/schemas/commentFieldSchemas.ts'

const updateCommentSchema = Joi.object<UpdateCommentDto, true>({
  id: guidContentSchema,
  content: commentContentSchema,
})

export default updateCommentSchema
