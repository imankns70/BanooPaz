# BanooPaz

**BanooPaz (بانوپز)** is a homemade food ordering system for Andimeshk, created around food prepared with care and in memory of mother.

> بانوپز؛ غذای خونگی، با عشق

## Technology stack

- ASP.NET Core Web API, C#, EF Core, and SQL Server
- .NET Worker Service for future background work
- WPF admin application using MVVM
- React, TypeScript, and Vite Telegram Mini App
- xUnit tests

## Repository structure

- `backend/`: API, application, domain, contracts, infrastructure, worker, and tests
- `desktop/`: WPF admin application and tests
- `frontend/`: Telegram Mini App
- `docs/`: product and technical documentation
- `.ai/`: current state, decisions, tasks, and reusable prompts

## Build and run

```powershell
dotnet build .\backend\BanooPaz.sln
dotnet build .\desktop\BanooPaz.Desktop.sln
cd .\frontend\BanooPaz.MiniApp
npm install
npm run dev
```

Current status: **Initial structure**.
