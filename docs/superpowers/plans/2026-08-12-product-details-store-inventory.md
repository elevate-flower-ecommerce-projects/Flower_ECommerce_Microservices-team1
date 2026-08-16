# Product Details and Store Inventory Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans or superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** Add normalized product images, included items, and store inventory, then expose store-aware product details.

**Architecture:** `Product` remains the catalog root. Images, included items, and store inventory are child entities keyed by `ProductId`; `StoreId` is an external GUID owned by Address & Store Coverage Service. The details endpoint reads all catalog detail data and resolves availability from inventory without cross-service database foreign keys.

**Tech Stack:** .NET 10, EF Core, Carter, MediatR, SQL Server.

## Global Constraints

- Do not create, edit, or execute EF Core migrations.
- Do not edit test projects.
- Exclude optional `ReservedQuantity`, store-price overrides, and image alt text.

---

### Task 1: Normalize product detail and inventory data

**Files:**
- Modify: `Catalog Service/Entities/Product.cs`
- Create: `Catalog Service/Entities/ProductImage.cs`
- Create: `Catalog Service/Entities/ProductIncludedItem.cs`
- Create: `Catalog Service/Entities/ProductStoreInventory.cs`
- Modify: `Catalog Service/Persistence/CatalogDbContext.cs`
- Create: `Catalog Service/Persistence/EntitiesConfiguration/ProductImageConfiguration.cs`
- Create: `Catalog Service/Persistence/EntitiesConfiguration/ProductIncludedItemConfiguration.cs`
- Create: `Catalog Service/Persistence/EntitiesConfiguration/ProductStoreInventoryConfiguration.cs`

- [ ] Add child collections to `Product`, description, child entities, EF mappings, and a unique index on `(ProductId, StoreId)`.

### Task 2: Seed normalized product details

**Files:**
- Modify: `Catalog Service/Persistence/CatalogDataSeeder.cs`
- Modify: `Catalog Service/Persistence/CatalogProductSeedData.cs`

- [ ] Seed two ordered images, two included items, and one inventory row per supported store for each deterministic product ID, inserting only rows that do not already exist.

### Task 3: Implement store-aware product details

**Files:**
- Create: `Catalog Service/Contracts/Products/ProductDetailResponse.cs`
- Create: `Catalog Service/Features/Products/GetProductDetailsQuery.cs`
- Create: `Catalog Service/Features/Products/GetProductDetailsHandler.cs`
- Modify: `Catalog Service/Features/Products/ProductsEndpoint.cs`
- Modify: `Catalog Service/Features/Products/GetProductsHandler.cs`
- Modify: `Catalog Service/Features/Home/GetHomeLayoutHandler.cs`

- [ ] Add `GET /products/{id}?storeId=`. Return `requiresStoreSelection: true` and unavailable stock when the store is absent. Resolve supplied-store availability from `ProductStoreInventory`, and migrate existing list/home store filtering to the normalized inventory relation.
