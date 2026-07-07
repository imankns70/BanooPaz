# Architecture

The backend API is the central integration point. Both customer and admin clients communicate with it. Only backend Infrastructure accesses SQL Server. The Worker processes background notification delivery and remains the place for future reminders, cleanup, and reports.

```text
Telegram Mini App
        |
        v
BanooPaz.Api
        |
        v
SQL Server
        ^
        |
BanooPaz.WPF

BanooPaz.Worker -> SQL Server outbox -> Telegram Bot API
```

The codebase follows Clean Architecture pragmatically: Domain contains business concepts, Application coordinates use cases, Contracts carries shared transport models, and Infrastructure implements external concerns.

Swagger UI is enabled for the API in Development at `/swagger`. It documents the v1 API surface and includes JWT Bearer authorization support so protected admin endpoints can be tested after logging in through `/api/auth/admin/login`.

Admin endpoints use the `/api/admin/...` route prefix and require a valid JWT for an admin role (`Owner`, `KitchenAdmin`, or `OrderManager`). The WPF application consumes these HTTP APIs for dashboard, food, daily-menu, and order management; it does not access SQL Server directly.

Customer menu reading uses the public `GET /api/menus/today` endpoint. Returning customer preload uses `POST /api/customers/me`, validates Telegram identity, and returns the saved profile plus active addresses when present. Customer order submission uses `POST /api/orders`. Admin dashboard summary uses `GET /api/admin/dashboard/today`; admin manual order creation uses `POST /api/admin/orders`; and admin order listing, details, and status changes use `/api/admin/orders` routes. All route groups call Application services; clients never access persistence directly.

`BanooPaz.Worker` processes pending `NotificationMessages` from the database outbox. Order submission enqueues an admin Telegram notification when `Telegram:AdminChatId` is configured. Order status changes enqueue customer Telegram notifications when the customer has a Telegram user ID. The Worker sends messages through Telegram Bot API `sendMessage`, marks successful messages as sent, and retries failures with backoff until the configured retry limit is reached.

Users and roles are managed by ASP.NET Core Identity with integer keys. Identity implementation details live in Infrastructure; `CustomerProfile` and `CustomerAddress` remain business entities linked through `CustomerProfile.UserId`. WPF admin authentication is API-based through `/api/auth/admin/login` and JWT. The WPF login screen includes a password hide/show toggle, stores the JWT in memory after successful login, and sends it on protected admin API calls as an `Authorization: Bearer ...` header.

Telegram customer identity is established from server-validated Telegram `initData`. The Mini App sends the raw `Telegram.WebApp.initData` query string with profile preload and order submissions; the backend verifies the HMAC-SHA-256 hash, checks `auth_date` freshness, and derives the Telegram user ID server-side before reading or mapping to an Identity user. Telegram-specific user/chat metadata is stored in `TelegramAccounts`, linked one-to-one to Identity users. Local development can allow missing `initData` only when validation is not required. Orders store delivery snapshots so later edits to profiles or saved addresses cannot change historical orders.

Development JWT and Telegram bot-token values are placeholders. Production signing keys, bot tokens, admin chat IDs, and connection strings must be supplied with user secrets, environment variables, or a secure secret manager; `.env` files and secrets must not be committed. In production, `Telegram:BotToken` must be configured and `initData` is required by default outside the Development environment.
