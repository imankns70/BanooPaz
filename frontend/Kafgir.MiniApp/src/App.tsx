import { useEffect, useState } from 'react'
import './App.css'
import { CartPage } from './features/cart/CartPage'
import { MenuPage } from './features/menu/MenuPage'
import { OrderSuccess } from './features/orders/OrderSuccess'
import { getTodayMenu } from './services/menuApi'
import type { CartItem, DailyMenuDto, OrderDto } from './types'

type Page = 'menu' | 'cart' | 'success'

function App() {
  const [page, setPage] = useState<Page>('menu')
  const [menu, setMenu] = useState<DailyMenuDto | null>(null)
  const [cart, setCart] = useState<CartItem[]>([])
  const [order, setOrder] = useState<OrderDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [menuError, setMenuError] = useState<string | null>(null)

  const loadMenu = async () => {
    setIsLoading(true)
    setMenuError(null)
    try {
      setMenu(await getTodayMenu())
    } catch (error) {
      setMenuError(error instanceof Error ? error.message : 'دریافت منوی امروز ناموفق بود.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => { void loadMenu() }, [])

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
        <div>
          <p className="brand">کفگیر</p>
          <p className="tagline">کفگیر؛ غذای خونگی، با عشق</p>
        </div>
        {page === 'menu' && (
          <button className="cart-button" onClick={() => setPage('cart')}>
            سبد خرید <span>{cart.reduce((sum, item) => sum + item.quantity, 0)}</span>
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
