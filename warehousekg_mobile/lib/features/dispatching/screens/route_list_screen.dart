import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../dispatching_repository.dart';
import '../models/dispatching_models.dart';

class RouteListScreen extends ConsumerStatefulWidget {
  const RouteListScreen({super.key});
  @override
  ConsumerState<RouteListScreen> createState() => _RouteListScreenState();
}

class _RouteListScreenState extends ConsumerState<RouteListScreen> {
  List<RouteModel>? _routes;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final repo = ref.read(dispatchingRepositoryProvider);
      _routes = await repo.getMyRoutes();
    } catch (e) {
      _error = 'Не удалось загрузить маршруты';
    }
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Мои маршруты'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/dashboard'),
        ),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 8),
                  ElevatedButton(onPressed: _load, child: const Text('Повторить')),
                ]))
              : _routes == null || _routes!.isEmpty
                  ? const Center(child: Text('Нет назначенных маршрутов', style: TextStyle(fontSize: 16, color: Colors.grey)))
                  : RefreshIndicator(
                      onRefresh: _load,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: _routes!.length,
                        itemBuilder: (context, i) {
                          final r = _routes![i];
                          return Card(
                            margin: const EdgeInsets.only(bottom: 12),
                            child: InkWell(
                              borderRadius: BorderRadius.circular(12),
                              onTap: () => context.push('/dispatching/routes/${r.id}'),
                              child: Padding(
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(children: [
                                      Text(r.code, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                                      const Spacer(),
                                      Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                        decoration: BoxDecoration(
                                          color: r.isActive ? Colors.green.shade50 : Colors.grey.shade100,
                                          borderRadius: BorderRadius.circular(8),
                                        ),
                                        child: Text(r.statusLabel, style: TextStyle(fontSize: 12, color: r.isActive ? Colors.green : Colors.grey.shade600)),
                                      ),
                                    ]),
                                    const SizedBox(height: 8),
                                    Row(children: [
                                      Icon(Icons.local_shipping, size: 16, color: Colors.grey.shade600),
                                      const SizedBox(width: 4),
                                      Text(r.vehicleCode ?? '—', style: TextStyle(color: Colors.grey.shade600)),
                                      const SizedBox(width: 16),
                                      Icon(Icons.place, size: 16, color: Colors.grey.shade600),
                                      const SizedBox(width: 4),
                                      Text('${r.stopCount} ост.', style: TextStyle(color: Colors.grey.shade600)),
                                    ]),
                                  ],
                                ),
                              ),
                            ),
                          );
                        },
                      ),
                    ),
    );
  }
}
