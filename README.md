# Kafgir

**Kafgir (کفگیر)** is a homemade food ordering system for Andimeshk, created around food prepared with care and in memory of mother.

> کفگیر؛ غذای خونگی، با عشق

## Technology stack

- ASP.NET Core Web API, C#, EF Core, and SQL Server
- .NET Worker Service for future background work
- WPF admin application using MVVM
- React, TypeScript, and Vite Telegram Mini App
- xUnit tests

## Repository structure

- `backend/`: API, application, domain, contracts, infrastructure, worker, WPF admin app, and backend tests
- `frontend/`: Telegram Mini App
- `docs/`: product and technical documentation
- `.ai/`: current state, decisions, tasks, and reusable prompts

## Build and run

```powershell
dotnet build .\backend\Kafgir.sln
cd .\frontend\Kafgir.MiniApp
npm install
npm run dev
```

## SQL Server configuration

The backend projects load config in this order:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.local.json` if present
- `appsettings.Development.local.json` if present

That lets each machine keep its own SQL Server connection string without editing tracked files.

- Docker SQL machine: keep the existing `appsettings.Development.json` values.
- Non-Docker SQL machine: copy `backend/src/Kafgir.Api/appsettings.Development.local.example.json` to `backend/src/Kafgir.Api/appsettings.Development.local.json`, then do the same for `backend/src/Kafgir.Worker`, and set your local SQL Server connection string there.

Example local SQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=KafgirDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Current status: **Initial structure**.
