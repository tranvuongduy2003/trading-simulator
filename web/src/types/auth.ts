import { z } from 'zod'

const passwordSpecialCharacterPattern = /[!@#$%^&*()_+\-=[\]{}|;:'",.<>?/`~]/

export const registerFormSchema = z
  .object({
    username: z
      .string()
      .trim()
      .min(3, 'Username must be at least 3 characters.')
      .max(32, 'Username must be at most 32 characters.')
      .regex(/^[A-Za-z0-9_]+$/, 'Username may only contain letters, digits, and underscores.'),
    email: z
      .string()
      .trim()
      .min(1, 'Email is required.')
      .email('Enter a valid email address.')
      .max(254, 'Email must be at most 254 characters.'),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters.')
      .regex(/[A-Za-z]/, 'Password must include at least one letter.')
      .regex(/[0-9]/, 'Password must include at least one digit.')
      .regex(
        passwordSpecialCharacterPattern,
        'Password must include at least one special character.',
      ),
    confirmPassword: z.string().min(1, 'Confirm your password.'),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })

export type RegisterFormValues = z.infer<typeof registerFormSchema>
