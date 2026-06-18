SELECT 'receipt' AS src, sr."ReceivedAtUtc", sl."Quantity"
FROM stock_receipt_lines sl JOIN stock_receipts sr ON sl."StockReceiptId" = sr."Id"
WHERE sl."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND sr."WarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND sr."Status" = 'Completed'
  AND sr."ReceivedAtUtc" IS NOT NULL
ORDER BY sr."ReceivedAtUtc";

SELECT 'adjust' AS src, sa."AdjustedAtUtc", sal."QuantityChange" AS qty
FROM stock_adjustment_lines sal JOIN stock_adjustments sa ON sal."StockAdjustmentId" = sa."Id"
WHERE sal."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND sa."WarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND sa."Status" = 'Completed'
  AND sa."AdjustedAtUtc" IS NOT NULL
ORDER BY sa."AdjustedAtUtc";

SELECT 'pick' AS src, po."PlannedPickDate", po."PickedAtUtc", pol."Quantity" AS qty
FROM pick_order_lines pol JOIN pick_orders po ON pol."PickOrderId" = po."Id"
WHERE pol."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND po."WarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND po."Status" = 'Completed';

SELECT 'po' AS src, po."ReceivedAtUtc", pol."Quantity" AS qty
FROM purchase_order_lines pol JOIN purchase_orders po ON pol."PurchaseOrderId" = po."Id"
WHERE pol."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND po."WarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND po."Status" = 'Received'
  AND po."ReceivedAtUtc" IS NOT NULL
ORDER BY po."ReceivedAtUtc";

SELECT 'so' AS src, so."ShippedAtUtc", sol."Quantity" AS qty
FROM sales_order_lines sol JOIN sales_orders so ON sol."SalesOrderId" = so."Id"
WHERE sol."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND so."WarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND so."Status" = 'Shipped'
  AND so."ShippedAtUtc" IS NOT NULL
ORDER BY so."ShippedAtUtc";

SELECT 'transfer_in' AS src, st."TransferredAtUtc", stl."Quantity" AS qty
FROM stock_transfer_lines stl JOIN stock_transfers st ON stl."StockTransferId" = st."Id"
WHERE stl."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND st."DestinationWarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND st."Status" = 'Completed'
  AND st."TransferredAtUtc" IS NOT NULL
ORDER BY st."TransferredAtUtc";

SELECT 'transfer_out' AS src, st."TransferredAtUtc", stl."Quantity" AS qty
FROM stock_transfer_lines stl JOIN stock_transfers st ON stl."StockTransferId" = st."Id"
WHERE stl."InventoryItemId" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0'
  AND st."SourceWarehouseId" = '1ac52ff4-ac6c-486d-8666-f09308b3d0af'
  AND st."Status" = 'Completed'
  AND st."TransferredAtUtc" IS NOT NULL
ORDER BY st."TransferredAtUtc";

SELECT 'qoh' AS src, NULL::timestamptz, "QuantityOnHand" AS qty FROM inventory_items WHERE "Id" = '21ca97d7-53f7-4ccd-a98a-3af249e118c0';
