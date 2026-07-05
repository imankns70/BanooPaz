type TelegramUser = { id?: number; username?: string }
type TelegramWindow = Window & { Telegram?: { WebApp?: { initData?: string; initDataUnsafe?: { user?: TelegramUser } } } }

export function getTelegramUser(): TelegramUser | null {
  return (window as TelegramWindow).Telegram?.WebApp?.initDataUnsafe?.user ?? null
}

export function getTelegramInitData(): string | null {
  return (window as TelegramWindow).Telegram?.WebApp?.initData || null
}
