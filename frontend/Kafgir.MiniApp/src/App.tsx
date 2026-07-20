import { useEffect, useState } from 'react'
import './App.css'
import { CartPage } from './features/cart/CartPage'
import { MenuPage } from './features/menu/MenuPage'
import { OrderSuccess } from './features/orders/OrderSuccess'
import { getTodayMenu } from './services/menuApi'
import { bindTelegramBackButton } from './services/telegram'
import type { CartItem, DailyMenuDto, OrderDto } from './types'

type Page = 'menu' | 'cart' | 'success'
const cartStorageKey = 'kafgir.cart'

function loadStoredCart(): CartItem[] {
  try {
    const value = JSON.parse(localStorage.getItem(cartStorageKey) ?? '[]') as unknown
    if (!Array.isArray(value)) return []
    return value.filter((item): item is CartItem => {
      if (typeof item !== 'object' || item === null) return false
      const candidate = item as Partial<CartItem>
      return typeof candidate.dailyMenuItemId === 'number'
        && typeof candidate.foodName === 'string'
        && typeof candidate.unitPrice === 'number'
        && typeof candidate.quantity === 'number'
        && candidate.quantity > 0
        && typeof candidate.remainingPortions === 'number'
    })
  } catch {
    return []
  }
}

function App() {
  const [page, setPage] = useState<Page>('menu')
  const [menu, setMenu] = useState<DailyMenuDto | null>(null)
  const [cart, setCart] = useState<CartItem[]>(loadStoredCart)
  const [order, setOrder] = useState<OrderDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [menuError, setMenuError] = useState<string | null>(null)

  const loadMenu = async () => {
    setIsLoading(true)
    setMenuError(null)
    try {
      const latestMenu = await getTodayMenu()
      setMenu(latestMenu)
      if (!latestMenu) {
        setCart([])
      } else {
        const latestItems = new Map(latestMenu.items.map((item) => [item.id, item]))
        setCart((current) => current.flatMap((cartItem) => {
          const latestItem = latestItems.get(cartItem.dailyMenuItemId)
          if (!latestMenu.isOpen || !latestItem?.isAvailable || latestItem.remainingPortions <= 0) return []
          return [{
            ...cartItem,
            foodName: latestItem.foodName,
            unitPrice: latestItem.price,
            remainingPortions: latestItem.remainingPortions,
            quantity: Math.min(cartItem.quantity, latestItem.remainingPortions),
          }]
        }))
      }
    } catch (error) {
      setMenuError(error instanceof Error ? error.message : 'دریافت منوی امروز ناموفق بود.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => { void loadMenu() }, [])

  useEffect(() => {
    localStorage.setItem(cartStorageKey, JSON.stringify(cart))
  }, [cart])

  useEffect(() => bindTelegramBackButton(page === 'menu'
    ? null
    : () => {
        if (page === 'success') setOrder(null)
        setPage('menu')
      }), [page])

  const addToCart = (item: DailyMenuDto['items'][number]) => {
    setCart((current) => {
      const existing = current.find((cartItem) => cartItem.dailyMenuItemId === item.id)
      if (existing) {
        return current.map((cartItem) => cartItem.dailyMenuItemId === item.id
          ? { ...cartItem, quantity: Math.min(cartItem.quantity + 1, cartItem.remainingPortions) }
          : cartItem)
      }
      return [...current, {
        dailyMenuItemId: item.id,
        foodName: item.foodName,
        unitPrice: item.price,
        quantity: 1,
        remainingPortions: item.remainingPortions,
      }]
    })
  }

  const updateQuantity = (id: number, quantity: number) => {
    setCart((current) => current
      .map((item) => item.dailyMenuItemId === id
        ? { ...item, quantity: Math.min(quantity, item.remainingPortions) }
        : item)
      .filter((item) => item.quantity > 0))
  }

  const handleSuccess = (createdOrder: OrderDto) => {
    setOrder(createdOrder)
    setCart([])
    setPage('success')
  }

  return (
    <div className="app-shell" dir="rtl">
      <header className="app-header">
        <div className="brand-lockup">
          <div className="brand-mark" aria-hidden="true">ک</div>
          <div>
            <p className="brand">کفگیر</p>
            <p className="tagline">غذای خونگی، با عشق</p>
          </div>
        </div>
        {page === 'menu' && (
          <button className="cart-button" onClick={() => setPage('cart')}>
            <span className="cart-label">سبد خرید</span>
            <span className="cart-count">{cart.reduce((sum, item) => sum + item.quantity, 0)}</span>
          </button>
        )}
      </header>

      {page === 'menu' && (
        <MenuPage menu={menu} isLoading={isLoading} error={menuError}
          onRetry={loadMenu} onAdd={addToCart} />
      )}
      {page === 'cart' && (
        <CartPage items={cart} onQuantityChange={updateQuantity}
          onBack={() => setPage('menu')} onSuccess={handleSuccess} />
      )}
      {page === 'success' && order && (
        <OrderSuccess order={order} onBack={() => { setOrder(null); setPage('menu') }} />
      )}
    </div>
  )
}

export default App
