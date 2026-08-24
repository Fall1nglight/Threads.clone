export type AuthResponseDto = {
  accessToken: string
  refreshToken: string
}

export type LoginRequestDto = {
  email: string
  password: string
}

export type SignupRequestDto = {
  username: string
  email: string
  password: string
  isPrivate: boolean
  bio?: string
}
