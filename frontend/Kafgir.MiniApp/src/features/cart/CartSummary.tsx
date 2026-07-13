import type { CartItem } from '../../types'
import { formatMoney, toPersianDigits } from '../../utils/format'

export function CartSummary({ items, onQuantityChange }: { items: CartItem[]; onQuantityChange: (id: number, quantity: number) => void }) {
  const total = items.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
  return <section className="panel">
    <h2 className="section-title">سبد خرید</h2>
    {items.length === 0 && <p className="muted">سبد خرید شما خالی است.</p>}
    {items.map((item) => <div className="cart-row" key={item.dailyMenuItemId}>
      <div><div className="cart-name">{item.foodName}</div><small className="muted">{formatMoney(item.unitPrice)} × {toPersianDigits(item.quantity)}</small></div>
      <div className="quantity-controls">
        <button type="button" className="quantity-button" onClick={() => onQuantityChange(item.dailyMenuItemId, item.quantity - 1)}>−</button>
        <strong>{toPersianDigits(item.quantity)}</strong>
        <button type="button" className="quantity-button" disabled={item.quantity >= item.remainingPortions}
          onClick={() => onQuantityChange(item.dailyMenuItemId, item.quantity + 1)}>+</button>
      </div>
      <span>{formatMoney(item.unitPrice * item.quantity)}</span>
    </div>)}
    <div className="cart-total"><span>جمع سفارش</span><span>{formatMoney(total)}</span></div>
  </section>
}
