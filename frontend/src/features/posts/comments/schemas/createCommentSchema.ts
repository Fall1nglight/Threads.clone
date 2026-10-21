import Joi from 'joi'
import type { CreateCommentDto } from '@/features/posts/comments/commentTypes.ts'
import { commentContentSchema } from '@/features/posts/comments/schemas/commentFieldSchemas.ts'

const createCommentSchema = Joi.object<CreateCommentDto, true>({
  content: commentContentSchema,
})

export default createCommentSchema
