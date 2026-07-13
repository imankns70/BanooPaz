import type { DailyMenuItemDto } from '../../types'
import { formatMoney, toPersianDigits } from '../../utils/format'

export function MenuItemCard({ item, onAdd }: { item: DailyMenuItemDto; onAdd: (item: DailyMenuItemDto) => void }) {
  return <article className="menu-card">
    {item.imageUrl
      ? <img className="food-image" src={item.imageUrl} alt={item.foodName} />
      : <div className="food-placeholder">غذای خانگی</div>}
    <div className="menu-card-body">
      <h3>{item.foodName}</h3>
      <p className="menu-card-description">{item.foodDescription || 'غذای خونگی تازه کفگیر'}</p>
      <div className="menu-card-meta">
        <span className="price">{formatMoney(item.price)}</span>
        <span className="muted">باقی‌مانده: {toPersianDigits(item.remainingPortions)} پرس</span>
      </div>
      <button className="primary-button full-width" onClick={() => onAdd(item)}>افزودن به سبد</button>
    </div>
  </article>
}
