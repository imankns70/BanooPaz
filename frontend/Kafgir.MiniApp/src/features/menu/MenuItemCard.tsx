import type { DailyMenuItemDto } from '../../types'
import { formatMoney, toPersianDigits } from '../../utils/format'

export function MenuItemCard({ item, onAdd }: { item: DailyMenuItemDto; onAdd: (item: DailyMenuItemDto) => void }) {
  return <article className="menu-card">
    <div className="card-media">
      {item.imageUrl
        ? <img className="food-image" src={item.imageUrl} alt={item.foodName} />
        : <div className="food-placeholder"><span>کفگیر</span><small>غذای خانگی</small></div>}
      <span className="portion-badge">{toPersianDigits(item.remainingPortions)} پرس باقی‌مانده</span>
    </div>
    <div className="menu-card-body">
      <h3>{item.foodName}</h3>
      <p className="menu-card-description">{item.foodDescription || 'غذای خونگی تازه کفگیر'}</p>
      <div className="menu-card-meta">
        <span className="price">{formatMoney(item.price)}</span>
        <span className="muted">برای هر پرس</span>
      </div>
      <button className="primary-button full-width add-button" onClick={() => onAdd(item)}>
        <span>افزودن به سبد</span><span className="add-mark" aria-hidden="true">+</span>
      </button>
    </div>
  </article>
}
