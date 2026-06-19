class AuditModel {
  final String id;
  final String number;
  final String warehouseId;
  final String warehouseName;
  final String? notes;
  final String? reconciledAtUtc;
  final String? employeeId;
  final String status; // Draft, PendingSync, Synced, Completed
  final String? backendId; // set after sync to backend
  final DateTime createdAt;
  final List<AuditLine> lines;

  AuditModel({
    required this.id,
    required this.number,
    required this.warehouseId,
    required this.warehouseName,
    this.notes,
    this.reconciledAtUtc,
    this.employeeId,
    required this.status,
    this.backendId,
    required this.createdAt,
    this.lines = const [],
  });

  int get totalItems => lines.length;
  int get countedItems => lines.where((l) => l.countedQuantity != null).length;
  bool get isAllCounted => totalItems > 0 && countedItems == totalItems;
}

class AuditLine {
  final String id;
  final String inventoryItemId;
  final String inventoryItemName;
  final String inventoryItemSku;
  final String? barcode;
  final double systemQuantity;
  final double? countedQuantity;

  AuditLine({
    required this.id,
    required this.inventoryItemId,
    this.inventoryItemName = '',
    this.inventoryItemSku = '',
    this.barcode,
    required this.systemQuantity,
    this.countedQuantity,
  });

  double? get variance {
    if (countedQuantity == null) return null;
    return countedQuantity! - systemQuantity;
  }
}

class Warehouse {
  final String id;
  final String code;
  final String name;
  Warehouse({required this.id, required this.code, required this.name});
}

class CachedInventoryItem {
  final String id;
  final String sku;
  final String name;
  final String? barcode;
  final double quantityOnHand;
  final String warehouseId;
  CachedInventoryItem({
    required this.id,
    required this.sku,
    required this.name,
    this.barcode,
    required this.quantityOnHand,
    required this.warehouseId,
  });
}

class RemoteAuditSummary {
  final String id;
  final String number;
  final String warehouseId;
  final String? warehouseName;
  final String status; // Draft, Completed, Cancelled
  final String? employeeName;
  final int lineCount;
  final double totalVariance;
  final DateTime createdAt;

  RemoteAuditSummary({
    required this.id,
    required this.number,
    required this.warehouseId,
    this.warehouseName,
    required this.status,
    this.employeeName,
    required this.lineCount,
    required this.totalVariance,
    required this.createdAt,
  });
}
