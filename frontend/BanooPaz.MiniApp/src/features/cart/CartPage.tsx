import type { CartItem, OrderDto } from '../../types'
import { CartSummary } from './CartSummary'
import { CheckoutForm } from '../orders/CheckoutForm'

type Props = { items: CartItem[]; onQuantityChange: (id: number, quantity: number) => void; onBack: () => void; onSuccess: (order: OrderDto) => void }

export function CartPage({ items, onQuantityChange, onBack, onSuccess }: Props) {
  return <main>
    <div className="page-actions"><h1 className="section-title">ثبت سفارش</h1><button className="secondary-button" onClick={onBack}>بازگشت به منو</button></div>
    <CartSummary items={items} onQuantityChange={onQuantityChange} />
    <CheckoutForm items={items} onSuccess={onSuccess} />
  </main>
}
