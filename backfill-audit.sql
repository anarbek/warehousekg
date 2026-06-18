UPDATE pick_orders SET "CreatedAt" = NOW(), "UpdatedAt" = NOW(), "UpdatedBy" = 'migration' WHERE "CreatedAt" < '2000-01-01';
UPDATE pack_orders SET "CreatedAt" = NOW(), "UpdatedAt" = NOW(), "UpdatedBy" = 'migration' WHERE "CreatedAt" < '2000-01-01';
UPDATE stock_receipts SET "CreatedAt" = COALESCE("ReceivedAtUtc", NOW()), "UpdatedAt" = NOW(), "UpdatedBy" = 'migration' WHERE "CreatedAt" < '2000-01-01';
UPDATE stock_transfers SET "CreatedAt" = COALESCE("TransferredAtUtc", NOW()), "UpdatedAt" = NOW(), "UpdatedBy" = 'migration' WHERE "CreatedAt" < '2000-01-01';
