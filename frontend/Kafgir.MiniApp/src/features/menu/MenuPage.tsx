import type { DailyMenuDto, DailyMenuItemDto } from '../../types'
import { MenuItemCard } from './MenuItemCard'

type Props = { menu: DailyMenuDto | null; isLoading: boolean; error: string | null; onRetry: () => void; onAdd: (item: DailyMenuItemDto) => void }

export function MenuPage({ menu, isLoading, error, onRetry, onAdd }: Props) {
  if (isLoading) return <div className="status-card">در حال دریافت منوی امروز…</div>
  if (error) return <div className="status-card error"><p>{error}</p><button className="secondary-button" onClick={onRetry}>تلاش دوباره</button></div>
  if (!menu) return <div className="status-card">امروز منویی ثبت نشده است.</div>
  if (!menu.isOpen) return <div className="status-card">سفارش‌گیری امروز بسته است.</div>

  const availableItems = menu.items.filter((item) => item.isAvailable && item.remainingPortions > 0)
  return <main>
    <section className="menu-intro">
      <div>
        <span className="eyebrow">پخت تازه امروز</span>
        <h1 className="section-title">منوی امروز</h1>
        <p className="section-subtitle">{menu.note || 'غذای تازه و خانگی در اندیمشک'}</p>
      </div>
    </section>
    {availableItems.length === 0
      ? <div className="status-card">غذای قابل سفارشی برای امروز باقی نمانده است.</div>
      : <div className="menu-grid">{availableItems.map((item) => <MenuItemCard key={item.id} item={item} onAdd={onAdd} />)}</div>}
  </main>
}
