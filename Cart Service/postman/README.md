# Cart Service — manual test pack

Postman collection covering SCRUM-11 (Add to Cart), SCRUM-22 (Update Quantity), SCRUM-25 (Remove Item)
and SCRUM-29 (Cart Summary).

| File | What it is |
|---|---|
| `Flower-Cart.postman_collection.json` | 24 requests in 7 folders, each with assertions |
| `Flower-Cart.postman_environment.json` | URLs, demo ids, and ready-to-use dev tokens |
| `cart-demo-data.sql` | Stock rows the cart needs, plus the price-change and reset snippets |
| `mint-test-token.ps1` | Regenerates a token when the bundled one expires |

---

## Setup

### 1. Import into Postman

`Import` → drop both `.json` files in → pick **Flower — Cart (local)** from the environment
dropdown at the top right.

### 2. Start the infrastructure

```bash
docker compose up -d sqlserver
```

### 3. Seed the stock

Nothing in the Catalog is in stock out of the box — `ProductStoreInventories` ships empty, so every
product reports `inStock: false` and the cart cannot be exercised at all. Run section 1 of the SQL:

```bash
docker exec dockercompose18376578966446921489-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "Flower_Dev_2026!" -C -I -i "Cart Service/postman/cart-demo-data.sql"
```

The `-I` flag is required: the Catalog schema uses filtered indexes, which need `QUOTED_IDENTIFIER ON`,
and `sqlcmd` defaults it off. EF and SqlClient set it automatically, which is why the app never hits this.

### 4. Start the two services

Run **Catalog Service** and **Cart Service** on the `http` profile. Catalog must be up before the
cart can price anything.

> If the `cartservice` / `catalogservice` containers are running they already hold ports 5292 and
> 5129. Stop those two containers, or change `cartUrl` / `catalogUrl` in the Postman environment to
> whatever ports you launch on.

### 5. Sanity check

Run folder **00 Setup**. All three requests must pass before anything else is meaningful — the last
assertion in the second request is exactly "is this product in stock", which is the usual reason a
first run fails.

---

## Demo data

| | Id | Notes |
|---|---|---|
| Classic Red Roses | `40000000-…-0001` | 499.00, **10 units** at Nasr City |
| Sunrise Birthday Bouquet | `40000000-…-0002` | 650.00, **4 units** at Nasr City |
| Peace Lily Plant | `40000000-…-0003` | 720.00, 6 units at **Maadi only** |
| Nasr City Branch | `60000000-…-0001` | the default `storeId` |
| Maadi Branch | `60000000-…-0002` | `storeIdMaadi` |

The three different stock levels are deliberate: 10 gives room to move quantities around, 4 makes the
409 easy to trigger, and a product stocked at a *different* branch proves `storeId` is really driving
the decision rather than being decoration.

---

## Running the tests

Folders are ordered and chain through collection variables, so run each folder top to bottom.
Use Postman's **Runner** for a folder at a time rather than the whole collection in one go — folders
05 and 06 need you to do something in between requests.

| Folder | What it proves |
|---|---|
| **00 Setup** | Environment is actually ready |
| **01 Happy path** | Add → increase → reprice → set quantity → remove |
| **02 Stock limits** | 409 with the real ceiling, 404s, 422s |
| **03 Authorization** | 401 without a token, 403 for an Admin |
| **04 Multiple lines** | Two lines, correct totals, removal down to empty |
| **05 Price indicator** | The core of SCRUM-29 — needs a manual SQL step |
| **06 Catalog outage** | Cart degrades instead of failing |

**Reset between runs** (folders assume they start from an empty cart):

```sql
USE FlowerCartDb;
DELETE FROM CartItems;
DELETE FROM Carts;
```

### Folder 05 — the price change indicator

This is the one worth demoing, and it needs three steps:

1. Send request **20** (adds an item, capturing the price snapshot).
2. Run the `UPDATE Products SET Price = 555.00` from section 2 of `cart-demo-data.sql`.
3. Send request **21** — the cart returns the **new** price with `priceChanged: true` and
   `hasChanges: true`.
4. Restore `Price = 499.00`, then send request **22** to watch the flags clear.

### Folder 06 — Catalog outage

1. With the Catalog still up, make sure the cart has at least one item (request 20).
2. Stop the Catalog Service.
3. Request **23** must return **200** with `pricingUnavailable: true` and the stored snapshot —
   not a 500.
4. Request **24** must return **503**, because adding genuinely cannot proceed without a live price.
5. Restart the Catalog Service.

---

## Tokens

The bundled tokens are Customer and Admin JWTs signed with the development key already committed in
`appsettings.json`. They expire **2026-11-25**.

They are hand-minted rather than obtained from `/auth/login` because the Identity service has no
customer registration endpoint yet, and its seeder currently fails with SQL error 2714, so the seeded
customer accounts never reach the database. Once either is fixed, log in normally and paste the real
token into `customerToken` instead.

To regenerate:

```bash
pwsh "Cart Service/postman/mint-test-token.ps1"
```

These are dev-only credentials for a local database. Never point this environment at anything real,
and rotate the signing key before any deployment — it has been in git history for a while.

---

## Through the gateway

The environment also carries `gatewayCartUrl` (`http://localhost:5086/api/v1/cart`). The gateway
strips the `/cart` prefix, so the same routes apply. Swapping `cartUrl` for `gatewayCartUrl` runs the
whole pack through YARP instead of hitting the service directly.

---

## Not covered, by design

`deliveryFee` always returns `null` and there is no undo-after-delete. Both were deferred as a
decision, not missed — the delivery fee belongs to Order & Fulfillment. Say so in the PR so nobody
reads the ACs as fully implemented.
