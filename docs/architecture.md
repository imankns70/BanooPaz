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

Admin endpoints use the `/api/admin/...` route prefix. The WPF application will consume these HTTP APIs for food, daily-menu, and future order management; it does not access SQL Server directly.

Customer order submission uses `POST /api/orders`. Admin order listing, details, and status changes use `/api/admin/orders` routes. Both route groups call the Application order service; clients never access persistence directly.

`BanooPaz.Worker` remains reserved for background jobs such as notifications, reminders, menu closing, and daily reports.
