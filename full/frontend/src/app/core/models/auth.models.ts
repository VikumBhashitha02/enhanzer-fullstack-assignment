export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserInfo {
  email: string;
  displayName: string;
  companyCode: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  user?: UserInfo;
  locationsCount: number;
}

