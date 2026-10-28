import { getApi } from '@/shared/api/baseApi.ts'
import type {
  AuthResponseDto,
  ChangePasswordDto,
  LoginRequestDto,
  SignupRequestDto,
} from '@/features/auth/authApiTypes.ts'

const api = getApi('/auth', { retryOnUnauthorized: false })

const paths = {
  login: '/login',
  signup: '/signup',
  renewToken: '/renew-token',
  logout: '/logout',
  logoutAll: '/logout-all',
  changePassword: '/password/change',
} as const

async function login(payload: LoginRequestDto) {
  const response = await api.post<AuthResponseDto>(paths.login, payload)
  return response.data
}

async function signup(payload: SignupRequestDto) {
  const response = await api.post<AuthResponseDto>(paths.signup, payload)
  return response.data
}

async function renewToken(plainToken: string) {
  const response = await api.post<AuthResponseDto>(paths.renewToken, { refreshToken: plainToken })
  return response.data
}

async function logout(plainToken: string) {
  const response = await api.post(paths.logout, { refreshToken: plainToken })
  return response.data
}

async function logoutAll() {
  const response = await api.post(paths.logoutAll)
  return response.data
}

async function changePassword(payload: ChangePasswordDto): Promise<void> {
  await api.put(paths.changePassword, payload)
}

export default { login, signup, renewToken, logout, logoutAll, changePassword }
