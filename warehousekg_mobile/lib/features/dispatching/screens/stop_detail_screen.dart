import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../dispatching_repository.dart';
import '../models/dispatching_models.dart';

class StopDetailScreen extends ConsumerStatefulWidget {
  final String routeId;
  final String stopId;
  const StopDetailScreen({super.key, required this.routeId, required this.stopId});
  @override
  ConsumerState<StopDetailScreen> createState() => _StopDetailScreenState();
}

class _StopDetailScreenState extends ConsumerState<StopDetailScreen> {
  RouteDetailModel? _route;
  StopModel? _stop;
  bool _loading = true;
  bool _working = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final repo = ref.read(dispatchingRepositoryProvider);
      _route = await repo.getMyRouteDetail(widget.routeId);
      _stop = _route?.stops.firstWhere((s) => s.id == widget.stopId, orElse: () => _route!.stops.first);
    } catch (_) {}
    setState(() => _loading = false);
  }

  Future<void> _arrive() async {
    setState(() => _working = true);
    try {
      await ref.read(dispatchingRepositoryProvider).arriveAtStop(widget.routeId, widget.stopId);
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
    }
    setState(() => _working = false);
  }

  Future<void> _complete() async {
    setState(() => _working = true);
    try {
      await ref.read(dispatchingRepositoryProvider).completeStop(widget.routeId, widget.stopId);
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
    }
    setState(() => _working = false);
  }

  Future<void> _skip() async {
    final reason = await showDialog<String>(context: context, builder: (ctx) {
      final ctrl = TextEditingController();
      return AlertDialog(
        title: const Text('Пропустить остановку?'),
        content: TextField(controller: ctrl, decoration: const InputDecoration(hintText: 'Причина пропуска', border: OutlineInputBorder()), maxLines: 2),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, null), child: const Text('Отмена')),
          ElevatedButton(onPressed: () => Navigator.pop(ctx, ctrl.text), style: ElevatedButton.styleFrom(backgroundColor: Colors.orange), child: const Text('Пропустить')),
        ],
      );
    });
    if (reason == null) return;
    setState(() => _working = true);
    try {
      await ref.read(dispatchingRepositoryProvider).skipStop(widget.routeId, widget.stopId, reason.isEmpty ? null : reason);
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
    }
    setState(() => _working = false);
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return Scaffold(appBar: AppBar(title: const Text('Остановка')), body: const Center(child: CircularProgressIndicator()));
    final s = _stop;
    if (s == null) return Scaffold(appBar: AppBar(title: const Text('Остановка')), body: const Center(child: Text('Не найдена')));

    Color stopColor;
    switch (s.status) {
      case 'Completed': stopColor = Colors.green; break;
      case 'InProgress': stopColor = Colors.orange; break;
      case 'Skipped': stopColor = Colors.red.shade300; break;
      default: stopColor = Colors.grey; break;
    }

    return Scaffold(
      appBar: AppBar(
        title: Text('Остановка ${s.sequenceNumber}'),
        leading: IconButton(icon: const Icon(Icons.arrow_back), onPressed: () => _working ? null : context.pop()),
        actions: [
          Container(margin: const EdgeInsets.only(right: 8), padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4), decoration: BoxDecoration(color: stopColor.withOpacity(0.1), borderRadius: BorderRadius.circular(8)), child: Text(s.statusLabel, style: TextStyle(color: stopColor, fontSize: 12))),
        ],
      ),
      body: _working
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                // Customer info
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Text(s.customerName ?? 'Клиент', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
                      if (s.address != null) ...[
                        const SizedBox(height: 4),
                        Row(children: [Icon(Icons.location_on, size: 16, color: Colors.grey.shade600), const SizedBox(width: 4), Expanded(child: Text(s.address!, style: TextStyle(color: Colors.grey.shade600)))]),
                      ],
                      if (s.latitude != null && s.longitude != null) ...[
                        const SizedBox(height: 4),
                        Text('${s.latitude!.toStringAsFixed(4)}, ${s.longitude!.toStringAsFixed(4)}', style: TextStyle(fontSize: 12, color: Colors.grey.shade400)),
                      ],
                      if (s.notes != null && s.notes!.isNotEmpty) ...[
                        const SizedBox(height: 8),
                        Container(padding: const EdgeInsets.all(8), decoration: BoxDecoration(color: Colors.orange.shade50, borderRadius: BorderRadius.circular(8)), child: Row(children: [Icon(Icons.warning_amber, size: 16, color: Colors.orange.shade700), const SizedBox(width: 8), Expanded(child: Text(s.notes!, style: TextStyle(color: Colors.orange.shade700, fontSize: 13)))]))
                      ],
                    ]),
                  ),
                ),

                const SizedBox(height: 16),

                // Shipments
                Text('Товары к доставке', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Theme.of(context).colorScheme.primary)),
                const SizedBox(height: 8),
                if (s.shipments.isEmpty)
                  Card(child: Padding(padding: const EdgeInsets.all(16), child: Text('Нет заказов для этой остановки', style: TextStyle(color: Colors.grey.shade500)))),
                ...s.shipments.map((sh) => Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: Padding(
                    padding: const EdgeInsets.all(12),
                    child: Row(children: [
                      Container(width: 40, height: 40, decoration: BoxDecoration(color: Colors.blue.shade50, borderRadius: BorderRadius.circular(8)), child: const Icon(Icons.inventory_2, color: Colors.blue)),
                      const SizedBox(width: 12),
                      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        Text(sh.salesOrderNumber, style: const TextStyle(fontWeight: FontWeight.w600)),
                        if (sh.customerName != null) Text(sh.customerName!, style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                      ])),
                      Text(sh.status, style: TextStyle(fontSize: 12, color: Colors.grey.shade500)),
                    ]),
                  ),
                )),

                const SizedBox(height: 24),

                // Actions
                if (s.canArrive || s.canComplete || s.canSkip)
                  Row(children: [
                    if (s.canArrive) Expanded(child: ElevatedButton.icon(onPressed: _arrive, icon: const Icon(Icons.play_arrow), label: const Text('Прибыл'), style: ElevatedButton.styleFrom(backgroundColor: Colors.blue, foregroundColor: Colors.white, padding: const EdgeInsets.symmetric(vertical: 14)))),
                    if (s.canArrive) const SizedBox(width: 8),
                    if (s.canComplete) Expanded(child: ElevatedButton.icon(onPressed: _complete, icon: const Icon(Icons.check), label: const Text('Доставлено'), style: ElevatedButton.styleFrom(backgroundColor: Colors.green, foregroundColor: Colors.white, padding: const EdgeInsets.symmetric(vertical: 14)))),
                    if (s.canComplete) const SizedBox(width: 8),
                    if (s.canSkip) Expanded(child: OutlinedButton.icon(onPressed: _skip, icon: const Icon(Icons.skip_next), label: const Text('Пропустить'), style: OutlinedButton.styleFrom(foregroundColor: Colors.orange, padding: const EdgeInsets.symmetric(vertical: 14)))),
                  ]),
              ]),
            ),
    );
  }
}
