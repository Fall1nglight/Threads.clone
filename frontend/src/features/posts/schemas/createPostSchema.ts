import Joi from 'joi'
import type { CreatePostDto } from '@/features/posts/postTypes.ts'
import { postContentSchema } from '@/features/posts/schemas/postFieldSchemas.ts'

const createPostSchema = Joi.object<CreatePostDto, true>({
  content: postContentSchema,
})

export default createPostSchema
