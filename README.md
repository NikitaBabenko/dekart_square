# DecisionHelper — Cartesian Square with AI

Decision-helper service that walks users through the classical "Cartesian Square" reflection method (4 questions) with an LLM filling in / synthesizing the quadrants.

One deployment serves three entry points:

- **Telegram bot** — main audience.
- **Telegram Mini App (TMA)** — richer UX inside Telegram.
- **Public web** — landing + try-it-now (no payment, anonymous by cookie).

## Stack

- .NET 9 + Blazor Web App (SSR + Interactive Server) + Tailwind (via CDN for now)
- PostgreSQL 16 + EF Core (migrations applied on startup)
- Telegram.Bot 22 (webhook by default; long-polling toggle for dev)
- OpenRouter via `HttpClient` (model configured via env)
- Docker (one image, one container)

## Quickstart (local with Docker)

```bash
cp .env.example .env
# fill: TELEGRAM_BOT_TOKEN, OPENROUTER_API_KEY, OPENROUTER_MODEL,
#       APP_BASE_URL, POSTGRES_PASSWORD, etc.

docker compose up --build -d

curl http://localhost:8080/healthz   # → {"ok":true}
open http://localhost:8080/          # landing
open http://localhost:8080/app       # square editor
```

For the Telegram bot to receive webhooks, `APP_BASE_URL` must be a public HTTPS URL (use ngrok / Cloudflare Tunnel during dev). Or set `TELEGRAM_USE_POLLING=true` in `.env` to use long polling locally — note that this scaffold auto-registers a webhook on startup; switch the flag to disable that.

## Quickstart (without Docker)

```bash
dotnet build
dotnet test
dotnet run --project src/DecisionHelper.Web
```

You still need a running Postgres reachable via `POSTGRES_*` env vars (or change `BuildPgConnectionString` to point at `localhost`).

## Limits

- Free: 3 / day **and** 12 / month
- Premium: 500 / month, no daily cap
- Reset: calendar, UTC (`yyyy-MM-dd` daily, `yyyy-MM` monthly)

## Payments (Telegram Stars)

- `/premium` in the bot sends an invoice with `currency=XTR` for `STARS_PRICE` stars / `PREMIUM_DAYS` days.
- `pre_checkout_query` is auto-acknowledged. On `successful_payment` we record the charge id and grant premium.
- The web has no payment flow yet — premium is sold only inside Telegram.

## Project layout

```
src/DecisionHelper.Core/             # Domain, AI client, limits, locales
src/DecisionHelper.Infrastructure/   # EF Core, migrations, repositories
src/DecisionHelper.Web/              # Blazor Web App + bot webhook + endpoints
tests/DecisionHelper.Tests/          # xUnit
```

## Secrets

`.env` is gitignored. `.env.example` is the template. On a server, set env vars via systemd unit or `docker run -e`. Never commit a populated `.env`.
