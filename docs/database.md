# Database

The initial SQL Server schema is represented by `InitialKafgirSchema`, followed by `AddIdentityCustomerProfilesAndAddresses`. The current model includes:

- ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, claims, logins, tokens, and user roles)
- TelegramAccounts
- CustomerProfiles
- CustomerAddresses
- Foods
- DailyMenus
- DailyMenuItems
- Orders
- OrderItems
- OrderStatusHistories
- NotificationMessages
- AppSettings

Daily menu dates are unique, and each food can appear only once per daily menu. Order items preserve food name and price snapshots. The four MVP foods are included as seed data. The migration has been generated but has not been applied to a database.

`DailyMenuItem.CapacityPortions` stores capacity as a number of portions. Daily-menu management APIs may change capacity only when it remains at least the existing `SoldPortions`; they never reduce or otherwise modify `SoldPortions`. Order submission does not reserve capacity. Confirming an order increases `SoldPortions`, and cancelling a confirmed, preparing, or ready order restores its portions.

`OrderItem` stores snapshot values for food name, unit price, quantity, and total price so later food or menu changes do not alter historical orders.

`CustomerProfiles` stores customer-specific business data and has a unique `UserId` link to Identity. `CustomerAddresses` stores multiple reusable, soft-disabled delivery addresses. Orders reference a profile and optionally a saved address, but also persist delivery name, phone, city, address line, and description snapshots.

`TelegramAccounts` stores Telegram-specific user and chat metadata, including `TelegramUserId`, username/name fields, `ChatId`, and last-seen timestamps. It has a unique one-to-one `UserId` link to `AspNetUsers` and a unique `TelegramUserId`. The migration backfills the table from existing Identity Telegram columns so older users keep their Telegram linkage.

The Identity migration preserves legacy customer IDs and copies existing customer/order/address data before removing the old custom Customers table. The old passwordless Admins table is replaced by an Identity admin created through the development seeder.

`NotificationMessages` is a durable outbox for Telegram notifications. Order submission and order status changes enqueue messages in the same database save as the business change. The Worker reads pending messages, sends them through Telegram, stores attempt timestamps/errors, and marks messages as `Sent` or `Failed` after the configured retry limit.
