import type { InternalAxiosRequestConfig } from 'axios'

export type InitOptions = {
  retryOnUnauthorized?: boolean
  includeCredentials?: boolean
  accessTokenPrefix?: string
  timeout?: number
}

export type AccessTokenProvider = () => string | null
export type RefreshTokenProvider = () => string | null
export type SessionRenewer = () => Promise<void>
export type RetryableAxiosRequest = { hasRetried: boolean } & InternalAxiosRequestConfig
