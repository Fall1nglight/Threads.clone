import Joi from 'joi'
import type { ChangePasswordDto } from '@/features/auth/authApiTypes.ts'
import { passwordSchema } from '@/features/users/schemas/userFieldSchemas.ts'

const changePasswordSchema = Joi.object<ChangePasswordDto, true>({
  currentPassword: passwordSchema,
  newPassword: passwordSchema.invalid(Joi.ref('currentPassword')).messages({
    'any.invalid': 'New password must differ from your current password.',
  }),
})

export default changePasswordSchema
