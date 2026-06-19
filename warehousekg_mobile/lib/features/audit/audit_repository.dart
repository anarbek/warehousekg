import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/api_constants.dart';
import '../auth/auth_repository.dart';
import 'models/audit_model.dart';

final auditRepositoryProvider = Provider<AuditRepository>((ref) {
  return AuditRepository(ref.watch(apiClientProvider));
});

class AuditRepository {
  final ApiClient _api;
  final List<AuditModel> _localAudits = [];
  final List<Warehouse> _cachedWarehouses = [];
  final List<CachedInventoryItem> _cachedItems = [];
  final List<RemoteAuditSummary> _remoteAudits = [];
  final Map<String, AuditModel> _remoteDetailCache = {};
  int _auditCounter = 1;
  final ValueNotifier<int> version = ValueNotifier<int>(0);

  void _bump() => version.value++;

  AuditRepository(this._api);

  // ─── Online ─────────────────────────────────────────────────────

  Future<List<Warehouse>> fetchWarehouses() async {
    final resp = await _api.dio.get(ApiConstants.warehouses);
    final list = (resp.data as List).map((j) => Warehouse(
          id: j['id'],
          code: j['code'] ?? '',
          name: j['name'] ?? '',
        )).toList();
    _cachedWarehouses.clear();
    _cachedWarehouses.addAll(list);
    return list;
  }

  Future<List<CachedInventoryItem>> fetchInventoryItems(String warehouseId) async {
    final resp = await _api.dio.get('${ApiConstants.warehouseStock}?warehouseId=$warehouseId');
    final list = (resp.data as List).map((j) => CachedInventoryItem(
          id: j['inventoryItemId'] ?? j['id'] ?? '',
          sku: j['sku'] ?? '',
          name: j['name'] ?? '',
          barcode: j['barcode'],
          quantityOnHand: (j['quantityOnHand'] ?? 0).toDouble(),
          warehouseId: warehouseId,
        )).toList();
    _cachedItems.removeWhere((i) => i.warehouseId == warehouseId);
    _cachedItems.addAll(list);
    return list;
  }

  Future<List<RemoteAuditSummary>> fetchAudits() async {
    final resp = await _api.dio.get(ApiConstants.stockAudits);
    final list = (resp.data as List).map((j) => RemoteAuditSummary(
          id: j['id'],
          number: j['number'] ?? '',
          warehouseId: j['warehouseId'] ?? '',
          warehouseName: j['warehouseName'],
          status: j['status'] ?? 'Draft',
          employeeName: j['employeeName'],
          lineCount: j['lineCount'] ?? 0,
          totalVariance: (j['totalVariance'] ?? 0).toDouble(),
          createdAt: DateTime.parse(j['createdAt']),
        )).toList();
    _remoteAudits.clear();
    _remoteAudits.addAll(list);
    // Sync counter to avoid colliding with existing backend audit numbers
    for (final a in list) {
      final m = RegExp(r'^AUD-M(\d+)$').firstMatch(a.number);
      if (m != null) {
        final n = int.parse(m.group(1)!);
        if (n >= _auditCounter) _auditCounter = n + 1;
      }
    }
    return list;
  }

  List<RemoteAuditSummary> getRemoteAudits() => List.unmodifiable(_remoteAudits);

  Future<AuditModel> fetchAuditDetail(String id) async {
    final resp = await _api.dio.get('${ApiConstants.stockAudits}/$id');
    final j = resp.data;
    final lines = (j['lines'] as List?)?.map((l) => AuditLine(
      id: l['id'] ?? '',
      inventoryItemId: l['inventoryItemId'] ?? '',
      inventoryItemName: l['inventoryItemName'] ?? '',
      inventoryItemSku: l['inventoryItemSku'] ?? '',
      systemQuantity: (l['systemQuantity'] ?? 0).toDouble(),
      countedQuantity: (l['countedQuantity'] ?? 0).toDouble(),
    )).toList() ?? [];
    final audit = AuditModel(
      id: id,
      number: j['number'] ?? '',
      warehouseId: j['warehouseId'] ?? '',
      warehouseName: j['warehouseName'] ?? '',
      notes: j['notes'],
      status: j['status'] ?? 'Completed',
      createdAt: DateTime.tryParse(j['reconciledAtUtc'] ?? '') ?? DateTime.now(),
      lines: lines,
    );
    _remoteDetailCache[id] = audit;
    return audit;
  }

  AuditModel? getRemoteDetail(String id) => _remoteDetailCache[id];

  Future<String> createAuditOnBackend(AuditModel audit) async {
    final lines = audit.lines.map((l) => {
      'inventoryItemId': l.inventoryItemId,
      'systemQuantity': l.systemQuantity,
      'countedQuantity': l.countedQuantity ?? 0,
    }).toList();
    final resp = await _api.dio.post(ApiConstants.stockAudits, data: {
      'number': audit.number,
      'warehouseId': audit.warehouseId,
      'notes': audit.notes,
      'lines': lines,
    });
    return resp.data.toString();
  }

  Future<void> completeAuditOnBackend(String id) async {
    await _api.dio.post(ApiConstants.auditComplete(id));
  }

  // ─── Offline (local) ────────────────────────────────────────────

  List<Warehouse> getCachedWarehouses() => List.unmodifiable(_cachedWarehouses);
  List<CachedInventoryItem> getCachedItems(String warehouseId) =>
      _cachedItems.where((i) => i.warehouseId == warehouseId).toList();

  AuditModel createLocalAudit(String warehouseId, String warehouseName, List<CachedInventoryItem> items, {String? notes}) {
    // Prevent multiple draft audits for the same warehouse on the same day.
    final today = DateTime.now();
    final existing = _localAudits.any((a) =>
        a.warehouseId == warehouseId &&
        a.status == 'Draft' &&
        a.createdAt.year == today.year &&
        a.createdAt.month == today.month &&
        a.createdAt.day == today.day);
    if (existing) {
      throw Exception('На этом складе уже есть черновой аудит за сегодня. Завершите или удалите его перед созданием нового.');
    }

    final id = DateTime.now().millisecondsSinceEpoch.toString();
    final number = 'AUD-M${_auditCounter.toString().padLeft(4, '0')}';
    _auditCounter++;
    final lines = items.map((item) => AuditLine(
          id: '${id}_${item.id}',
          inventoryItemId: item.id,
          inventoryItemName: item.name,
          inventoryItemSku: item.sku,
          systemQuantity: item.quantityOnHand,
        )).toList();
    final audit = AuditModel(
      id: id,
      number: number,
      warehouseId: warehouseId,
      warehouseName: warehouseName,
      notes: notes,
      status: 'Draft',
      createdAt: DateTime.now(),
      lines: lines,
    );
    _localAudits.add(audit);
    _bump();
    return audit;
  }

  void updateCountedQuantity(String auditId, String lineId, double? quantity) {
    final audit = _localAudits.firstWhere((a) => a.id == auditId);
    final line = audit.lines.firstWhere((l) => l.id == lineId);
    final idx = audit.lines.indexOf(line);
    audit.lines[idx] = AuditLine(
      id: line.id,
      inventoryItemId: line.inventoryItemId,
      inventoryItemName: line.inventoryItemName,
      inventoryItemSku: line.inventoryItemSku,
      systemQuantity: line.systemQuantity,
      countedQuantity: quantity,
    );
    _bump();
  }

  List<AuditModel> getLocalAudits() => List.unmodifiable(_localAudits);

  AuditModel? getLocalAudit(String id) {
    try {
      return _localAudits.firstWhere((a) => a.id == id);
    } catch (_) {
      return null;
    }
  }

  void deleteLocalAudit(String id) {
    _localAudits.removeWhere((a) => a.id == id);
    _bump();
  }

  /// Removes local audits that have been synced to the backend.
  void removeSyncedLocalAudits() {
    _localAudits.removeWhere((a) => a.status == 'Synced');
    _bump();
  }

  // ─── Sync ───────────────────────────────────────────────────────

  Future<void> syncAudit(String auditId) async {
    final audit = getLocalAudit(auditId);
    if (audit == null || audit.status != 'PendingSync') return;

    // If already created on backend, don't re-create
    if (audit.backendId != null) {
      _markSynced(auditId, audit.backendId!);
      return;
    }

    // Check if an audit with the same number already exists on backend
    // (e.g. from a previous partial sync that created but didn't complete)
    final existingRemote = _remoteAudits.where((r) => r.number == audit.number);
    if (existingRemote.isNotEmpty) {
      _markSynced(auditId, existingRemote.first.id);
      return;
    }

    try {
      final backendId = await createAuditOnBackend(audit);
      _markSynced(auditId, backendId);
    } catch (_) {
      // Keep as PendingSync, will retry
    }
  }

  void _markSynced(String auditId, String backendId) {
    final idx = _localAudits.indexWhere((a) => a.id == auditId);
    final audit = _localAudits[idx];
    _localAudits[idx] = AuditModel(
      id: audit.id,
      backendId: backendId,
      number: audit.number,
      warehouseId: audit.warehouseId,
      warehouseName: audit.warehouseName,
      notes: audit.notes,
      status: 'Synced',
      createdAt: audit.createdAt,
      lines: audit.lines,
    );
    _bump();
  }

  void markPendingSync(String auditId) {
    final idx = _localAudits.indexWhere((a) => a.id == auditId);
    final audit = _localAudits[idx];
    _localAudits[idx] = AuditModel(
      id: audit.id,
      number: audit.number,
      warehouseId: audit.warehouseId,
      warehouseName: audit.warehouseName,
      notes: audit.notes,
      status: 'PendingSync',
      createdAt: audit.createdAt,
      lines: audit.lines,
    );
    _bump();
  }
}
