import {
  type AccessTokenClaims,
  type AuthUser,
  EmailClaim,
  IdClaim,
  type SessionState,
  type SessionTokens,
  type UnvalidatedAccessTokenClaims,
  type UnvalidatedSessionTokens,
  UsernameClaim,
} from '@/features/auth/stores/authStoreTypes.ts'
import type { LoginRequestDto, SignupRequestDto } from '@/features/auth/authApiTypes.ts'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { jwtDecode } from 'jwt-decode'
import { useModalStore } from '@/shared/modal/modalStore.ts'
import { setAccessTokenProvider, setSessionRenewer } from '@/shared/api/baseApi.ts'
import authApi from '@/features/auth/authApi.ts'
import { getInitials } from '@/shared/helpers/profileInitials.ts'

const LOCAL_STORAGE_REFRESH_TOKEN_KEY = 'threads.clone.refreshtoken'

export const useAuthStore = defineStore('auth', () => {
  // other stores
  const modalStore = useModalStore()

  // state
  const session = ref<SessionState>({
    status: 'uninitialized',
  })

  const renewSessionPromise = ref<Promise<void> | null>(null)
  const initializationPromise = ref<Promise<void> | null>(null)

  // configure baseApi
  setAccessTokenProvider(() => {
    if (session.value.status === 'authenticated') return session.value.tokens.accessToken
    return ''
  })

  setSessionRenewer(() => renewSession())

  // getters
  const isLoggedIn = computed<boolean>(() => session.value.status === 'authenticated')
  const isAnonymous = computed<boolean>(() => session.value.status === 'anonymous')
  const isInitializing = computed<boolean>(() => session.value.status === 'initializing')

  const loggedInUser = computed<AuthUser | null>(() => {
    if (session.value.status !== 'authenticated') return null
    return session.value.user
  })

  const loggedInUserId = computed<string | null>(() => {
    if (session.value.status !== 'authenticated') return null
    return session.value.user.id
  })

  const loggedInUsername = computed<string | null>(() => {
    if (session.value.status !== 'authenticated') return null
    return session.value.user.username
  })

  const loggedInUserInitials = computed<string | null>(() => {
    if (session.value.status !== 'authenticated') return null
    return getInitials(session.value.user.username)
  })

  const loggedInUserEmail = computed<string | null>(() => {
    if (session.value.status !== 'authenticated') return null
    return session.value.user.email
  })

  // initialization
  async function ensureSessionReady() {
    if (!initializationPromise.value) {
      initializationPromise.value = initializeCore().catch(() => {
        modalStore.openAlertModal('Failed to restore session. Please login again!')
      })
    }

    return initializationPromise.value
  }

  async function initializeCore() {
    session.value = {
      status: 'initializing',
    }

    const refreshToken = localStorage.getItem(LOCAL_STORAGE_REFRESH_TOKEN_KEY)
    if (!refreshToken) {
      $reset()
      return
    }

    await renewSession()
  }

  // actions
  async function renewSession() {
    if (renewSessionPromise.value) return renewSessionPromise.value
    renewSessionPromise.value = renewSessionCore()

    // dont need to catch as we rethrow the error for baseApi to cancel retry on http 401
    try {
      await renewSessionPromise.value
    } finally {
      renewSessionPromise.value = null
    }
  }

  async function renewSessionCore() {
    const refreshToken = localStorage.getItem(LOCAL_STORAGE_REFRESH_TOKEN_KEY)
    if (!refreshToken) {
      $reset()
      throw new Error('Refresh token is missing.')
    }

    try {
      const tokens = await authApi.renewToken(refreshToken)
      establishSession(tokens as SessionTokens)
    } catch (e) {
      $reset()
      throw e
    }
  }

  async function login(credentials: LoginRequestDto) {
    const tokens = await authApi.login(credentials)
    establishSession(tokens as SessionTokens)
  }

  async function signup(credentials: SignupRequestDto) {
    const tokens = await authApi.signup(credentials)
    establishSession(tokens as SessionTokens)
  }

  async function logoutSingle() {
    if (session.value.status === 'authenticated') {
      await authApi.logout(session.value.tokens.refreshToken)
    }

    $reset()
  }

  async function logoutAll() {
    if (session.value.status === 'authenticated') {
      await authApi.logoutAll()
    }

    $reset()
  }

  // helpers
  function extractClaimsFromToken(accessToken: string): UnvalidatedAccessTokenClaims {
    return jwtDecode<UnvalidatedAccessTokenClaims>(accessToken)
  }

  function assertClaims(claims: UnvalidatedAccessTokenClaims): asserts claims is AccessTokenClaims {
    if (typeof claims[IdClaim] !== 'string' || claims[IdClaim].length === 0) {
      throw new Error('User ID claim is invalid.')
    }

    if (typeof claims[EmailClaim] !== 'string' || claims[EmailClaim].length === 0) {
      throw new Error('Email claim is invalid.')
    }

    if (typeof claims[UsernameClaim] !== 'string' || claims[UsernameClaim].length === 0) {
      throw new Error('Username claim is invalid.')
    }
  }

  function assertTokens(tokens: UnvalidatedSessionTokens): asserts tokens is SessionTokens {
    if (typeof tokens.accessToken !== 'string' || tokens.accessToken.length === 0) {
      throw new Error('Access token is invalid.')
    }

    if (typeof tokens.refreshToken !== 'string' || tokens.refreshToken.length === 0) {
      throw new Error('Refresh token is invalid.')
    }
  }

  function createAuthUser(claims: AccessTokenClaims) {
    const user: AuthUser = {
      username: claims[UsernameClaim],
      email: claims[EmailClaim],
      id: claims[IdClaim],
    }
    return user
  }

  function persistRefreshToken(refreshToken: string) {
    localStorage.setItem(LOCAL_STORAGE_REFRESH_TOKEN_KEY, refreshToken)
  }

  function establishSession(tokens: UnvalidatedSessionTokens) {
    assertTokens(tokens)

    const claims = extractClaimsFromToken(tokens.accessToken)
    assertClaims(claims)

    const user = createAuthUser(claims)
    persistRefreshToken(tokens.refreshToken)

    session.value = {
      status: 'authenticated',
      tokens,
      user,
    }
  }

  function $reset() {
    session.value = {
      status: 'anonymous',
    }
    localStorage.removeItem(LOCAL_STORAGE_REFRESH_TOKEN_KEY)
  }

  return {
    session,
    isLoggedIn,
    isAnonymous,
    isInitializing,
    loggedInUser,
    loggedInUserId,
    loggedInUsername,
    loggedInUserInitials,
    loggedInUserEmail,
    ensureSessionReady,
    login,
    signup,
    logoutSingle,
    logoutAll,
    renewSession,
  }
})
