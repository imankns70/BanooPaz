# WPF admin application

The WPF application gives admins a desktop interface for operating Kafgir.

The WPF app project lives at `backend/src/Kafgir.WPF` so it sits beside the other source projects.

The WPF app embeds the Vazir font from `backend/src/Kafgir.WPF/Fonts/Vazir` and applies it as the default app font for Persian UI readability.

The login screen uses a food-themed background image stored at `backend/src/Kafgir.WPF/Assets/login-food-background.png` with a soft overlay to keep the login card readable.

The WPF app configures `fa-IR` with `PersianCalendar` at startup and uses a custom Persian date picker for admin date inputs. The picker exposes Persian year, Persian month names, and Persian days while the API contracts continue to use normal `DateTime`/`DateOnly` values internally. Date values sent to backend routes and query strings are always serialized with invariant Gregorian `yyyy-MM-dd` formatting, so Persian display culture does not leak into API filters.

Initial pages:

- Dashboard
- Foods
- Daily Menu
- Orders
- Order Details
- Manual Order
- Settings

The WPF application communicates with the backend API and never accesses the database directly. Its structure follows MVVM and uses the shared backend contracts.

The Dashboard screen is implemented as the first tab after login. It loads `GET /api/admin/dashboard/today` and shows today's order totals, status counts, active orders, ordered portions, gross sales excluding cancelled orders, and today's menu state.

The Orders screen is implemented. Admins can load orders by date with the `جستجو` button, optionally filter by status, search the loaded result by order number, select an order, and view its customer information, items, totals, and status history. Row-level status actions call the backend API for `تایید`, `تحویل`, and `لغو`; `تحویل` is only shown after the order has passed confirmation. The older preparation/readiness buttons are not exposed in WPF. The list can poll the API every 10 seconds without starting another request while a load is active, and admins can disable `تازه‌سازی خودکار` with a switch toggle when selecting rows or changing status.

Order date filtering uses the BanooPaz business day in Iran time while order timestamps remain stored in UTC. The WPF Orders API client sends the selected date as Gregorian `yyyy-MM-dd` even when the UI date picker displays a Persian date. The WPF shell refreshes the Orders page whenever admins navigate back to it, so newly created manual orders are not hidden behind a stale list.

The API base URL is read from the WPF `appsettings.json`; services do not hardcode it. The default development value is `https://localhost:7279`, matching the API HTTPS launch profile.

Navigation now uses a right sidebar shell exposing Dashboard, Orders, Foods, and Daily Menu screens. Admins can manage food identity details and active status from WPF. The Foods page does not expose selling price because price belongs to the selected daily menu. Admins can search daily menus by date, view the selected date's menu items, and manage each food's daily price, portion capacity, and availability through explicit actions. All operations use backend HTTP APIs.

The sidebar also includes a manual order page (`ثبت سفارش`) for admins to create phone/in-person orders from today's active menu items. It posts to the protected admin order endpoint, so it does not require Telegram Mini App identity data. The food ComboBox displays the food name, price, and remaining capacity. Items with zero remaining capacity remain visible for clarity, but the admin cannot add them to an order until capacity is available.

The manual order page is organized as customer information, menu item selection, order lines, and total cards. It does not expose a menu date picker; manual orders use today's menu. Quantity is controlled with `+` and `−` buttons both before adding a food and inside the order-lines grid. Pressing `افزودن` for a food already in the order shows a message and does not increase the existing line; admins must use the row `+` and `−` controls to change quantity. The order-lines operation button is styled as a visible danger action.

Manual order entry does not show a city field. The app sends the default city (`اندیمشک`) internally. It defaults to pickup (`تحویل حضوری`) so admin-entered orders can be stored without an address; if delivery (`ارسال`) is selected, the address remains required.

Admin/manual order phone numbers are normalized before storage, including Persian/Arabic digits and common separators, so repeated customers are less likely to create duplicate or failing customer identities.

Daily menu item management is action-based. `افزودن` immediately creates one daily-menu item with its own daily price and capacity, `ویرایش` opens the same modal in edit mode for changing price, capacity, and active state, and `حذف` immediately removes one unbooked item. Items that are referenced by any customer order item cannot be removed, even if their confirmed sold portions are still zero; they should be disabled from the edit modal instead to preserve order history.

The Daily Menu list page keeps its top area intentionally small: Persian date picker, `جستجو`, and `افزودن`. `جستجو` loads the selected date so the same screen can be used for history lookup later. The grid below shows the daily menu items for the selected date, defaulting to today on first load.

The `افزودن` button opens a modal add-item form over the Daily Menu grid. The popup uses an aligned two-column form, shows the selected Persian date, and contains the food, `قیمت امروز`, and `ظرفیت` inputs. The `قیمت امروز` input applies thousands separators while typing instead of waiting for focus loss. In edit mode, the food selector is locked and the `فعال است؟` checkbox appears for changing the row's active state alongside daily price/capacity. After a successful add or edit, WPF closes the popup and leaves the stored menu item visible in the grid.

For safety, the legacy full-menu replacement API still rejects an empty item list for an existing menu that already has items. The WPF settings-save path does not call that replacement API.

Daily-menu settings save is guarded against repeated clicks. While one save is running, additional save clicks are ignored, and unexpected save failures are displayed as page error messages instead of crashing the WPF process.

After daily-menu settings save, the WPF page applies the stored menu returned by the API so the grid remains aligned with persisted item state.

The daily menu item list is shown in the labeled `آیتم‌های منوی روزانه` grid below the search bar. The grid includes a read-only `فعال` column and is read-only for price/capacity editing; item changes are handled through explicit action buttons. The add form accepts `قیمت امروز` and `ظرفیت`; the price field displays deterministic comma separators such as `150,000`.

Adding a food to the daily menu is immediate persistence, not a local draft. The WPF `افزودن به منو` action calls `POST /api/admin/daily-menus/by-date/{date}/items`, stores the new daily-menu item, and reloads the stored menu so the row has its database ID.

WPF admin authentication is performed through the backend API. The visual login screen includes a hide/show toggle for the password field. After successful login, the desktop app keeps the JWT in memory and attaches it to food, daily-menu, and order API requests with an `Authorization: Bearer ...` header. Orders polling starts only after login so the app does not call protected endpoints while unauthenticated.

When the WPF app opens, the login screen checks API reachability through the public `/api/health` endpoint. If the configured API base URL is unavailable, the login screen shows a connection message and a retry button instead of silently failing at login time. Login is re-enabled after the health check succeeds.

Development credentials are `admin` / `Admin@123456`. Identity hashes the password before database storage; plaintext passwords are never stored in the database. Replace development credentials and JWT configuration outside local development.

Payment method (`روش پرداخت`) is currently an enum in contracts (`Cash`, `CardToCard`, `Online`), not a database lookup table. `Online` exists as a method value only; no payment gateway integration has been implemented yet.

Daily-menu price entry fields use a thousands-separator converter so values such as `150,000` or Persian-digit equivalents can be displayed and saved correctly.
