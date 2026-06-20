import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/api_constants.dart';
import '../auth/auth_repository.dart';
import 'models/dispatching_models.dart';

final dispatchingRepositoryProvider = Provider<DispatchingRepository>((ref) {
  return DispatchingRepository(ref.watch(apiClientProvider));
});

class DispatchingRepository {
  final ApiClient _api;
  DispatchingRepository(this._api);

  Future<List<RouteModel>> getMyRoutes() async {
    final resp = await _api.dio.get(ApiConstants.myRoutes);
    return (resp.data as List).map((j) => RouteModel.fromJson(j)).toList();
  }

  Future<RouteDetailModel> getMyRouteDetail(String id) async {
    final resp = await _api.dio.get(ApiConstants.myRouteDetail(id));
    return RouteDetailModel.fromJson(resp.data);
  }

  Future<void> startRoute(String id) async {
    await _api.dio.post(ApiConstants.routeStart(id));
  }

  Future<void> completeRoute(String id) async {
    await _api.dio.post(ApiConstants.routeComplete(id));
  }

  Future<void> arriveAtStop(String routeId, String stopId) async {
    await _api.dio.post(ApiConstants.stopArrive(routeId, stopId));
  }

  Future<void> completeStop(String routeId, String stopId) async {
    await _api.dio.post(ApiConstants.stopComplete(routeId, stopId));
  }

  Future<void> skipStop(String routeId, String stopId, String? reason) async {
    await _api.dio.post(
      ApiConstants.stopSkip(routeId, stopId),
      data: {'notes': reason ?? ''},
    );
  }
}

class RouteDetailModel {
  final String id;
  final String code;
  final DateTime date;
  final String? vehicleCode;
  final String? driverName;
  final String status;
  final String? notes;
  final int stopCount;
  final List<StopModel> stops;

  RouteDetailModel({
    required this.id,
    required this.code,
    required this.date,
    this.vehicleCode,
    this.driverName,
    required this.status,
    this.notes,
    required this.stopCount,
    required this.stops,
  });

  factory RouteDetailModel.fromJson(Map<String, dynamic> json) => RouteDetailModel(
    id: json['id'] ?? '',
    code: json['code'] ?? '',
    date: DateTime.parse(json['date'] ?? DateTime.now().toIso8601String()),
    vehicleCode: json['vehicleCode'],
    driverName: json['driverName'],
    status: json['status'] ?? 'Planned',
    notes: json['notes'],
    stopCount: json['stopCount'] ?? 0,
    stops: (json['stops'] as List<dynamic>?)
        ?.map((s) => StopModel.fromJson(s))
        .toList() ?? [],
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
  bool get canStart => status == 'Planned';
  bool get canComplete => status == 'InProgress';
}
