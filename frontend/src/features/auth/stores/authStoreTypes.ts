export const IdClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
export const EmailClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
export const UsernameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

export type AccessTokenClaims = {
  [IdClaim]: string
  [EmailClaim]: string
  [UsernameClaim]: string
}

export type UnvalidatedAccessTokenClaims = Partial<AccessTokenClaims>

export type SessionTokens = {
  accessToken: string
  refreshToken: string
}

export type UnvalidatedSessionTokens = Partial<SessionTokens>

export type AuthUser = {
  id: string
  email: string
  username: string
}

export type SessionState =
  | {
      status: 'uninitialized'
    }
  | {
      status: 'initializing'
    }
  | {
      status: 'anonymous'
    }
  | {
      status: 'authenticated'
      user: AuthUser
      tokens: SessionTokens
    }
