import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../dispatching_repository.dart';

class RouteDetailScreen extends ConsumerStatefulWidget {
  final String routeId;
  const RouteDetailScreen({super.key, required this.routeId});
  @override
  ConsumerState<RouteDetailScreen> createState() => _RouteDetailScreenState();
}

class _RouteDetailScreenState extends ConsumerState<RouteDetailScreen> {
  RouteDetailModel? _route;
  bool _loading = true;

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
    } catch (_) {}
    setState(() => _loading = false);
  }

  Future<void> _startRoute() async {
    if (_route == null) return;
    try {
      await ref.read(dispatchingRepositoryProvider).startRoute(widget.routeId);
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
    }
  }

  Future<void> _completeRoute() async {
    final ok = await showDialog<bool>(context: context, builder: (ctx) => AlertDialog(
      title: const Text('Завершить маршрут?'),
      content: const Text('Все остановки будут завершены, товары отгружены.'),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
        ElevatedButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Завершить')),
      ],
    ));
    if (ok != true) return;
    try {
      await ref.read(dispatchingRepositoryProvider).completeRoute(widget.routeId);
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return Scaffold(appBar: AppBar(title: const Text('Маршрут')), body: const Center(child: CircularProgressIndicator()));
    final r = _route;
    if (r == null) return Scaffold(appBar: AppBar(title: const Text('Маршрут')), body: const Center(child: Text('Не найден')));

    return Scaffold(
      appBar: AppBar(
        title: Text(r.code),
        leading: IconButton(icon: const Icon(Icons.arrow_back), onPressed: () => context.pop()),
        actions: [
          Container(
            margin: const EdgeInsets.only(right: 8),
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            decoration: BoxDecoration(color: r.isActive ? Colors.green.shade50 : Colors.grey.shade100, borderRadius: BorderRadius.circular(8)),
            child: Text(r.statusLabel, style: TextStyle(fontSize: 12, color: r.isActive ? Colors.green : Colors.grey.shade600)),
          ),
        ],
      ),
      body: Column(
        children: [
          // Info card
          Padding(
            padding: const EdgeInsets.all(16),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Row(children: [
                    _infoTile(Icons.calendar_today, 'Дата', '${r.date.day}.${r.date.month}.${r.date.year}'),
                    const SizedBox(width: 24),
                    _infoTile(Icons.local_shipping, 'Транспорт', r.vehicleCode ?? '—'),
                  ]),
                  const SizedBox(height: 8),
                  Row(children: [
                    _infoTile(Icons.place, 'Остановок', '${r.stopCount}'),
                    if (r.driverName != null) ...[
                      const SizedBox(width: 24),
                      _infoTile(Icons.person, 'Водитель', r.driverName!),
                    ],
                  ]),
                ]),
              ),
            ),
          ),

          // Action buttons
          if (r.canStart || r.canComplete)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Row(children: [
                if (r.canStart)
                  Expanded(child: ElevatedButton.icon(onPressed: _startRoute, icon: const Icon(Icons.play_arrow), label: const Text('Начать маршрут'), style: ElevatedButton.styleFrom(backgroundColor: Colors.green, foregroundColor: Colors.white))),
                if (r.canComplete)
                  Expanded(child: ElevatedButton.icon(onPressed: _completeRoute, icon: const Icon(Icons.check), label: const Text('Завершить маршрут'), style: ElevatedButton.styleFrom(backgroundColor: Colors.blue, foregroundColor: Colors.white))),
              ]),
            ),

          const SizedBox(height: 8),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Row(children: [Text('Остановки', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Theme.of(context).colorScheme.primary))]),
          ),

          // Stop list
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: r.stops.length,
              itemBuilder: (context, i) {
                final s = r.stops[i];
                Color stopColor;
                switch (s.status) {
                  case 'Completed': stopColor = Colors.green; break;
                  case 'InProgress': stopColor = Colors.orange; break;
                  case 'Skipped': stopColor = Colors.red.shade300; break;
                  default: stopColor = Colors.grey; break;
                }
                return Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(12),
                    onTap: () => context.push('/dispatching/routes/${r.id}/stops/${s.id}'),
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Row(children: [
                        Container(width: 32, height: 32, decoration: BoxDecoration(color: stopColor.withOpacity(0.15), borderRadius: BorderRadius.circular(8)), child: Center(child: Text('${s.sequenceNumber}', style: TextStyle(fontWeight: FontWeight.bold, color: stopColor)))),
                        const SizedBox(width: 12),
                        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          Text(s.customerName ?? 'Клиент', style: const TextStyle(fontWeight: FontWeight.w500)),
                          if (s.address != null) Text(s.address!, style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                        ])),
                        Container(padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2), decoration: BoxDecoration(color: stopColor.withOpacity(0.1), borderRadius: BorderRadius.circular(6)), child: Text(s.statusLabel, style: TextStyle(fontSize: 11, color: stopColor))),
                        const SizedBox(width: 8),
                        const Icon(Icons.chevron_right, color: Colors.grey),
                      ]),
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

  Widget _infoTile(IconData icon, String label, String value) {
    return Row(mainAxisSize: MainAxisSize.min, children: [
      Icon(icon, size: 16, color: Colors.grey),
      const SizedBox(width: 4),
      Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(label, style: TextStyle(fontSize: 11, color: Colors.grey.shade500)),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w500)),
      ]),
    ]);
  }
}
