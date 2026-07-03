# Project state

- Status: Food and daily menu admin APIs
- Brand: BanooPaz / بانوپز
- City: Andimeshk / اندیمشک
- Sales model: Per portion
- Backend: ASP.NET Core Web API, SQL Server, EF Core, and .NET Worker Service
- Admin: WPF desktop application communicating only with the API
- Customer: React + TypeScript + Vite Telegram Mini App
- Architecture: Pragmatic Clean Architecture with the API as the central integration point
- Current scope: Domain and persistence foundation plus admin-facing Food and Daily Menu APIs
- MVP foods: زرشک‌پلو با مرغ، قورمه‌سبزی، ماکارونی، قیمه
- MVP domain entities have been created for customers, addresses, foods, daily menus, orders, admins, and settings.
- EF Core persistence foundation and the `InitialBanooPazSchema` migration have been added.
- The four initial foods are seeded with fixed creation timestamps and placeholder prices.
- Food admin APIs have been implemented.
- Daily menu admin APIs have been implemented with additive item upserts.
