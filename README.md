# Flower E-Commerce Microservices

Backend microservices solution for the Flowery flower e-commerce app.

This README is intentionally written as a quick handoff for humans and AI coding agents opening a new session in this repo.

## Current Working Context

- Main working branch for team integration: `Development`.
- Other important branches used recently: `master`, `Supra's-Playground`.
- Before changing code, always check `git status --short --branch` and preserve any user changes already in the working tree.
- Do not run or restart Docker containers unless the user explicitly asks. The user often runs containers locally to test.
- Keep `flowery-app-api.yaml` updated when API endpoints, request bodies, response bodies, auth, or status codes change. The Flutter team uses it in <https://editor.swagger.io/>.

## Solution Layout

- `Identity service` - authentication, registration, roles, JWT, driver applications, password reset, identity seeding.
- `Catalog Service` - products, categories, occasions, banners, stores/inventory where present, SDUI home layout, catalog seeding.
- `Cart Service` - shopping cart APIs.
- `Order & Fulfillment Service` - order and fulfillment APIs.
- `Payment Service` - payment APIs.
- `Address & Store Coverage Service` - address/store coverage APIs.
- `API Gateway` - gateway project.
- `Common` - shared response and utility code, including `OperationResult`.
- `Base.Repository` - repository/unit-of-work helper library used by services.
- `docs` - supporting documentation.
- `flowery-app-api.yaml` - OpenAPI contract shared with Flutter.

## Architecture Conventions

The solution follows Vertical Slice Architecture.

For new features, keep endpoint/controller, request/command/query, handler, validator, DTOs, and feature-specific logic together inside the service's `Features` folder. Avoid adding new centralized controller folders unless the existing service already uses that style for the exact area.

Use CQRS/MediatR where the service already follows it:

- Query records should implement `IRequest<OperationResult<T>>`.
- Command records should implement `IRequest<OperationResult<T>>` or the matching existing response pattern.
- Handlers should contain application flow and use injected dependencies.
- Carter endpoints should be thin: bind inputs, send the command/query, return `result.ToHttpResult()`.

Use the shared response pattern:

- `OperationResult` / `OperationResult<T>` from `Common/StandardizedResponse`.
- `OperationResultFactory.Success`, `Created`, `Validation`, `NotFound`, `Conflict`, etc.
- Return HTTP results through `ToHttpResult()` where available.

Use `IUnitOfWork<TDbContext>` and repository helpers from `Base.Repository` where the service has them registered. Prefer repository helper paging methods such as `GetPageSelectAsync(...)` instead of manual `.Skip((page - 1) * pageSize).Take(pageSize)` paging.

## API/Auth Notes

Swagger is configured per service with Bearer JWT support. In Swagger UI, paste the raw JWT token only; do not include the `Bearer` prefix if the service description says Swagger adds it.

JWT configuration lives in each service's appsettings when that service validates tokens:

- `Identity service/appsettings.json`
- `Identity service/appsettings.Development.json`
- `Catalog Service/appsettings.json`
- `Catalog Service/appsettings.Development.json`

Identity is the token issuer. Other services that require auth must use matching JWT issuer/audience/key settings.

Public catalog endpoints should remain anonymous unless the product requirement says otherwise. Recent intended public endpoints include categories, occasions, products, product details, and home layout.

Admin-only endpoints should keep role authorization, usually `Admin`.

## Server-Driven Home Screen

Catalog has a Server-Driven UI home layout feature.

Important contract decisions:

- `GET /home/layout` returns ordered sections from `HomeSection`.
- Each section has at least `type`, `id`, `title`, `order`, `enabled`, and type-specific config/payload from `ContentRefJson`.
- The home layout response should describe the page structure/config. Clients can then call the dedicated endpoints for products, categories, occasions, etc.
- Disabled or unknown section types should be safe for clients to skip.
- Keep `flowery-app-api.yaml` aligned with this contract.

## Database Initialization And Seeding

Services run migrations/seeding at startup through their database initialization extensions.

Reset switches are intentionally defaulted to `false`:

- appsettings key: `DatabaseInitialization:ResetOnStartup`
- docker compose override env vars:
  - `ADDRESS_RESET_DATABASE_ON_STARTUP`
  - `CART_RESET_DATABASE_ON_STARTUP`
  - `CATALOG_RESET_DATABASE_ON_STARTUP`
  - `IDENTITY_RESET_DATABASE_ON_STARTUP`
  - `ORDER_RESET_DATABASE_ON_STARTUP`
  - `PAYMENT_RESET_DATABASE_ON_STARTUP`

Set the relevant variable to `true` only when you explicitly want a service database dropped/recreated on startup.

Seed data locations:

- Identity seed config: `Identity service/appsettings.json` and `Identity service/appsettings.Development.json` under `Seed`.
- Identity seeder: `Identity service/Infrastructure/IdentityDataSeeder.cs`.
- Catalog seeder: `Catalog Service/Persistence/CatalogDataSeeder.cs`.
- Catalog product seed data: `Catalog Service/Persistence/CatalogProductSeedData.cs`.

If SQL Server says a database or table already exists during startup, check the database initialization logic first. The desired behavior is usually: migrate if needed, seed idempotently, and reset only when the reset key is explicitly true.

## Build And Verification

Build the full solution with:

```powershell
dotnet build "Flower_ECommerce_Microservices-team1.slnx"
```

Useful checks before finishing API work:

```powershell
rg -n "<<<<<<<|=======|>>>>>>>" .
rg -n "Skip\(|Take\(" "Catalog Service\Features\Products"
rg -n "OperationResult|ToHttpResult" "Catalog Service" "Identity service"
```

Do not assume Docker runtime behavior was verified unless containers were actually run. If the user says they will run containers, only build/check code locally.

## OpenAPI Contract

`flowery-app-api.yaml` should be treated as a deliverable. Update it when backend endpoints change.

The YAML should include:

- route path and method
- auth requirement or anonymous access
- query/path parameters
- request body schema
- `OperationResult` response wrapper shape
- success and important error responses

Keep examples practical for Flutter testing.

## Coding Style Notes

- Keep changes scoped to the requested service/feature.
- Avoid unrelated refactors.
- Prefer existing patterns in the target service over inventing new abstractions.
- Use ASCII in files unless the file already uses non-ASCII or there is a clear reason.
- Do not revert user changes unless explicitly asked.
- For Swagger file upload endpoints, prefer real multipart/form-data schemas so Swagger shows file pickers instead of plain text boxes.