import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../preseller_repository.dart';
import '../models/preseller_models.dart';

class PreOrderListScreen extends ConsumerStatefulWidget {
  const PreOrderListScreen({super.key});

  @override
  ConsumerState<PreOrderListScreen> createState() => _PreOrderListScreenState();
}

class _PreOrderListScreenState extends ConsumerState<PreOrderListScreen> {
  List<PreOrderSummary> _orders = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final orders = await ref.read(presellerRepositoryProvider).getMyPreOrders();
      if (mounted) setState(() { _orders = orders; _loading = false; });
    } catch (e) {
      if (mounted) setState(() { _error = 'Ошибка загрузки: $e'; _loading = false; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Предзаказы'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/dashboard'),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          await context.push('/preseller/new');
          _load();
        },
        child: const Icon(Icons.add),
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_loading) return const Center(child: CircularProgressIndicator());
    if (_error != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(_error!, style: const TextStyle(color: Colors.red)),
            const SizedBox(height: 12),
            ElevatedButton(onPressed: _load, child: const Text('Повторить')),
          ],
        ),
      );
    }
    if (_orders.isEmpty) {
      return const Center(
        child: Text('Нет предзаказов', style: TextStyle(fontSize: 16, color: Colors.grey)),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(12),
        itemCount: _orders.length,
        itemBuilder: (ctx, i) {
          final o = _orders[i];
          return Card(
            margin: const EdgeInsets.only(bottom: 8),
            child: InkWell(
              onTap: () async {
                await context.push('/preseller/${o.id}');
                _load();
              },
              child: Padding(
                padding: const EdgeInsets.all(14),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(o.number, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                        _statusBadge(o),
                      ],
                    ),
                    const SizedBox(height: 6),
                    if (o.customerName != null)
                      Text(o.customerName!, style: const TextStyle(fontSize: 14)),
                    const SizedBox(height: 4),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('${o.lineCount} поз. · ${NumberFormat('#,##0.00').format(o.totalAmount)}'),
                        Text(DateFormat('dd.MM.yyyy').format(o.orderDateUtc),
                          style: const TextStyle(color: Colors.grey, fontSize: 12)),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _statusBadge(PreOrderSummary o) {
    final color = switch (o.status) {
      PreOrderStatus.draft => Colors.orange,
      PreOrderStatus.submitted => Colors.blue,
      PreOrderStatus.approved => Colors.green,
      PreOrderStatus.rejected => Colors.red,
      PreOrderStatus.converted => Colors.purple,
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color.shade50,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(o.statusLabel, style: TextStyle(fontSize: 12, color: color.shade700)),
    );
  }
}
