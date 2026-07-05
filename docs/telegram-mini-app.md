# Telegram Mini App

The Mini App is the customer-facing ordering experience.

## Customer flow

1. Open the Mini App.
2. View the daily menu.
3. Add foods to the cart.
4. Enter customer information and address.
5. Submit the order.
6. Wait for admin confirmation.

Telegram `initData` validation is implemented on the backend for order submission.

The first MVP customer UI is implemented:

- Customer can view today's menu.
- Customer can add food to cart and update quantities.
- Customer can enter delivery details and submit an order.
- The Mini App sends raw `Telegram.WebApp.initData` during checkout. The backend validates its HMAC hash and freshness before trusting Telegram user identity.
- Returning customers will later receive their saved profile and active address data after secure Telegram identity validation.
- Checkout will allow selecting a previous address or adding and optionally saving a new delivery destination.

Today's menu is loaded from the public backend endpoint `GET /api/menus/today`. Admin-only menu management remains under protected `/api/admin/daily-menus/...` routes.

## Backend configuration

Set `Telegram:BotToken` from BotFather in a secure configuration source. `Telegram:InitDataMaxAgeMinutes` controls freshness; the default is 1440 minutes. Missing `initData` is rejected outside the Development environment. In Development, missing `initData` can fall back to raw Mini App user fields only when `Telegram:RequireInitData` is `false`.
