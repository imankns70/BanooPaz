import type { DailyMenuDto } from '../types'
import { ApiError, apiGet } from './apiClient'

export async function getTodayMenu(): Promise<DailyMenuDto | null> {
  const date = new Date()
  const localDate = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
  // TODO: Replace admin route with public customer menu endpoint after backend adds /api/menus/today.
  try {
    return await apiGet<DailyMenuDto>(`/api/admin/daily-menus/by-date/${localDate}`)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}
