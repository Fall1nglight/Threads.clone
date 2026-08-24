import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { jwtDecode } from 'jwt-decode'
import authApi from '@/features/auth/authApi.ts'
import axios from 'axios'
import { configureAuthApi } from '@/shared/api/baseApi.ts'
import type {
  AuthResponseDto,
  LoginRequestDto,
  SignupRequestDto,
} from '@/features/auth/authTypes.ts'

export const REFRESH_TOKEN_STORAGE_KEY = 'threads.refreshToken'
export type SessionStatus = 'uninitialized' | 'restoring' | 'anonymous' | 'authenticated'
export type User = {
  id: string
  username: string
  email: string
}

export const NAME_IDENTIFIER_CLAIM =
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'

export const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

export const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'

export type AccessTokenClaims = {
  [NAME_IDENTIFIER_CLAIM]: string
  [NAME_CLAIM]: string
  [EMAIL_CLAIM]: string
}

export const useAuthStore = defineStore('auth', () => {
  const sessionStatus = ref<SessionStatus>('uninitialized')
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const user = ref<User | null>(null)
  const isAuthenticated = computed<boolean>(
    () => sessionStatus.value == 'authenticated' && accessToken.value != null,
  )
  const currentUserId = computed<string | null>(() => user.value?.id ?? null)
  let renewalPromise: Promise<boolean> | null = null

  configureAuthApi({
    accessTokenProvider: () => accessToken.value,
    renewSession: () => renewSession(),
  })

  async function initialize() {
    sessionStatus.value = 'restoring'

    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY)

    if (storedRefreshToken == null) {
      sessionStatus.value = 'anonymous'
      return
    }

    try {
      await renewSession(storedRefreshToken)
    } catch {
      return
    }
  }

  void initialize()

  async function renewSession(plainToken: string | null = refreshToken.value): Promise<boolean> {
    if (renewalPromise) return renewalPromise

    if (!plainToken) {
      $reset()
      return false
    }

    renewalPromise = renewTokens(plainToken)

    try {
      return await renewalPromise
    } finally {
      renewalPromise = null
    }
  }

  async function renewTokens(plainToken: string): Promise<boolean> {
    try {
      const tokens: AuthResponseDto = await authApi.renewToken(plainToken)
      setSession(tokens.accessToken, tokens.refreshToken, 'authenticated')
      return true
    } catch (error) {
      handleError(error)
      $reset()
      throw error
    }
  }

  // update access,refresh tokens, sessionStatus and set user
  function setSession(acToken: string, rfToken: string, sessionState: SessionStatus) {
    accessToken.value = acToken
    refreshToken.value = rfToken
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, rfToken)
    sessionStatus.value = sessionState
    setUser(acToken)
  }

  // process jwt claims
  function setUser(accessToken: string) {
    const claims = jwtDecode<AccessTokenClaims>(accessToken)

    user.value = {
      id: claims[NAME_IDENTIFIER_CLAIM],
      username: claims[NAME_CLAIM],
      email: claims[EMAIL_CLAIM],
    }
  }

  async function signup(payload: SignupRequestDto) {
    const response = await authApi.signup(payload)
    setSession(response.accessToken, response.refreshToken, 'authenticated')
  }

  async function login(payload: LoginRequestDto) {
    const response = await authApi.login(payload)
    setSession(response.accessToken, response.refreshToken, 'authenticated')
  }

  async function logout() {
    const tokenToLogout = refreshToken.value

    if (!tokenToLogout) {
      $reset()
      return
    }

    try {
      await authApi.logout(tokenToLogout)
    } catch (error) {
      if (!axios.isAxiosError(error) || error.response?.status !== 401) throw error

      const renewed = await renewSession()
      if (!renewed || !refreshToken.value) return

      await authApi.logout(refreshToken.value)
    }

    $reset()
  }

  async function logoutAll() {
    await authApi.logoutAll()
    $reset()
  }

  function handleError(error: unknown) {
    if (axios.isAxiosError(error)) {
      console.error('Auth request failed', {
        status: error.response?.status,
        data: error.response?.data,
        code: error.code,
      })
    } else {
      console.error('Unexpected error', error)
    }
  }

  function $reset() {
    sessionStatus.value = 'anonymous'
    accessToken.value = null
    refreshToken.value = null
    localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY)
    user.value = null
  }

  return {
    sessionStatus,
    accessToken,
    refreshToken,
    isAuthenticated,
    user,
    currentUserId,
    signup,
    login,
    renewSession,
    logout,
    logoutAll,
  }
})
