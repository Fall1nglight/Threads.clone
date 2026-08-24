import Joi from 'joi'
import { postContentSchema } from '@/features/posts/schemas/postFieldSchemas.ts'
import type { CreateCommentDto } from '@/features/posts/comments/commentTypes.ts'

const createCommentSchema = Joi.object<CreateCommentDto, true>({
  content: postContentSchema,
})

export default createCommentSchema
