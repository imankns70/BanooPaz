# WPF admin application

The WPF application gives admins a desktop interface for operating BanooPaz.

Initial pages:

- Dashboard
- Foods
- Daily Menu
- Orders
- Order Details
- Settings

The WPF application communicates with the backend API and never accesses the database directly. Its structure follows MVVM and uses the shared backend contracts.

The first Orders screen is implemented. Admins can load orders by date, optionally filter by status, select an order, and view its customer information, items, totals, and status history. Status actions call the backend API for confirmation, preparation, readiness, delivery, and cancellation. The list also polls the API every 10 seconds without starting another request while a load is active.

The API base URL is read from the WPF `appsettings.json`; order services do not hardcode it. The default development value is `https://localhost:5001`.

Navigation now exposes Orders, Foods, and Daily Menu screens. Admins can manage food details and active status from WPF. They can also load, create, or update a daily menu, set its open state, and configure each food's daily price, portion capacity, and availability. All operations use backend HTTP APIs.
