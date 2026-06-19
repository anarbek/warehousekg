import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'audit_repository.dart';
import 'models/audit_model.dart';

class AuditDetailScreen extends ConsumerStatefulWidget {
  final String auditId;
  const AuditDetailScreen({super.key, required this.auditId});

  @override
  ConsumerState<AuditDetailScreen> createState() => _AuditDetailScreenState();
}

class _AuditDetailScreenState extends ConsumerState<AuditDetailScreen> {
  AuditModel? _audit;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final repo = ref.read(auditRepositoryProvider);
    var audit = repo.getLocalAudit(widget.auditId) ?? repo.getRemoteDetail(widget.auditId);
    if (audit == null) {
      try {
        audit = await repo.fetchAuditDetail(widget.auditId);
      } catch (e) {
        debugPrint('fetchAuditDetail error: $e');
      }
    }
    if (mounted) setState(() { _audit = audit; _loading = false; });
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Аудит')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    final audit = _audit;
    if (audit == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Аудит')),
        body: const Center(child: Text('Аудит не найден')),
      );
    }

    final isLocal = audit.id.length < 30; // local IDs are 13-digit timestamps, GUIDs are 36 chars

    return Scaffold(
      appBar: AppBar(
        leading: IconButton(icon: const Icon(Icons.arrow_back), onPressed: () => Navigator.of(context).pop()),
        title: Text(audit.number),
      ),
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _infoRow('Склад', audit.warehouseName),
                _infoRow('Статус', audit.status),
                _infoRow('Создан', '${audit.createdAt.day}.${audit.createdAt.month}.${audit.createdAt.year}'),
                _infoRow('Всего позиций', '${audit.totalItems}'),
                _infoRow('Посчитано', '${audit.countedItems} из ${audit.totalItems}'),
                if (audit.notes != null && audit.notes!.isNotEmpty) _infoRow('Заметки', audit.notes!),
                const SizedBox(height: 12),
                LinearProgressIndicator(value: audit.totalItems > 0 ? audit.countedItems / audit.totalItems : 0),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text('Позиции (${audit.lines.length})',
                style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
          ),
          const SizedBox(height: 4),
          Expanded(
            child: ListView.builder(
              itemCount: audit.lines.length,
              padding: const EdgeInsets.symmetric(horizontal: 8),
              itemBuilder: (context, index) {
                final line = audit.lines[index];
                final counted = line.countedQuantity;
                final variance = line.variance;
                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(12),
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(line.inventoryItemName, style: const TextStyle(fontWeight: FontWeight.w500)),
                              Text('SKU: ${line.inventoryItemSku}', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                            ],
                          ),
                        ),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.end,
                          children: [
                            Text('Система: ${line.systemQuantity.toStringAsFixed(1)}',
                                style: const TextStyle(fontSize: 13, color: Colors.grey)),
                            Text(
                              counted != null ? 'Факт: ${counted.toStringAsFixed(1)}' : 'Не посчитано',
                              style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600,
                                  color: counted != null ? Colors.blue : Colors.orange),
                            ),
                            if (variance != null && variance != 0)
                              Text('Откл: ${variance > 0 ? "+" : ""}${variance.toStringAsFixed(1)}',
                                  style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600,
                                      color: variance > 0 ? Colors.red : Colors.green)),
                          ],
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
          if (isLocal && (audit.status == 'Draft' || audit.status == 'PendingSync'))
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: Column(
                children: [
                  SizedBox(
                    width: double.infinity,
                    child: FilledButton.icon(
                      onPressed: () => context.push('/audits/${audit.id}/count'),
                      icon: const Icon(Icons.edit),
                      label: Text(audit.countedItems > 0 ? 'Продолжить' : 'Начать подсчёт'),
                    ),
                  ),
                  if (audit.isAllCounted) ...[
                    const SizedBox(height: 8),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.icon(
                        onPressed: () => _completeAudit(audit),
                        icon: const Icon(Icons.check),
                        label: const Text('Завершить аудит'),
                        style: FilledButton.styleFrom(backgroundColor: Colors.green),
                      ),
                    ),
                  ],
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _infoRow(String label, String value) => Padding(
        padding: const EdgeInsets.only(bottom: 8),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(width: 120, child: Text(label, style: const TextStyle(color: Colors.grey))),
            Expanded(child: Text(value, style: const TextStyle(fontWeight: FontWeight.w500))),
          ],
        ),
      );

  void _completeAudit(AuditModel audit) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Завершить аудит?'),
        content: const Text('После завершения данные будут отправлены на сервер. Изменения станут невозможны.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
          FilledButton(
            onPressed: () {
              Navigator.pop(ctx);
              final repo = ref.read(auditRepositoryProvider);
              repo.markPendingSync(audit.id);
              repo.syncAudit(audit.id);
              if (context.mounted) {
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Аудит отправлен на синхронизацию')));
                context.go('/audits');
              }
            },
            child: const Text('Завершить'),
          ),
        ],
      ),
    );
  }
}
