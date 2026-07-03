# Instructions for coding agents

- Keep the architecture simple and avoid over-engineering.
- Respect project and folder boundaries.
- The Domain project must not depend on Infrastructure, API, desktop, or Telegram concerns.
- The WPF application must call the backend API and must never connect directly to SQL Server.
- Add packages only when a concrete requirement needs them.
- Update `.ai/PROJECT_STATE.md`, `.ai/DECISIONS.md`, and `.ai/TASKS.md` after meaningful changes.
- Do not implement authentication, Telegram integration, or payments until explicitly requested.
- Do not add database migrations until the MVP data model is approved.
