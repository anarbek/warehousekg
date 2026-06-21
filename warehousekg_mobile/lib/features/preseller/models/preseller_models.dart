// Pre-order status enum for UI display
enum PreOrderStatus {
  draft,
  submitted,
  approved,
  rejected,
  converted;
}

class PreOrderSummary {
  final String id;
  final String number;
  final String? customerName;
  final String? presellerName;
  final PreOrderStatus status;
  final String paymentType;
  final double totalAmount;
  final int lineCount;
  final DateTime orderDateUtc;

  PreOrderSummary({
    required this.id,
    required this.number,
    this.customerName,
    this.presellerName,
    required this.status,
    required this.paymentType,
    required this.totalAmount,
    required this.lineCount,
    required this.orderDateUtc,
  });

  factory PreOrderSummary.fromJson(Map<String, dynamic> json) => PreOrderSummary(
    id: json['id'] ?? '',
    number: json['number'] ?? '',
    customerName: json['customerName'],
    presellerName: json['presellerName'],
    status: PreOrderStatus.values[json['status'] ?? 0],
    paymentType: json['paymentType'] ?? '',
    totalAmount: (json['totalAmount'] ?? 0).toDouble(),
    lineCount: json['lineCount'] ?? 0,
    orderDateUtc: DateTime.parse(json['orderDateUtc'] ?? DateTime.now().toIso8601String()),
  );

  String get statusLabel {
    switch (status) {
      case PreOrderStatus.draft: return 'Черновик';
      case PreOrderStatus.submitted: return 'Отправлен';
      case PreOrderStatus.approved: return 'Одобрен';
      case PreOrderStatus.rejected: return 'Отклонён';
      case PreOrderStatus.converted: return 'Конвертирован';
    }
  }

  bool get isDraft => status == PreOrderStatus.draft;
}

class PreOrderModel {
  final String id;
  final String number;
  final String customerId;
  final String? customerName;
  final String? presellerName;
  final String warehouseId;
  final String? warehouseName;
  final PreOrderStatus status;
  final String paymentType;
  final String currency;
  final DateTime orderDateUtc;
  final DateTime? expectedDateUtc;
  final String? notes;
  final double totalAmount;
  final double amountPlanned;
  final double amountPaid;
  final String? convertedSalesOrderNumber;
  final List<PreOrderLineModel> lines;

  PreOrderModel({
    required this.id,
    required this.number,
    required this.customerId,
    this.customerName,
    this.presellerName,
    required this.warehouseId,
    this.warehouseName,
    required this.status,
    required this.paymentType,
    required this.currency,
    required this.orderDateUtc,
    this.expectedDateUtc,
    this.notes,
    required this.totalAmount,
    this.amountPlanned = 0,
    this.amountPaid = 0,
    this.convertedSalesOrderNumber,
    required this.lines,
  });

  factory PreOrderModel.fromJson(Map<String, dynamic> json) => PreOrderModel(
    id: json['id'] ?? '',
    number: json['number'] ?? '',
    customerId: json['customerId'] ?? '',
    customerName: json['customerName'],
    presellerName: json['presellerName'],
    warehouseId: json['warehouseId'] ?? '',
    warehouseName: json['warehouseName'],
    status: PreOrderStatus.values[json['status'] ?? 0],
    paymentType: json['paymentType'] ?? '',
    currency: json['currency'] ?? 'KGS',
    orderDateUtc: DateTime.parse(json['orderDateUtc'] ?? DateTime.now().toIso8601String()),
    expectedDateUtc: json['expectedDateUtc'] != null ? DateTime.parse(json['expectedDateUtc']) : null,
    notes: json['notes'],
    totalAmount: (json['totalAmount'] ?? 0).toDouble(),
    amountPlanned: (json['amountPlanned'] ?? 0).toDouble(),
    amountPaid: (json['amountPaid'] ?? 0).toDouble(),
    convertedSalesOrderNumber: json['convertedSalesOrderNumber'],
    lines: (json['lines'] as List<dynamic>?)
        ?.map((l) => PreOrderLineModel.fromJson(l as Map<String, dynamic>))
        .toList() ?? [],
  );
}

class PreOrderLineModel {
  final String id;
  final String inventoryItemId;
  final String? inventoryItemName;
  final String? sku;
  final double quantity;
  final double unitPrice;
  final double warehouseStockSnapshot;
  final double stockDifference;
  final double discountPercent;
  final double lineTotal;

  PreOrderLineModel({
    required this.id,
    required this.inventoryItemId,
    this.inventoryItemName,
    this.sku,
    required this.quantity,
    required this.unitPrice,
    required this.warehouseStockSnapshot,
    required this.stockDifference,
    required this.discountPercent,
    required this.lineTotal,
  });

  factory PreOrderLineModel.fromJson(Map<String, dynamic> json) => PreOrderLineModel(
    id: json['id'] ?? '',
    inventoryItemId: json['inventoryItemId'] ?? '',
    inventoryItemName: json['inventoryItemName'],
    sku: json['sku'],
    quantity: (json['quantity'] ?? 0).toDouble(),
    unitPrice: (json['unitPrice'] ?? 0).toDouble(),
    warehouseStockSnapshot: (json['warehouseStockSnapshot'] ?? 0).toDouble(),
    stockDifference: (json['stockDifference'] ?? 0).toDouble(),
    discountPercent: (json['discountPercent'] ?? 0).toDouble(),
    lineTotal: (json['lineTotal'] ?? 0).toDouble(),
  );
}

class PaymentType {
  final String name;

  PaymentType({required this.name});

  factory PaymentType.fromJson(Map<String, dynamic> json) => PaymentType(
    name: json['name'] ?? '',
  );
}

class WarehouseStockItem {
  final String inventoryItemId;
  final String sku;
  final String name;
  final double quantityOnHand;

  WarehouseStockItem({
    required this.inventoryItemId,
    required this.sku,
    required this.name,
    required this.quantityOnHand,
  });

  factory WarehouseStockItem.fromJson(Map<String, dynamic> json) => WarehouseStockItem(
    inventoryItemId: json['inventoryItemId'] ?? '',
    sku: json['sku'] ?? '',
    name: json['name'] ?? '',
    quantityOnHand: (json['quantityOnHand'] ?? 0).toDouble(),
  );
}
