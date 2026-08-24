import { getApi } from '@/shared/api/baseApi.ts'
import type {
  AuthResponseDto,
  LoginRequestDto,
  SignupRequestDto,
} from '@/features/auth/authTypes.ts'
const api = getApi('/auth', { retryUnauthorized: false })

const routes = {
  login: '/login',
  signup: '/signup',
  renewToken: '/renew-token',
  logout: '/logout',
  logoutAll: '/logout-all',
}

async function login(payload: LoginRequestDto): Promise<AuthResponseDto> {
  const response = await api.post<AuthResponseDto>(routes.login, payload)
  return response.data
}

async function signup(payload: SignupRequestDto): Promise<AuthResponseDto> {
  const response = await api.post<AuthResponseDto>(routes.signup, payload)
  return response.data
}

async function renewToken(plainToken: string): Promise<AuthResponseDto> {
  const response = await api.post<AuthResponseDto>(routes.renewToken, { refreshToken: plainToken })
  return response.data
}

async function logout(plainToken: string) {
  const response = await api.post(routes.logout, { refreshToken: plainToken })
  return response.data
}

async function logoutAll() {
  const response = await api.post(routes.logoutAll)
  return response.data
}

export default { login, signup, renewToken, logout, logoutAll }
