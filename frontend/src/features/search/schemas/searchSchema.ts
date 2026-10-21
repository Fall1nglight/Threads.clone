import Joi from 'joi'
import type { SearchForm } from '@/features/search/searchTypes.ts'
import { searchContentSchema } from '@/features/search/schemas/searchFieldSchemas.ts'

const searchSchema = Joi.object<SearchForm, true>({
  username: searchContentSchema,
})

export default searchSchema
