import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'audit_repository.dart';
import 'models/audit_model.dart';

class AuditCountScreen extends ConsumerStatefulWidget {
  final String auditId;
  const AuditCountScreen({super.key, required this.auditId});

  @override
  ConsumerState<AuditCountScreen> createState() => _AuditCountScreenState();
}

class _AuditCountScreenState extends ConsumerState<AuditCountScreen> {
  String _filter = 'all';
  String _search = '';
  final _searchCtrl = TextEditingController();
  final Map<String, TextEditingController> _qtyCtrls = {};

  @override
  void dispose() {
    _searchCtrl.dispose();
    for (final c in _qtyCtrls.values) {
      c.dispose();
    }
    super.dispose();
  }

  TextEditingController _ctrlFor(AuditLine line) {
    if (!_qtyCtrls.containsKey(line.id)) {
      _qtyCtrls[line.id] = TextEditingController(
        text: line.countedQuantity?.toString() ?? '',
      );
    }
    return _qtyCtrls[line.id]!;
  }

  @override
  Widget build(BuildContext context) {
    final repo = ref.watch(auditRepositoryProvider);
    final audit = repo.getLocalAudit(widget.auditId);
    if (audit == null) {
      return Scaffold(appBar: AppBar(), body: const Center(child: Text('Аудит не найден')));
    }

    final filtered = audit.lines.where((l) {
      if (_filter == 'uncounted' && l.countedQuantity != null) return false;
      if (_filter == 'counted' && l.countedQuantity == null) return false;
      if (_filter == 'variance' && (l.countedQuantity == null || l.variance == 0)) return false;
      if (_search.isNotEmpty) {
        final q = _search.toLowerCase();
        if (!l.inventoryItemName.toLowerCase().contains(q) && !l.inventoryItemSku.toLowerCase().contains(q)) return false;
      }
      return true;
    }).toList();

    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          tooltip: 'Пауза',
          onPressed: () => Navigator.of(context).pop(),
        ),
        title: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(audit.number, style: const TextStyle(fontSize: 14)),
          Text('${audit.countedItems}/${audit.totalItems} посчитано', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.normal)),
        ]),
        actions: [
          IconButton(
            icon: const Icon(Icons.pause),
            tooltip: 'Пауза',
            onPressed: () => Navigator.pop(context),
          ),
          if (audit.isAllCounted)
            IconButton(
              icon: const Icon(Icons.check_circle),
              tooltip: 'Завершить',
              color: Colors.green,
              onPressed: () => _completeAudit(repo, audit),
            ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(8),
            child: TextField(
              controller: _searchCtrl,
              decoration: const InputDecoration(
                hintText: 'Поиск по названию или SKU...',
                prefixIcon: Icon(Icons.search),
                isDense: true,
              ),
              onChanged: (v) => setState(() => _search = v),
            ),
          ),
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: Row(
              children: [
                _filterChip('Все', 'all'),
                _filterChip('Не посчитано', 'uncounted'),
                _filterChip('Посчитано', 'counted'),
                _filterChip('Расхождения', 'variance'),
              ],
            ),
          ),
          Expanded(
            child: ListView.builder(
              itemCount: filtered.length,
              padding: const EdgeInsets.symmetric(horizontal: 8),
              itemBuilder: (context, index) {
                final line = filtered[index];
                final isCounted = line.countedQuantity != null;
                final variance = line.variance;

                return Card(
                  color: isCounted ? (variance != 0 ? Colors.orange.shade50 : Colors.green.shade50) : null,
                  child: ListTile(
                    title: Text(line.inventoryItemName, style: const TextStyle(fontWeight: FontWeight.w500)),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('SKU: ${line.inventoryItemSku}', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                        Text('Система: ${line.systemQuantity.toStringAsFixed(1)}', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                        if (isCounted && variance != null)
                          Text('Расхождение: ${variance > 0 ? "+" : ""}${variance.toStringAsFixed(1)}',
                              style: TextStyle(fontSize: 12, color: variance == 0 ? Colors.green : Colors.red, fontWeight: FontWeight.w600)),
                      ],
                    ),
                    trailing: SizedBox(
                      width: 100,
                      child: TextField(
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        textAlign: TextAlign.center,
                        decoration: InputDecoration(
                          hintText: 'Кол-во',
                          isDense: true,
                          border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
                          filled: true,
                          fillColor: isCounted ? Colors.white : null,
                        ),
                        controller: _ctrlFor(line),
                        onChanged: (v) {
                          final qty = double.tryParse(v.replaceAll(',', '.'));
                          repo.updateCountedQuantity(audit.id, line.id, qty);
                          setState(() {});
                        },
                      ),
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _filterChip(String label, String value) {
    final active = _filter == value;
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 4),
      child: FilterChip(
        label: Text(label),
        selected: active,
        onSelected: (_) => setState(() => _filter = value),
      ),
    );
  }

  void _completeAudit(AuditRepository repo, AuditModel audit) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Завершить аудит?'),
        content: const Text('После завершения данные будут отправлены на сервер.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
          FilledButton(
            onPressed: () {
              Navigator.pop(ctx);
              repo.markPendingSync(audit.id);
              repo.syncAudit(audit.id);
              if (mounted) {
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
