# ECommerce Platform (.NET 8)

Production-ready ASP.NET Core solution with Web API + Razor Pages UI, Identity + JWT, EF Core (SQLite dev / SQL Server prod), Stripe payments, ML.NET recommendations, Serilog logging, Swagger, Docker, and GitHub Actions.

## Tech Stack
- Backend: ASP.NET Core Web API (net8.0)
- UI: Razor Pages (Bootstrap)
- Auth: ASP.NET Core Identity + JWT
- Data: EF Core (SQLite dev, SQL Server prod)
- Payments: Stripe .NET SDK
- ML: ML.NET (content-based recommendations)
- Logging: Serilog
- Docs: Swagger/OpenAPI
- CI/CD: GitHub Actions
- Deploy: Azure App Service (Docker supported)

## Getting Started

### Prerequisites
- .NET SDK 8
- SQLite (optional for local, created automatically)
- Stripe test account (keys)

### Setup
1. Configure settings in `src/ECommerce.Api/appsettings.json`:
   - `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
   - `Stripe:ApiKey`, `Stripe:WebhookSecret`
   - `DatabaseProvider`: `Sqlite` (default) or `SqlServer`
2. Create database and seed sample data:
   - `dotnet tool install -g dotnet-ef`
   - `dotnet ef database update -p src/ECommerce.Infrastructure -s src/ECommerce.Api`
3. Run API + UI:
   - API: `dotnet run --project src/ECommerce.Api` (Swagger at `/swagger`)
   - UI: `dotnet run --project src/ECommerce.Web` (pages call API via same origin if reverse-proxied; or run behind a reverse proxy)

Default seeded admin: `admin@shop.local` / `Admin123!`

### Features
- Registration/Login (`/api/auth/register`, `/api/auth/login`) => returns JWT
- Products CRUD (admin-only for create/update/delete)
- Persistent Cart, Orders, Stripe PaymentIntent creation
- Session analytics middleware stores `SessionEvent`s
- Recommendations: content-based suggestions using product text similarity

### API Endpoints (examples)
- `GET /api/products?search=phone&page=1&pageSize=12`
- `GET /api/products/{id}`
- `POST /api/cart/add?productId={id}&quantity=1` (auth)
- `POST /api/orders?stripePaymentId=pi_123` (auth)
- `POST /api/payments/intent?amount=100&currency=usd` (auth)
- `POST /api/auth/register`, `POST /api/auth/login`

Swagger available at `/swagger`.

### Security
- HTTPS enforcement
- JWT bearer authentication
- Identity password policies
- Input validation by model binding and EF parameterization
- Rate limiting: add `AspNetCoreRateLimit` configuration as needed

### Performance
- Pagination for products
- Indices on cart uniqueness
- MemoryCache ready; add caching to product queries if needed

### Deployment (Azure App Service)
- Build and push Docker image:
  - `docker build -t yourrepo/ecommerce:latest .`
  - `docker push yourrepo/ecommerce:latest`
- Create Web App for Containers and point to the image
- Set environment variables (ConnectionStrings, Jwt, Stripe)

### CI/CD
- GitHub Actions workflow in `.github/workflows/ci.yml` builds, tests, and publishes artifacts on push/PR.

### Sample Data
- Seeding creates 3 categories, 50 products, admin user.

### Metrics & Admin
- Session events captured; extend with Chart.js in a Razor admin page.

### Lessons Learned
- Cached catalog endpoints improve p95 latency.
- Simplified content-based recommendations avoid heavy training and run fast.

### Roadmap / Bonus
- Wishlist, Reviews, Email notifications (SendGrid), full admin dashboard UI.

