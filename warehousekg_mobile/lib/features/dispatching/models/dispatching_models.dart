class RouteModel {
  final String id;
  final String code;
  final DateTime date;
  final String? vehicleCode;
  final String? driverName;
  final String status;
  final String? notes;
  final int stopCount;

  RouteModel({
    required this.id,
    required this.code,
    required this.date,
    this.vehicleCode,
    this.driverName,
    required this.status,
    this.notes,
    required this.stopCount,
  });

  factory RouteModel.fromJson(Map<String, dynamic> json) => RouteModel(
    id: json['id'] ?? '',
    code: json['code'] ?? '',
    date: DateTime.parse(json['date'] ?? DateTime.now().toIso8601String()),
    vehicleCode: json['vehicleCode'],
    driverName: json['driverName'],
    status: json['status'] ?? 'Planned',
    notes: json['notes'],
    stopCount: json['stopCount'] ?? 0,
  );

  String get statusLabel {
    switch (status) {
      case 'Planned': return 'Запланирован';
      case 'InProgress': return 'В пути';
      case 'Completed': return 'Завершён';
      case 'Cancelled': return 'Отменён';
      default: return status;
    }
  }

  bool get isActive => status == 'Planned' || status == 'InProgress';
}

class StopModel {
  final String id;
  final String routeId;
  final int sequenceNumber;
  final String? customerName;
  final String? address;
  final double? latitude;
  final double? longitude;
  final String status;
  final String? notes;
  final int shipmentCount;
  final List<ShipmentModel> shipments;

  StopModel({
    required this.id,
    required this.routeId,
    required this.sequenceNumber,
    this.customerName,
    this.address,
    this.latitude,
    this.longitude,
    required this.status,
    this.notes,
    required this.shipmentCount,
    this.shipments = const [],
  });

  factory StopModel.fromJson(Map<String, dynamic> json) => StopModel(
    id: json['id'] ?? '',
    routeId: json['routeId'] ?? '',
    sequenceNumber: json['sequenceNumber'] ?? 0,
    customerName: json['customerName'],
    address: json['address'],
    latitude: (json['latitude'] as num?)?.toDouble(),
    longitude: (json['longitude'] as num?)?.toDouble(),
    status: json['status'] ?? 'Pending',
    notes: json['notes'],
    shipmentCount: json['shipmentCount'] ?? 0,
    shipments: (json['shipments'] as List<dynamic>?)
        ?.map((s) => ShipmentModel.fromJson(s))
        .toList() ?? [],
  );

  String get statusLabel {
    switch (status) {
      case 'Pending': return 'Ожидает';
      case 'InProgress': return 'В процессе';
      case 'Completed': return 'Завершён';
      case 'Skipped': return 'Пропущен';
      default: return status;
    }
  }

  bool get canArrive => status == 'Pending';
  bool get canComplete => status == 'InProgress';
  bool get canSkip => status == 'Pending' || status == 'InProgress';
}

class ShipmentModel {
  final String id;
  final String salesOrderNumber;
  final String? customerName;
  final String status;

  ShipmentModel({
    required this.id,
    required this.salesOrderNumber,
    this.customerName,
    required this.status,
  });

  factory ShipmentModel.fromJson(Map<String, dynamic> json) => ShipmentModel(
    id: json['id'] ?? '',
    salesOrderNumber: json['salesOrderNumber'] ?? '',
    customerName: json['customerName'],
    status: json['status'] ?? 'Pending',
  );
}
