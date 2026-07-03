# Database

The initial SQL Server schema is represented by the `InitialBanooPazSchema` EF Core migration. It includes these MVP tables:

- Customers
- CustomerAddresses
- Foods
- DailyMenus
- DailyMenuItems
- Orders
- OrderItems
- OrderStatusHistories
- Admins
- AppSettings

Daily menu dates are unique, and each food can appear only once per daily menu. Order items preserve food name and price snapshots. The four MVP foods are included as seed data. The migration has been generated but has not been applied to a database.

`DailyMenuItem.CapacityPortions` stores capacity as a number of portions. Daily-menu management APIs may change capacity only when it remains at least the existing `SoldPortions`; they never reduce or otherwise modify `SoldPortions`.
