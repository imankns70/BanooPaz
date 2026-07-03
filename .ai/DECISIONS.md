# Decisions

- The brand name is BanooPaz / بانوپز and the product is built in memory of mother.
- SQL Server is selected as the database.
- WPF is selected for the admin application.
- A Telegram Mini App is selected for the customer application.
- The backend API is the central integration point.
- WPF must not connect directly to SQL Server.
- Initial sales are per portion in Andimeshk.
- Initial foods are:
  - زرشک‌پلو با مرغ
  - قورمه‌سبزی
  - ماکارونی
  - قیمه
- Domain enums are stored as integers for now.
- Order items snapshot food name, unit price, and total price so historical orders remain stable.
- Daily menu date is unique.
- A food can appear only once in a given daily menu.
