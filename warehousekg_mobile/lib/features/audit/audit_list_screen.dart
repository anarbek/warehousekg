import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'audit_repository.dart';
import 'models/audit_model.dart';
import '../../core/network/connectivity.dart';

class AuditListScreen extends ConsumerStatefulWidget {
  const AuditListScreen({super.key});

  @override
  ConsumerState<AuditListScreen> createState() => _AuditListScreenState();
}

class _AuditListScreenState extends ConsumerState<AuditListScreen> {
  bool _remoteLoaded = false;

  @override
  void initState() {
    super.initState();
    _loadRemote();
  }

  Future<void> _loadRemote() async {
    final repo = ref.read(auditRepositoryProvider);
    try {
      await repo.fetchAudits();
    } catch (_) {
      // offline — stay with cached/empty
    }
    if (mounted) setState(() => _remoteLoaded = true);
  }

  @override
  Widget build(BuildContext context) {
    final repo = ref.watch(auditRepositoryProvider);
    final localAudits = repo.getLocalAudits();
    final pendingCount = localAudits.where((a) => a.status == 'PendingSync').length;
    final isOnline = ref.watch(isOnlineProvider);
    final remoteAudits = repo.getRemoteAudits();
    final warehouses = repo.getCachedWarehouses();

    // Dedup: local audits already synced share the same number as a remote audit
    final localNumbers = localAudits.map((a) => a.number).toSet();
    final remoteFiltered = remoteAudits.where((r) => !localNumbers.contains(r.number)).toList();

    // Build display list
    final allItems = <_AuditListItem>[];
    for (final a in localAudits) {
      allItems.add(_AuditListItem(
        key: 'local_${a.id}',
        isLocal: true,
        localAudit: a,
        number: a.number,
        warehouseName: a.warehouseName,
        status: a.status,
        employeeName: null,
        lineCount: a.totalItems,
        countedItems: a.countedItems,
        totalVariance: null,
        createdAt: a.createdAt,
      ));
    }
    for (final r in remoteFiltered) {
      final wh = warehouses.cast<Warehouse?>().firstWhere((w) => w!.id == r.warehouseId, orElse: () => null);
      allItems.add(_AuditListItem(
        key: 'remote_${r.id}',
        isLocal: false,
        remoteId: r.id,
        number: r.number,
        warehouseName: wh?.name ?? r.warehouseId,
        status: r.status,
        employeeName: r.employeeName,
        lineCount: r.lineCount,
        countedItems: null,
        totalVariance: r.totalVariance,
        createdAt: r.createdAt,
      ));
    }

    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/dashboard'),
        ),
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Аудиты'),
            if (pendingCount > 0)
              Container(
                margin: const EdgeInsets.only(left: 8),
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(color: Colors.orange.shade100, borderRadius: BorderRadius.circular(10)),
                child: Text('$pendingCount на синхр.', style: TextStyle(fontSize: 11, color: Colors.orange.shade900)),
              ),
          ],
        ),
        actions: [
          IconButton(icon: const Icon(Icons.refresh), onPressed: () { _remoteLoaded = false; _loadRemote(); }),
          if (!isOnline) const Padding(padding: EdgeInsets.only(right: 4), child: Icon(Icons.cloud_off, color: Colors.orange, size: 20)),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showNewAuditDialog(repo),
        icon: const Icon(Icons.add),
        label: const Text('Новый аудит'),
      ),
      body: allItems.isEmpty
          ? const Center(child: Text('Нет аудитов', style: TextStyle(color: Colors.grey, fontSize: 16)))
          : ListView.builder(
              itemCount: allItems.length,
              padding: const EdgeInsets.all(8),
              itemBuilder: (context, index) {
                final item = allItems[index];
                final sub = <String>[];
                sub.add('Позиций: ${item.lineCount}');
                if (item.countedItems != null) sub.add('Посчитано: ${item.countedItems}');
                if (item.totalVariance != null && item.totalVariance != 0) {
                  sub.add('Откл: ${item.totalVariance! > 0 ? "+" : ""}${item.totalVariance!.toStringAsFixed(1)}');
                }
                if (item.employeeName != null && item.employeeName!.isNotEmpty) sub.add(item.employeeName!);

                final tile = ListTile(
                  leading: _statusBadge(item.status),
                  title: Text('${item.number} — ${item.warehouseName}'),
                  subtitle: Text(sub.join(' | ')),
                  trailing: item.isLocal ? const Icon(Icons.chevron_right) : const Icon(Icons.chevron_right),
                  onTap: item.isLocal
                      ? () => context.push('/audits/${item.localAudit!.id}')
                      : () => context.push('/audits/${item.remoteId}'),
                );

                if (!item.isLocal) return Card(child: tile);

                return Dismissible(
                  key: Key(item.key),
                  direction: DismissDirection.endToStart,
                  background: Container(
                    alignment: Alignment.centerRight,
                    padding: const EdgeInsets.only(right: 20),
                    color: Colors.red,
                    child: const Icon(Icons.delete, color: Colors.white),
                  ),
                  confirmDismiss: (_) => showDialog<bool>(
                    context: context,
                    builder: (ctx) => AlertDialog(
                      title: const Text('Удалить аудит?'),
                      content: Text('${item.number} — ${item.warehouseName}'),
                      actions: [
                        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
                        FilledButton(
                          style: FilledButton.styleFrom(backgroundColor: Colors.red),
                          onPressed: () => Navigator.pop(ctx, true),
                          child: const Text('Удалить'),
                        ),
                      ],
                    ),
                  ),
                  onDismissed: (_) => repo.deleteLocalAudit(item.localAudit!.id),
                  child: Card(child: tile),
                );
              },
            ),
    );
  }

  Widget _statusBadge(String status) {
    final (Color color, String text) = switch (status) {
      'Draft' => (Colors.orange, 'Draft'),
      'PendingSync' => (Colors.blue, 'Sync'),
      'Completed' => (Colors.green, 'Done'),
      _ => (Colors.grey, status),
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(color: color.withAlpha(38), borderRadius: BorderRadius.circular(8)),
      child: Text(text, style: TextStyle(color: color, fontWeight: FontWeight.w600, fontSize: 12)),
    );
  }

  void _showNewAuditDialog(AuditRepository repo) async {
    // Try to fetch warehouses first, fall back to cache
    List<Warehouse> warehouses = repo.getCachedWarehouses();
    final dialogCtx = context; // capture before async gap
    if (!mounted) return;

    if (warehouses.isEmpty) {
      // Show loading dialog while fetching
      showDialog(
        context: dialogCtx,
        barrierDismissible: false,
        builder: (_) => const Center(child: CircularProgressIndicator()),
      );
      try {
        warehouses = await repo.fetchWarehouses();
      } catch (_) {
        // offline — no warehouses cached either
        if (dialogCtx.mounted) Navigator.pop(dialogCtx); // close loading
        if (mounted) {
          ScaffoldMessenger.of(dialogCtx).showSnackBar(
            const SnackBar(content: Text('Нет подключения. Загрузите склады при наличии интернета.')),
          );
        }
        return;
      }
      if (dialogCtx.mounted) Navigator.pop(dialogCtx); // close loading
    }

    buildAuditForm(dialogCtx, repo, warehouses);
  }

  void buildAuditForm(BuildContext ctx, AuditRepository repo, List<Warehouse> warehouses) {
    String? selectedWarehouseId;
    final notesCtrl = TextEditingController();
    bool loading = false;
    List<CachedInventoryItem>? loadedItems;

    showDialog(
      context: ctx,
      barrierDismissible: false,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setDialogState) {
            return AlertDialog(
              title: const Text('Новый аудит'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  DropdownButtonFormField<String>(
                    decoration: const InputDecoration(labelText: 'Склад'),
                    items: warehouses.map((w) => DropdownMenuItem(value: w.id, child: Text(w.name))).toList(),
                    onChanged: (v) => selectedWarehouseId = v,
                  ),
                  const SizedBox(height: 12),
                  TextField(controller: notesCtrl, decoration: const InputDecoration(labelText: 'Заметки (опционально)')),
                  if (loadedItems != null)
                    Padding(
                      padding: const EdgeInsets.only(top: 12),
                      child: Text('Товаров: ${loadedItems!.length}', style: const TextStyle(color: Colors.grey)),
                    ),
                ],
              ),
              actions: [
                TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
                FilledButton(
                  onPressed: loading ? null : () async {
                    if (selectedWarehouseId == null) {
                      ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(content: Text('Выберите склад')));
                      return;
                    }
                    setDialogState(() => loading = true);
                    try {
                      try {
                        loadedItems = await repo.fetchInventoryItems(selectedWarehouseId!);
                      } catch (e) {
                        // ignore: avoid_print
                        print('fetchInventoryItems failed: $e');
                        loadedItems = repo.getCachedItems(selectedWarehouseId!);
                      }
                      if (loadedItems == null || loadedItems!.isEmpty) {
                        setDialogState(() => loading = false);
                        if (ctx.mounted) {
                          ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(
                            content: Text('На складе нет товаров. Сначала добавьте товары в веб-приложении.'),
                            duration: Duration(seconds: 4),
                          ));
                        }
                        return;
                      }
                      final wh = warehouses.firstWhere((w) => w.id == selectedWarehouseId);
                      final audit = repo.createLocalAudit(selectedWarehouseId!, wh.name, loadedItems!, notes: notesCtrl.text.isNotEmpty ? notesCtrl.text : null);
                      if (ctx.mounted) {
                        Navigator.pop(ctx);
                        context.push('/audits/${audit.id}');
                      }
                    } catch (e) {
                      debugPrint('New audit error: $e');
                      setDialogState(() => loading = false);
                      if (ctx.mounted) {
                        ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
                      }
                    }
                  },
                  child: Text(loading ? 'Загрузка...' : 'Начать аудит'),
                ),
              ],
            );
          },
        );
      },
    );
  }
}

class _AuditListItem {
  final String key;
  final bool isLocal;
  final AuditModel? localAudit;
  final String? remoteId;
  final String number;
  final String warehouseName;
  final String status;
  final String? employeeName;
  final int lineCount;
  final int? countedItems;
  final double? totalVariance;
  final DateTime createdAt;

  _AuditListItem({
    required this.key,
    required this.isLocal,
    this.localAudit,
    this.remoteId,
    required this.number,
    required this.warehouseName,
    required this.status,
    this.employeeName,
    required this.lineCount,
    this.countedItems,
    this.totalVariance,
    required this.createdAt,
  });
}
