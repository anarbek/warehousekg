import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import '../../features/audit/audit_repository.dart';

final syncServiceProvider = Provider<SyncService>((ref) {
  return SyncService(ref);
});

class SyncService {
  final Ref _ref;
  StreamSubscription? _connectivitySub;
  Timer? _periodicTimer;

  SyncService(this._ref) {
    _start();
  }

  void _start() {
    _connectivitySub = Connectivity().onConnectivityChanged.listen((results) {
      if (!results.contains(ConnectivityResult.none)) {
        _syncPending();
      }
    });

    _periodicTimer = Timer.periodic(const Duration(seconds: 30), (_) {
      _syncPending();
    });
  }

  Future<void> _syncPending() async {
    final repo = _ref.read(auditRepositoryProvider);
    final pending = repo.getLocalAudits().where((a) => a.status == 'PendingSync').toList();
    for (final audit in pending) {
      await repo.syncAudit(audit.id);
    }
  }

  void dispose() {
    _connectivitySub?.cancel();
    _periodicTimer?.cancel();
  }
}
