import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../preseller_repository.dart';
import '../models/preseller_models.dart';

class PreOrderFormScreen extends ConsumerStatefulWidget {
  const PreOrderFormScreen({super.key});

  @override
  ConsumerState<PreOrderFormScreen> createState() => _PreOrderFormScreenState();
}

class _PreOrderFormScreenState extends ConsumerState<PreOrderFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _numberCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();

  List<Map<String, dynamic>> _customers = [];
  List<Map<String, dynamic>> _warehouses = [];
  List<Map<String, dynamic>> _inventoryItems = [];
  List<PaymentType> _paymentTypes = [];
  List<WarehouseStockItem> _warehouseStock = [];

  String? _selectedCustomerId;
  String? _selectedWarehouseId;
  String? _selectedPaymentType;
  String _selectedCurrency = 'KGS';
  DateTime? _expectedDate;

  final List<_LineEntry> _lines = [];

  bool _loading = true;
  bool _saving = false;

  static const _currencies = ['KGS', 'USD', 'EUR', 'RUB', 'KZT', 'CNY', 'TRY'];

  @override
  void initState() {
    super.initState();
    _loadRefData();
  }

  @override
  void dispose() {
    _numberCtrl.dispose();
    _notesCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadRefData() async {
    try {
      final repo = ref.read(presellerRepositoryProvider);
      final results = await Future.wait([
        repo.getCustomers(),
        repo.getWarehouses(),
        repo.getInventoryItems(),
        repo.getPaymentTypes(),
      ]);
      if (mounted) {
        setState(() {
          _customers = results[0] as List<Map<String, dynamic>>;
          _warehouses = results[1] as List<Map<String, dynamic>>;
          _inventoryItems = results[2] as List<Map<String, dynamic>>;
          _paymentTypes = results[3] as List<PaymentType>;
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
      }
    }
  }

  Future<void> _onWarehouseChanged(String? warehouseId) async {
    _selectedWarehouseId = warehouseId;
    if (warehouseId == null) {
      setState(() => _warehouseStock = []);
      return;
    }
    try {
      final stock = await ref.read(presellerRepositoryProvider).getWarehouseStock(warehouseId);
      if (mounted) setState(() => _warehouseStock = stock);
    } catch (_) {}
  }

  double _getStockForItem(String itemId) {
    final s = _warehouseStock.where((w) => w.inventoryItemId == itemId).firstOrNull;
    return s?.quantityOnHand ?? 0;
  }

  String _getItemName(String itemId) {
    final item = _inventoryItems.where((i) => i['id'] == itemId).firstOrNull;
    if (item == null) return '';
    return '${item['name']} (${item['sku']})';
  }

  void _addLine() {
    setState(() => _lines.add(_LineEntry()));
  }

  void _removeLine(int idx) {
    setState(() => _lines.removeAt(idx));
  }

  double _getLineTotal(_LineEntry line) {
    return line.quantity * line.unitPrice * (1 - line.discountPercent / 100);
  }

  double _getGrandTotal() {
    return _lines.fold(0, (sum, l) => sum + _getLineTotal(l));
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    if (_lines.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Добавьте хотя бы один товар')),
      );
      return;
    }
    if (_selectedCustomerId == null || _selectedWarehouseId == null || _selectedPaymentType == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Заполните все обязательные поля')),
      );
      return;
    }

    setState(() => _saving = true);
    try {
      final data = {
        'number': _numberCtrl.text,
        'customerId': _selectedCustomerId,
        'warehouseId': _selectedWarehouseId,
        'paymentType': _selectedPaymentType,
        'currency': _selectedCurrency,
        'expectedDateUtc': _expectedDate?.toIso8601String(),
        'notes': _notesCtrl.text.isEmpty ? null : _notesCtrl.text,
        'lines': _lines.map((l) => {
          'inventoryItemId': l.itemId,
          'quantity': l.quantity,
          'unitPrice': l.unitPrice,
          'discountPercent': l.discountPercent,
        }).toList(),
      };
      await ref.read(presellerRepositoryProvider).createPreOrder(data);
      if (mounted) context.pop();
    } catch (e) {
      if (mounted) {
        setState(() => _saving = false);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Ошибка: $e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Новый предзаказ')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _numberCtrl,
              decoration: const InputDecoration(labelText: 'Номер', border: OutlineInputBorder()),
              validator: (v) => (v == null || v.isEmpty) ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            // Customer dropdown
            DropdownButtonFormField<String>(
              initialValue: _selectedCustomerId,
              decoration: const InputDecoration(labelText: 'Клиент', border: OutlineInputBorder()),
              items: _customers.map((c) => DropdownMenuItem(
                value: c['id'] as String,
                child: Text(c['name'] ?? ''),
              )).toList(),
              onChanged: (v) => setState(() => _selectedCustomerId = v),
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            // Warehouse dropdown
            DropdownButtonFormField<String>(
              initialValue: _selectedWarehouseId,
              decoration: const InputDecoration(labelText: 'Склад', border: OutlineInputBorder()),
              items: _warehouses.map((w) => DropdownMenuItem(
                value: w['id'] as String,
                child: Text(w['name'] ?? ''),
              )).toList(),
              onChanged: (v) {
                setState(() => _selectedWarehouseId = v);
                _onWarehouseChanged(v);
              },
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            // Payment type dropdown
            DropdownButtonFormField<String>(
              initialValue: _selectedPaymentType,
              decoration: const InputDecoration(labelText: 'Тип оплаты', border: OutlineInputBorder()),
              items: _paymentTypes.map((p) => DropdownMenuItem(
                value: p.name,
                child: Text(p.name),
              )).toList(),
              onChanged: (v) => setState(() => _selectedPaymentType = v),
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            // Currency
            DropdownButtonFormField<String>(
              initialValue: _selectedCurrency,
              decoration: const InputDecoration(labelText: 'Валюта', border: OutlineInputBorder()),
              items: _currencies.map((c) => DropdownMenuItem(value: c, child: Text(c))).toList(),
              onChanged: (v) => setState(() => _selectedCurrency = v!),
            ),
            const SizedBox(height: 12),

            // Expected date
            ListTile(
              title: const Text('Ожидаемая дата'),
              subtitle: Text(_expectedDate != null
                  ? DateFormat('dd.MM.yyyy').format(_expectedDate!)
                  : 'Не выбрана'),
              trailing: const Icon(Icons.calendar_today),
              onTap: () async {
                final d = await showDatePicker(
                  context: context,
                  initialDate: _expectedDate ?? DateTime.now(),
                  firstDate: DateTime.now(),
                  lastDate: DateTime.now().add(const Duration(days: 365)),
                );
                if (d != null) setState(() => _expectedDate = d);
              },
            ),
            const SizedBox(height: 12),

            TextFormField(
              controller: _notesCtrl,
              decoration: const InputDecoration(labelText: 'Примечания', border: OutlineInputBorder()),
              maxLines: 2,
            ),
            const SizedBox(height: 20),

            // Lines header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text('Товары', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                TextButton.icon(
                  onPressed: _addLine,
                  icon: const Icon(Icons.add),
                  label: const Text('Добавить'),
                ),
              ],
            ),

            // Lines list
            ...List.generate(_lines.length, (i) {
              final line = _lines[i];
              final stock = _getStockForItem(line.itemId);
              final diff = stock - line.quantity;
              return Card(
                margin: const EdgeInsets.only(bottom: 8),
                child: Padding(
                  padding: const EdgeInsets.all(10),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Expanded(
                            child: DropdownButtonFormField<String>(
                              initialValue: line.itemId.isEmpty ? null : line.itemId,
                              decoration: const InputDecoration(
                                labelText: 'Товар',
                                border: OutlineInputBorder(),
                                isDense: true,
                              ),
                              items: _inventoryItems.map((it) => DropdownMenuItem(
                                value: it['id'] as String,
                                child: Text('${it['name']} (${it['sku']})',
                                  style: const TextStyle(fontSize: 13)),
                              )).toList(),
                              onChanged: (v) => setState(() => line.itemId = v ?? ''),
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.delete, color: Colors.red),
                            onPressed: () => _removeLine(i),
                          ),
                        ],
                      ),
                      if (line.itemId.isNotEmpty) ...[
                        Text('На складе: ${stock.toStringAsFixed(0)}',
                          style: const TextStyle(fontSize: 12, color: Colors.grey)),
                      ],
                      const SizedBox(height: 6),
                      Row(
                        children: [
                          SizedBox(
                            width: 80,
                            child: TextFormField(
                              initialValue: line.quantity.toString(),
                              decoration: const InputDecoration(labelText: 'Кол-во', isDense: true),
                              keyboardType: TextInputType.number,
                              onChanged: (v) {
                                final q = double.tryParse(v) ?? 0;
                                setState(() => line.quantity = q);
                              },
                            ),
                          ),
                          const SizedBox(width: 8),
                          Text('Разница: ${diff.toStringAsFixed(0)}',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.bold,
                              color: diff >= 0 ? Colors.green : Colors.red,
                            )),
                          const SizedBox(width: 8),
                          SizedBox(
                            width: 80,
                            child: TextFormField(
                              initialValue: line.unitPrice.toString(),
                              decoration: const InputDecoration(labelText: 'Цена', isDense: true),
                              keyboardType: TextInputType.number,
                              onChanged: (v) {
                                final p = double.tryParse(v) ?? 0;
                                setState(() => line.unitPrice = p);
                              },
                            ),
                          ),
                          const SizedBox(width: 8),
                          Text(_getLineTotal(line).toStringAsFixed(2),
                            style: const TextStyle(fontWeight: FontWeight.bold)),
                        ],
                      ),
                    ],
                  ),
                ),
              );
            }),

            if (_lines.isNotEmpty) ...[
              const SizedBox(height: 16),
              Text('Итого: ${NumberFormat('#,##0.00').format(_getGrandTotal())}',
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                textAlign: TextAlign.right,
              ),
            ],

            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                onPressed: _saving ? null : _save,
                style: ElevatedButton.styleFrom(
                  backgroundColor: Theme.of(context).primaryColor,
                  foregroundColor: Colors.white,
                ),
                child: _saving
                    ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : const Text('Сохранить', style: TextStyle(fontSize: 16)),
              ),
            ),
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }
}

class _LineEntry {
  String itemId = '';
  double quantity = 1;
  double unitPrice = 0;
  double discountPercent = 0;
}
