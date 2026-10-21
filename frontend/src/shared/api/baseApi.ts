import type {
  AccessTokenProvider,
  RetryableAxiosRequest,
  InitOptions,
  SessionRenewer,
} from '@/shared/api/baseApiTypes.ts'

import axios from 'axios'
import type { PagedRequest } from '@/shared/types/paginationTypes.ts'

// see vite.config.ts for more info
const BASE_PATH = '/api'
const DEFAULT_ACCESS_TOKEN_PREFIX = 'Bearer '
let requestAccessToken: AccessTokenProvider = () => null

let renewSession: SessionRenewer = async () => {
  throw new Error('Session renewer not set')
}

export function setAccessTokenProvider(provider: AccessTokenProvider) {
  requestAccessToken = provider
}

export function setSessionRenewer(renewer: SessionRenewer) {
  renewSession = renewer
}

export function getApi(path: string, options: InitOptions = {}) {
  const {
    retryOnUnauthorized = true,
    includeCredentials = true,
    accessTokenPrefix = DEFAULT_ACCESS_TOKEN_PREFIX,
    timeout = 10000,
  } = options

  const api = axios.create({
    baseURL: BASE_PATH + path,
    timeout,
  })

  // include access token for authentication if desired
  if (includeCredentials) {
    api.interceptors.request.use((config) => {
      const accessToken = requestAccessToken()
      if (accessToken) {
        config.headers.Authorization = accessTokenPrefix + accessToken
      }
      return config
    })
  }

  // retry failed 401 requests
  if (retryOnUnauthorized) {
    api.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (!axios.isAxiosError(error)) throw error
        if (error.response?.status !== 401) throw error
        if (!error.config) throw error

        const request = error.config as RetryableAxiosRequest
        if (request.hasRetried) throw error
        request.hasRetried = true

        await renewSession()
        return api(request)
      },
    )
  }

  return api
}

export function getPaginatedUrl(path: string, paginationDetails: PagedRequest) {
  const separator = path.includes('?') ? '&' : '?'
  return `${path}${separator}pageSize=${paginationDetails.pageSize}&cursor=${paginationDetails.cursor}`
}
