/*
    Demo data for manually testing the Cart Service (SCRUM-11 / 22 / 25 / 29).

    Run with sqlcmd against the SQL Server container. The -I flag is required because the
    Catalog schema uses filtered indexes, which need QUOTED_IDENTIFIER ON:

        docker exec <sqlserver-container> /opt/mssql-tools18/bin/sqlcmd \
            -S 127.0.0.1 -U sa -P 'Flower_Dev_2026!' -C -I -i cart-demo-data.sql

    Or paste each section into SSMS / Azure Data Studio.
*/

------------------------------------------------------------------------------
-- 1. Stock. Without these rows every product reports inStock:false and the
--    cart can never be exercised. Two products with deliberately different
--    ceilings so the 409 path is easy to hit.
------------------------------------------------------------------------------
USE FlowerCatalogDb;
GO

-- Classic Red Roses @ Nasr City Branch -> 10 units
IF NOT EXISTS (SELECT 1 FROM ProductStoreInventories
               WHERE ProductId = '40000000-0000-0000-0000-000000000001'
                 AND StoreId   = '60000000-0000-0000-0000-000000000001')
    INSERT INTO ProductStoreInventories (Id, ProductId, StoreId, AvailableQuantity, IsEnabled)
    VALUES (NEWID(), '40000000-0000-0000-0000-000000000001',
                     '60000000-0000-0000-0000-000000000001', 10, 1);

-- Sunrise Birthday Bouquet @ Nasr City Branch -> 4 units (low, for the 409 test)
IF NOT EXISTS (SELECT 1 FROM ProductStoreInventories
               WHERE ProductId = '40000000-0000-0000-0000-000000000002'
                 AND StoreId   = '60000000-0000-0000-0000-000000000001')
    INSERT INTO ProductStoreInventories (Id, ProductId, StoreId, AvailableQuantity, IsEnabled)
    VALUES (NEWID(), '40000000-0000-0000-0000-000000000002',
                     '60000000-0000-0000-0000-000000000001', 4, 1);

-- Peace Lily Plant @ Maadi Branch -> 6 units (proves storeId actually matters:
-- this product is NOT stocked at Nasr City, so asking for it there returns 409)
IF NOT EXISTS (SELECT 1 FROM ProductStoreInventories
               WHERE ProductId = '40000000-0000-0000-0000-000000000003'
                 AND StoreId   = '60000000-0000-0000-0000-000000000002')
    INSERT INTO ProductStoreInventories (Id, ProductId, StoreId, AvailableQuantity, IsEnabled)
    VALUES (NEWID(), '40000000-0000-0000-0000-000000000003',
                     '60000000-0000-0000-0000-000000000002', 6, 1);

SELECT p.Name, i.StoreId, i.AvailableQuantity, i.IsEnabled
FROM ProductStoreInventories i
JOIN Products p ON p.Id = i.ProductId;
GO


------------------------------------------------------------------------------
-- 2. Price change, for the SCRUM-29 "prices updated" indicator.
--    Add the item to the cart FIRST, then run this, then GET the cart again.
--    Expected: priceChanged = true on the line, hasChanges = true on the cart.
------------------------------------------------------------------------------
-- USE FlowerCatalogDb;
-- UPDATE Products SET Price = 555.00 WHERE Id = '40000000-0000-0000-0000-000000000001';

-- Put it back afterwards:
-- UPDATE Products SET Price = 499.00 WHERE Id = '40000000-0000-0000-0000-000000000001';


------------------------------------------------------------------------------
-- 3. Reset the cart between runs.
------------------------------------------------------------------------------
-- USE FlowerCartDb;
-- DELETE FROM CartItems;
-- DELETE FROM Carts;


------------------------------------------------------------------------------
-- 4. Inspect what the cart actually stored, including the price snapshot.
------------------------------------------------------------------------------
-- USE FlowerCartDb;
-- SELECT c.UserId, i.ProductName, i.Quantity, i.UnitPriceSnapshot, i.AddedAtUtc
-- FROM Carts c JOIN CartItems i ON i.CartId = c.Id;
