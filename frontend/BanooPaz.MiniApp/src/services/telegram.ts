type TelegramUser = { id?: number; username?: string }
type TelegramWindow = Window & { Telegram?: { WebApp?: { initDataUnsafe?: { user?: TelegramUser } } } }

export function getTelegramUser(): TelegramUser | null {
  // TODO: Validate Telegram initData on backend before trusting Telegram user data.
  return (window as TelegramWindow).Telegram?.WebApp?.initDataUnsafe?.user ?? null
}
