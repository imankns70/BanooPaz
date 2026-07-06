import type { CustomerProfileDto, CustomerProfileLookupRequest } from '../types'
import { ApiError, apiPost } from './apiClient'

export async function getMyCustomerProfile(request: CustomerProfileLookupRequest): Promise<CustomerProfileDto | null> {
  try {
    return await apiPost<CustomerProfileDto>('/api/customers/me', request)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}
