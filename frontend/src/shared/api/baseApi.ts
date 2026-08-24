import axios from 'axios'

import type { InternalAxiosRequestConfig } from 'axios'

const baseUrl = '/api'
const pahts = {
  baseUrl,
}

export type TokenProvider = () => string | null
export type SessionRenewal = () => Promise<boolean>

export type AuthApiConfiguration = {
  accessTokenProvider: TokenProvider
  renewSession: SessionRenewal
}

export type ApiOptions = {
  retryUnauthorized?: boolean
}

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

let getAccessToken: TokenProvider = () => null
let renewSession: SessionRenewal = async () => false

export function configureAuthApi(configuration: AuthApiConfiguration) {
  getAccessToken = configuration.accessTokenProvider
  renewSession = configuration.renewSession
}

function getApi(endpointPath: string, options: ApiOptions = {}) {
  const { retryUnauthorized = true } = options
  const apiInstance = axios.create({
    baseURL: baseUrl + endpointPath,
    timeout: 7500,
  })

  apiInstance.interceptors.request.use((config) => {
    const accessToken = getAccessToken()
    if (accessToken) config.headers.set('Authorization', `Bearer ${accessToken}`)
    return config
  })

  if (retryUnauthorized) {
    apiInstance.interceptors.response.use(
      (response) => response,
      async (error: unknown) => {
        if (!axios.isAxiosError(error) || error.response?.status !== 401 || !error.config) {
          return Promise.reject(error)
        }

        const originalRequest = error.config as RetryableRequestConfig

        if (originalRequest._retry) return Promise.reject(error)

        originalRequest._retry = true

        const renewed = await renewSession()
        if (!renewed) return Promise.reject(error)

        return apiInstance(originalRequest)
      },
    )
  }

  return apiInstance
}

export { pahts, getApi }
