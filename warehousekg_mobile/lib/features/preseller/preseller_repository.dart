import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/api_constants.dart';
import '../auth/auth_repository.dart';
import 'models/preseller_models.dart';

final presellerRepositoryProvider = Provider<PresellerRepository>((ref) {
  return PresellerRepository(ref.watch(apiClientProvider));
});

class PresellerRepository {
  final ApiClient _api;

  PresellerRepository(this._api);

  Future<List<PreOrderSummary>> getMyPreOrders() async {
    final resp = await _api.dio.get(ApiConstants.myPreOrders);
    return (resp.data as List<dynamic>)
        .map((e) => PreOrderSummary.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<PreOrderModel> getPreOrderById(String id) async {
    final resp = await _api.dio.get(ApiConstants.preOrderDetail(id));
    return PreOrderModel.fromJson(resp.data as Map<String, dynamic>);
  }

  Future<String> createPreOrder(Map<String, dynamic> data) async {
    final resp = await _api.dio.post(ApiConstants.preOrders, data: data);
    return resp.data.toString().replaceAll('"', '');
  }

  Future<void> submitPreOrder(String id) async {
    await _api.dio.post(ApiConstants.preOrderSubmit(id));
  }

  Future<void> updatePreOrder(String id, Map<String, dynamic> data) async {
    await _api.dio.put(ApiConstants.preOrderUpdate(id), data: data);
  }

  Future<List<PaymentType>> getPaymentTypes() async {
    final resp = await _api.dio.get(ApiConstants.paymentTypes);
    return (resp.data as List<dynamic>)
        .map((e) => PaymentType.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<List<WarehouseStockItem>> getWarehouseStock(String warehouseId) async {
    final resp = await _api.dio.get(
      ApiConstants.preOrderWarehouseStock,
      queryParameters: {'warehouseId': warehouseId},
    );
    return (resp.data as List<dynamic>)
        .map((e) => WarehouseStockItem.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<List<Map<String, dynamic>>> getCustomers() async {
    final resp = await _api.dio.get('${ApiConstants.apiPrefix}/customers');
    return (resp.data as List<dynamic>)
        .map((e) => e as Map<String, dynamic>)
        .toList();
  }

  Future<List<Map<String, dynamic>>> getWarehouses() async {
    final resp = await _api.dio.get(ApiConstants.warehouses);
    return (resp.data as List<dynamic>)
        .map((e) => e as Map<String, dynamic>)
        .toList();
  }

  Future<List<Map<String, dynamic>>> getInventoryItems() async {
    final resp = await _api.dio.get(ApiConstants.inventoryItems);
    return (resp.data as List<dynamic>)
        .map((e) => e as Map<String, dynamic>)
        .toList();
  }
}
