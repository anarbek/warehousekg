import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../preseller_repository.dart';
import '../models/preseller_models.dart';

class PreOrderDetailScreen extends ConsumerStatefulWidget {
  final String id;

  const PreOrderDetailScreen({super.key, required this.id});

  @override
  ConsumerState<PreOrderDetailScreen> createState() => _PreOrderDetailScreenState();
}

class _PreOrderDetailScreenState extends ConsumerState<PreOrderDetailScreen> {
  PreOrderModel? _order;
  bool _loading = true;
  bool _working = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final order = await ref.read(presellerRepositoryProvider).getPreOrderById(widget.id);
      if (mounted) setState(() { _order = order; _loading = false; });
    } catch (e) {
      if (mounted) setState(() { _error = 'Ошибка: $e'; _loading = false; });
    }
  }

  Future<void> _submit() async {
    setState(() => _working = true);
    try {
      await ref.read(presellerRepositoryProvider).submitPreOrder(widget.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Предзаказ отправлен на согласование')),
        );
        _load();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
        setState(() => _working = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_order?.number ?? 'Предзаказ'),
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_loading) return const Center(child: CircularProgressIndicator());
    if (_error != null) return Center(child: Text(_error!, style: const TextStyle(color: Colors.red)));

    final o = _order!;
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Status
          Center(child: _statusChip(o)),
          const SizedBox(height: 16),

          // Summary cards
          _infoRow('Клиент', o.customerName ?? '-'),
          _infoRow('Продавец', o.presellerName ?? '-'),
          _infoRow('Склад', o.warehouseName ?? '-'),
          _infoRow('Тип оплаты', o.paymentType),
          _infoRow('Валюта', o.currency),
          _infoRow('Дата', DateFormat('dd.MM.yyyy').format(o.orderDateUtc)),
          if (o.expectedDateUtc != null)
            _infoRow('Ожидаемая дата', DateFormat('dd.MM.yyyy').format(o.expectedDateUtc!)),
          _infoRow('Сумма', '${NumberFormat('#,##0.00').format(o.totalAmount)} ${o.currency}'),
          if (o.notes != null && o.notes!.isNotEmpty)
            _infoRow('Примечания', o.notes!),
          if (o.convertedSalesOrderNumber != null)
            _infoRow('Заказ на продажу', o.convertedSalesOrderNumber!),

          // Submit button (draft only)
          if (o.status == PreOrderStatus.draft) ...[
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _working ? null : _submit,
                icon: _working
                    ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Icon(Icons.send),
                label: const Text('Отправить на согласование'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.blue,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                ),
              ),
            ),
          ],

          const SizedBox(height: 20),
          const Text('Товары', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),

          // Lines
          ...o.lines.map((line) => Card(
            margin: const EdgeInsets.only(bottom: 8),
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(line.inventoryItemName ?? 'Товар', style: const TextStyle(fontWeight: FontWeight.bold)),
                  if (line.sku != null) Text('SKU: ${line.sku}', style: const TextStyle(color: Colors.grey, fontSize: 13)),
                  const SizedBox(height: 8),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('На складе: ${line.warehouseStockSnapshot.toStringAsFixed(0)}'),
                      Text('Кол-во: ${line.quantity.toStringAsFixed(0)}'),
                    ],
                  ),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('Разница: ${line.stockDifference.toStringAsFixed(0)}',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: line.stockDifference >= 0 ? Colors.green : Colors.red,
                        )),
                      Text(NumberFormat('#,##0.00').format(line.lineTotal),
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                    ],
                  ),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text('Цена: ${NumberFormat('#,##0.00').format(line.unitPrice)}'),
                      Text('Скидка: ${line.discountPercent.toStringAsFixed(0)}%',
                        style: const TextStyle(color: Colors.grey, fontSize: 13)),
                    ],
                  ),
                ],
              ),
            ),
          )),
        ],
      ),
    );
  }

  Widget _statusChip(PreOrderModel o) {
    final color = switch (o.status) {
      PreOrderStatus.draft => Colors.orange,
      PreOrderStatus.submitted => Colors.blue,
      PreOrderStatus.approved => Colors.green,
      PreOrderStatus.rejected => Colors.red,
      PreOrderStatus.converted => Colors.purple,
    };
    String label;
    switch (o.status) {
      case PreOrderStatus.draft: label = 'Черновик'; break;
      case PreOrderStatus.submitted: label = 'Отправлен'; break;
      case PreOrderStatus.approved: label = 'Одобрен'; break;
      case PreOrderStatus.rejected: label = 'Отклонён'; break;
      case PreOrderStatus.converted: label = 'Конвертирован'; break;
    }
    return Chip(
      avatar: Icon(Icons.circle, size: 10, color: color),
      label: Text(label, style: TextStyle(color: color.shade700)),
      backgroundColor: color.shade50,
    );
  }

  Widget _infoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(color: Colors.grey)),
          Flexible(child: Text(value, textAlign: TextAlign.right,
            style: const TextStyle(fontWeight: FontWeight.w500))),
        ],
      ),
    );
  }
}
