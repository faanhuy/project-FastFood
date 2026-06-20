# FastFood

A full-stack food ordering & e-commerce platform built with .NET 8 Clean Architecture and React 19, featuring AI-powered search, loyalty points, flash sales, and bilingual (VI/EN) UI.

![CI](https://github.com/faanhuy/project-FastFood/actions/workflows/ci.yml/badge.svg)
![CD](https://github.com/faanhuy/project-FastFood/actions/workflows/cd.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-19-blue)
![Tests](https://img.shields.io/badge/tests-65%2B-green)

## Features

**Core Commerce**
- **Auth**: JWT Bearer + Refresh Token rotation, Google OAuth
- **Products**: CRUD, Redis cache, slug routing, size management, soft delete
- **Cart**: Add/update/remove with real-time stock validation
- **Orders**: Atomic checkout — stock reduction + cart clear in one transaction
- **Payment**: VNPay HMAC integration (full & partial refunds), COD

**Advanced**
- **Flash Sales**: Time-boxed events with per-item stock limits and per-user purchase caps
- **Combos**: Bundle products with independent pricing
- **Coupons**: Usage-limited discount codes with minimum order validation
- **Price Campaigns**: Scheduled promotional pricing
- **Loyalty Points**: Earn on purchase, redeem for discounts
- **Returns**: Full return/refund workflow — VNPay partial refund support

**Admin**
- **Dashboard**: Revenue & top-product analytics (Recharts, CSV/PDF export)
- **Bulk actions**, CSV product import
- **Store inventory management** across multiple locations

**UX & Infrastructure**
- **AI Search**: Semantic product search (Voyage AI embeddings)
- **AI Recommendations**: Similar products via Claude API
- **AI Description**: Auto-generate product descriptions (Groq llama-3.3-70b)
- **Real-time notifications**: SignalR
- **Bilingual UI**: VI/EN via react-i18next + DB-driven backend messages
- **PWA**: Installable on mobile/desktop
- **Rate limiting**: Redis + Lua atomic scripts

## Tech Stack

| Layer       | Technology                                                          |
|-------------|---------------------------------------------------------------------|
| Backend     | .NET 8, ASP.NET Core, Clean Architecture, CQRS (MediatR)           |
| ORM         | EF Core 8, SQL Server 2022, Optimistic Concurrency (RowVersion)     |
| Cache       | Redis 7                                                             |
| Frontend    | React 19, TypeScript, Vite, TailwindCSS, Zustand, SignalR          |
| Auth        | JWT Bearer + Refresh Token Rotation, Google OAuth                   |
| Payment     | VNPay (HMAC signature, sandbox & production)                        |
| AI          | Groq (llama-3.3-70b), Voyage AI (voyage-3), Claude API             |
| Testing     | xUnit, Moq, FluentAssertions, Coverlet — 65+ unit tests            |
| CI/CD       | GitHub Actions, Docker multi-stage build, GHCR                     |

## Quick Start

### Run with Docker Compose

**Prerequisites**: Docker Desktop, Git

```bash
git clone https://github.com/faanhuy/project-FastFood.git
cd project-FastFood

cp .env.example .env
# Edit .env — add your API keys

docker compose up -d
# API + Swagger: http://localhost:8080/swagger
# Frontend:      http://localhost:3000
```

### Run Locally

**Prerequisites**: .NET 8 SDK, Node.js 22, SQL Server, Redis

```bash
# Backend
dotnet run --project src/SmartShop.WebAPI
# API + Swagger: http://localhost:5284/swagger

# Frontend
cd smartshop-frontend
npm install && npm run dev
# http://localhost:5173
```

## Environment Variables

| Variable            | Description                              |
|---------------------|------------------------------------------|
| `JWT_KEY`           | JWT signing key (min 32 characters)      |
| `GROQ_API_KEY`      | Groq API key for AI features             |
| `SA_PASSWORD`       | SQL Server SA password (Docker only)     |
| `VNPAY_TMN_CODE`    | VNPay merchant code                      |
| `VNPAY_HASH_SECRET` | VNPay HMAC secret                        |

See `.env.example` for full list.

## Architecture

```
├── src/
│   ├── SmartShop.Domain/         # Entities, value objects, interfaces
│   ├── SmartShop.Application/    # CQRS handlers, DTOs, validators
│   ├── SmartShop.Infrastructure/ # EF Core, Redis, JWT, AI services
│   └── SmartShop.WebAPI/         # Controllers, middleware, SignalR hubs
├── tests/
│   ├── SmartShop.Application.Tests/   # 65+ unit tests
│   └── SmartShop.Domain.Tests/
└── smartshop-frontend/                # React 19 + TypeScript + Vite
```

**Patterns**: Clean Architecture, CQRS (MediatR), Repository, Factory (`Entity.Create()`), Soft Delete, Audit Trail, Optimistic Concurrency

## API Overview (30+ controllers)

| Domain | Key Endpoints |
|--------|--------------|
| Auth | Register, Login, Refresh, Google OAuth |
| Products | CRUD, images, sizes, bulk CSV import |
| Cart | Add/update/remove, combo support |
| Orders | Place, cancel, timeline, history |
| Payment | VNPay init, callback, refund |
| Flash Sales | List, detail, admin CRUD |
| Loyalty | Points balance, history, redeem |
| Returns | Create, approve/reject, refund |
| Admin | Users, revenue analytics, bulk actions |
| AI | Semantic search, recommendations, description gen |
| ... | Coupons, Combos, Wishlist, Reviews, Notifications, Stores, Geography |

## Running Tests

```bash
dotnet test tests/SmartShop.Application.Tests/ --collect:"XPlat Code Coverage"
```

65+ unit tests across Auth, Products, Cart, Orders, Payments, Notifications, and more.

## CI/CD

- **CI** (`.github/workflows/ci.yml`): Push/PR → dotnet test, frontend build, Docker build check
- **CD** (`.github/workflows/cd.yml`): Push to master → Docker multi-stage build → GHCR push
