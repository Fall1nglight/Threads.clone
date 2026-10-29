import { getApi } from '@/shared/api/baseApi.ts'

const api = getApi('/health', {
  retryOnUnauthorized: false,
  includeCredentials: false,
  timeout: 5_000,
})

const paths = {
  checkApi: '/api',
} as const

async function check(timeout: number): Promise<void> {
  await api.get(paths.checkApi, { timeout })
}

export default {
  check,
}
