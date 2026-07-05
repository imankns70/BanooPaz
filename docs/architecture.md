# Architecture

The backend API is the central integration point. Both customer and admin clients communicate with it. Only backend Infrastructure accesses SQL Server. The Worker is reserved for notifications, reminders, cleanup, and reports added later.

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
BanooPaz.Admin.Wpf

BanooPaz.Worker -> BanooPaz.Api/Infrastructure for future background tasks
```

The codebase follows Clean Architecture pragmatically: Domain contains business concepts, Application coordinates use cases, Contracts carries shared transport models, and Infrastructure implements external concerns.

Admin endpoints use the `/api/admin/...` route prefix and require a valid JWT for an admin role (`Owner`, `KitchenAdmin`, or `OrderManager`). The WPF application consumes these HTTP APIs for food, daily-menu, and order management; it does not access SQL Server directly.

Customer menu reading uses the public `GET /api/menus/today` endpoint. Customer order submission uses `POST /api/orders`. Admin order listing, details, and status changes use `/api/admin/orders` routes. Both route groups call Application services; clients never access persistence directly.

`BanooPaz.Worker` remains reserved for background jobs such as notifications, reminders, menu closing, and daily reports.

Users and roles are managed by ASP.NET Core Identity with integer keys. Identity implementation details live in Infrastructure; `CustomerProfile` and `CustomerAddress` remain business entities linked through `CustomerProfile.UserId`. WPF admin authentication is API-based through `/api/auth/admin/login` and JWT. The WPF login screen includes a password hide/show toggle, stores the JWT in memory after successful login, and sends it on protected admin API calls as an `Authorization: Bearer ...` header.

Telegram customer identity is established from server-validated Telegram `initData`. The Mini App sends the raw `Telegram.WebApp.initData` query string with order submissions; the backend verifies the HMAC-SHA-256 hash, checks `auth_date` freshness, and derives the Telegram user ID server-side before mapping to an Identity user. Local development can allow missing `initData` only when validation is not required. Orders store delivery snapshots so later edits to profiles or saved addresses cannot change historical orders.

Development JWT and Telegram bot-token values are placeholders. Production signing keys, bot tokens, and connection strings must be supplied with user secrets, environment variables, or a secure secret manager; `.env` files and secrets must not be committed. In production, `Telegram:BotToken` must be configured and `initData` is required by default outside the Development environment.
