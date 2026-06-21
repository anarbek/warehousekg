import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../preseller_repository.dart';
import '../models/preseller_models.dart';

class PreOrderFormScreen extends ConsumerStatefulWidget {
  final String? editId;

  const PreOrderFormScreen({super.key, this.editId});

  @override
  ConsumerState<PreOrderFormScreen> createState() => _PreOrderFormScreenState();
}

class _PreOrderFormScreenState extends ConsumerState<PreOrderFormScreen> {
  bool get _isEdit => widget.editId != null;
  final _formKey = GlobalKey<FormState>();
  final _numberCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();
  final _amountPlannedCtrl = TextEditingController();
  final _amountPaidCtrl = TextEditingController();
  final _amountPlannedFocus = FocusNode();
  final _amountPaidFocus = FocusNode();

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
    _amountPlannedCtrl.dispose();
    _amountPaidCtrl.dispose();
    _amountPlannedFocus.dispose();
    _amountPaidFocus.dispose();
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
        // If editing, load existing pre-order data
        if (_isEdit) await _loadExistingOrder();
      }
    } catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Ошибка: $e')));
      }
    }
  }

  Future<void> _loadExistingOrder() async {
    try {
      final order = await ref.read(presellerRepositoryProvider).getPreOrderById(widget.editId!);
      if (!mounted) return;
      setState(() {
        _numberCtrl.text = order.number;
        _selectedCustomerId = order.customerId;
        _selectedWarehouseId = order.warehouseId;
        _selectedPaymentType = order.paymentType;
        _selectedCurrency = order.currency;
        _expectedDate = order.expectedDateUtc;
        _notesCtrl.text = order.notes ?? '';
        _amountPlannedCtrl.text = order.amountPlanned > 0 ? order.amountPlanned.toString() : '';
        _amountPaidCtrl.text = order.amountPaid > 0 ? order.amountPaid.toString() : '';
        _lines.clear();
        _lines.addAll(order.lines.map((l) => _LineEntry()
          ..itemId = l.inventoryItemId
          ..quantity = l.quantity
          ..unitPrice = l.unitPrice));
      });
      // Load warehouse stock for the selected warehouse
      _onWarehouseChanged(order.warehouseId);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Ошибка загрузки: $e')));
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
      final stock = await ref
          .read(presellerRepositoryProvider)
          .getWarehouseStock(warehouseId);
      if (mounted) setState(() => _warehouseStock = stock);
    } catch (_) {}
  }

  String _getItemName(String itemId) {
    final item =
        _inventoryItems.where((i) => i['id'] == itemId).firstOrNull;
    if (item == null) return '';
    return '${item['name']} (${item['sku']})';
  }

  double _getLineTotal(_LineEntry line) {
    return line.quantity * line.unitPrice * (1 - line.discountPercent / 100);
  }

  double _getGrandTotal() {
    return _lines.fold(0, (sum, l) => sum + _getLineTotal(l));
  }

  void _removeLine(int idx) {
    setState(() => _lines.removeAt(idx));
  }

  Future<void> _openItemModal() async {
    final result = await showModalBottomSheet<List<_LineEntry>>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) => _ItemSearchSheet(
        inventoryItems: _inventoryItems,
        warehouseStock: _warehouseStock,
        existingLines: List<_LineEntry>.from(_lines),
      ),
    );
    if (result != null && mounted) {
      setState(() => _lines.clear());
      setState(() => _lines.addAll(result));
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    if (_lines.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Добавьте хотя бы один товар')),
      );
      return;
    }
    if (_selectedCustomerId == null ||
        _selectedWarehouseId == null ||
        _selectedPaymentType == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Заполните все обязательные поля')),
      );
      return;
    }

    setState(() => _saving = true);
    try {
      final amountPlanned =
          double.tryParse(_amountPlannedCtrl.text.replaceAll(',', '.')) ?? 0;
      final amountPaid =
          double.tryParse(_amountPaidCtrl.text.replaceAll(',', '.')) ?? 0;

      final data = <String, dynamic>{
        if (_isEdit) 'id': widget.editId,
        'number': _numberCtrl.text,
        'customerId': _selectedCustomerId,
        'warehouseId': _selectedWarehouseId,
        'paymentType': _selectedPaymentType,
        'currency': _selectedCurrency,
        'expectedDateUtc': _expectedDate?.toIso8601String(),
        'notes':
            _notesCtrl.text.isEmpty ? null : _notesCtrl.text,
        'amountPlanned': amountPlanned,
        'amountPaid': amountPaid,
        'lines': _lines
            .map((l) => {
                  'inventoryItemId': l.itemId,
                  'quantity': l.quantity,
                  'unitPrice': l.unitPrice,
                  'discountPercent': l.discountPercent,
                })
            .toList(),
      };
      if (_isEdit) {
        await ref.read(presellerRepositoryProvider).updatePreOrder(widget.editId!, data);
      } else {
        await ref.read(presellerRepositoryProvider).createPreOrder(data);
      }
      if (mounted) context.pop();
    } catch (e) {
      if (mounted) {
        setState(() => _saving = false);
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Ошибка: $e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(
          body: Center(child: CircularProgressIndicator()));
    }

    final grandTotal = _getGrandTotal();

    return Scaffold(
      appBar: AppBar(title: Text(_isEdit ? 'Редактировать предзаказ' : 'Новый предзаказ')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _numberCtrl,
              decoration: const InputDecoration(
                  labelText: 'Номер', border: OutlineInputBorder()),
              validator: (v) =>
                  (v == null || v.isEmpty) ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            DropdownButtonFormField<String>(
              initialValue: _selectedCustomerId,
              decoration: const InputDecoration(
                  labelText: 'Клиент', border: OutlineInputBorder()),
              items: _customers
                  .map((c) => DropdownMenuItem(
                        value: c['id'] as String,
                        child: Text(c['name'] ?? ''),
                      ))
                  .toList(),
              onChanged: (v) => setState(() => _selectedCustomerId = v),
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            DropdownButtonFormField<String>(
              initialValue: _selectedWarehouseId,
              decoration: const InputDecoration(
                  labelText: 'Склад', border: OutlineInputBorder()),
              items: _warehouses
                  .map((w) => DropdownMenuItem(
                        value: w['id'] as String,
                        child: Text(w['name'] ?? ''),
                      ))
                  .toList(),
              onChanged: (v) {
                setState(() => _selectedWarehouseId = v);
                _onWarehouseChanged(v);
              },
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            DropdownButtonFormField<String>(
              initialValue: _selectedPaymentType,
              decoration: const InputDecoration(
                  labelText: 'Тип оплаты', border: OutlineInputBorder()),
              items: _paymentTypes
                  .map((p) => DropdownMenuItem(
                        value: p.name,
                        child: Text(p.name),
                      ))
                  .toList(),
              onChanged: (v) => setState(() => _selectedPaymentType = v),
              validator: (v) => v == null ? 'Обязательно' : null,
            ),
            const SizedBox(height: 12),

            DropdownButtonFormField<String>(
              initialValue: _selectedCurrency,
              decoration: const InputDecoration(
                  labelText: 'Валюта', border: OutlineInputBorder()),
              items: _currencies
                  .map((c) =>
                      DropdownMenuItem(value: c, child: Text(c)))
                  .toList(),
              onChanged: (v) => setState(() => _selectedCurrency = v!),
            ),
            const SizedBox(height: 12),

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
              decoration: const InputDecoration(
                  labelText: 'Примечания', border: OutlineInputBorder()),
              maxLines: 2,
            ),
            const SizedBox(height: 20),

            // Items section
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Товары (${_lines.length})',
                  style: const TextStyle(
                      fontSize: 18, fontWeight: FontWeight.bold),
                ),
                TextButton.icon(
                  onPressed: _openItemModal,
                  icon: const Icon(Icons.add),
                  label: const Text('Добавить'),
                ),
              ],
            ),

            if (_lines.isEmpty)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 24),
                child: Center(
                  child: Text('Нет добавленных товаров',
                      style: TextStyle(color: Colors.grey)),
                ),
              )
            else ...[
              const SizedBox(height: 8),
              _buildItemsGrid(),
              const SizedBox(height: 8),
              Align(
                alignment: Alignment.centerRight,
                child: Text(
                  'Итого: ${NumberFormat('#,##0.00').format(grandTotal)}',
                  style: const TextStyle(
                      fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ),
            ],

            const SizedBox(height: 20),

            // Payment tracking
            TextFormField(
              controller: _amountPlannedCtrl,
              focusNode: _amountPlannedFocus,
              decoration: const InputDecoration(
                  labelText: 'Запланированная сумма',
                  border: OutlineInputBorder(),
                  suffixText: 'KGS'),
              keyboardType:
                  const TextInputType.numberWithOptions(decimal: true),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'[\d.,]')),
              ],
              onTap: () {
                _amountPlannedCtrl.selection = TextSelection(
                  baseOffset: 0,
                  extentOffset: _amountPlannedCtrl.text.length,
                );
              },
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _amountPaidCtrl,
              focusNode: _amountPaidFocus,
              decoration: const InputDecoration(
                  labelText: 'Оплаченная сумма',
                  border: OutlineInputBorder(),
                  suffixText: 'KGS'),
              keyboardType:
                  const TextInputType.numberWithOptions(decimal: true),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'[\d.,]')),
              ],
              onTap: () {
                _amountPaidCtrl.selection = TextSelection(
                  baseOffset: 0,
                  extentOffset: _amountPaidCtrl.text.length,
                );
              },
            ),

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
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white))
                    : Text(_isEdit ? 'Сохранить изменения' : 'Сохранить',
                        style: TextStyle(fontSize: 16)),
              ),
            ),
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }

  Widget _buildItemsGrid() {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(8),
        child: Column(
          children: [
            Row(
              children: [
                const Expanded(flex: 3, child: Text('Товар', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12))),
                const SizedBox(width: 4),
                const SizedBox(width: 44, child: Text('Кол-во', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12), textAlign: TextAlign.center)),
                const SizedBox(width: 4),
                const SizedBox(width: 52, child: Text('Цена', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12), textAlign: TextAlign.right)),
                const SizedBox(width: 4),
                const SizedBox(width: 56, child: Text('Сумма', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12), textAlign: TextAlign.right)),
                const SizedBox(width: 4),
                const SizedBox(width: 32),
              ],
            ),
            const Divider(height: 4),
            ...List.generate(_lines.length, (i) {
              final line = _lines[i];
              final itemName = _getItemName(line.itemId);
              return Padding(
                padding: const EdgeInsets.symmetric(vertical: 4),
                child: Row(
                  children: [
                    Expanded(
                      flex: 3,
                      child: Text(itemName.isEmpty ? '-' : itemName,
                          style: const TextStyle(fontSize: 12)),
                    ),
                    const SizedBox(width: 4),
                    SizedBox(
                      width: 44,
                      child: Text(
                        line.quantity.toStringAsFixed(line.quantity == line.quantity.truncateToDouble() ? 0 : 1),
                        style: const TextStyle(fontSize: 12),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    const SizedBox(width: 4),
                    SizedBox(
                      width: 52,
                      child: Text(
                        NumberFormat('#,##0.00').format(line.unitPrice),
                        style: const TextStyle(fontSize: 12),
                        textAlign: TextAlign.right,
                      ),
                    ),
                    const SizedBox(width: 4),
                    SizedBox(
                      width: 56,
                      child: Text(
                        NumberFormat('#,##0.00').format(_getLineTotal(line)),
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
                        textAlign: TextAlign.right,
                      ),
                    ),
                    const SizedBox(width: 4),
                    SizedBox(
                      width: 32,
                      child: IconButton(
                        icon: const Icon(Icons.delete, color: Colors.red, size: 18),
                        padding: EdgeInsets.zero,
                        constraints: const BoxConstraints(),
                        onPressed: () => _removeLine(i),
                      ),
                    ),
                  ],
                ),
              );
            }),
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

// ─── Item Search Modal ───────────────────────────────────────────────────

class _ItemSearchSheet extends StatefulWidget {
  final List<Map<String, dynamic>> inventoryItems;
  final List<WarehouseStockItem> warehouseStock;
  final List<_LineEntry> existingLines;

  const _ItemSearchSheet({
    required this.inventoryItems,
    required this.warehouseStock,
    required this.existingLines,
  });

  @override
  State<_ItemSearchSheet> createState() => _ItemSearchSheetState();
}

class _ItemSearchSheetState extends State<_ItemSearchSheet> {
  final _skuCtrl = TextEditingController();
  final _nameCtrl = TextEditingController();
  final _qtyCtrl = TextEditingController(text: '1');
  final _priceCtrl = TextEditingController();
  final _discountCtrl = TextEditingController(text: '0');
  final _qtyFocus = FocusNode();
  final _priceFocus = FocusNode();
  final _discountFocus = FocusNode();

  String? _selectedItemId;
  double _selectedStock = 0;
  double _selectedPrice = 0;
  double _selectedDiscount = 0;
  final List<_LineEntry> _localLines = [];

  @override
  void initState() {
    super.initState();
    _localLines.addAll(widget.existingLines);
  }

  @override
  void dispose() {
    _skuCtrl.dispose();
    _nameCtrl.dispose();
    _qtyCtrl.dispose();
    _priceCtrl.dispose();
    _discountCtrl.dispose();
    _qtyFocus.dispose();
    _priceFocus.dispose();
    _discountFocus.dispose();
    super.dispose();
  }

  void _onItemSelected(String itemId) {
    final item = widget.inventoryItems
        .where((i) => i['id'] == itemId)
        .firstOrNull;
    if (item == null) return;

    final stock = widget.warehouseStock
        .where((w) => w.inventoryItemId == itemId)
        .firstOrNull
        ?.quantityOnHand ??
        0;

    final price = (item['unitPrice'] ?? 0).toDouble();

    setState(() {
      _selectedItemId = itemId;
      _selectedStock = stock;
      _selectedPrice = price;
      _priceCtrl.text = price > 0 ? NumberFormat('#,##0.00').format(price) : '';
      _skuCtrl.text = item['sku'] ?? '';
      _nameCtrl.text = '${item['name'] ?? ''} (${item['sku'] ?? ''})';
    });
    FocusScope.of(context).unfocus();
  }

  void _addCurrentItem() {
    if (_selectedItemId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Select an item')),
      );
      return;
    }
    final qty = double.tryParse(_qtyCtrl.text.replaceAll(',', '.')) ?? 0;
    if (qty <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Enter quantity')),
      );
      return;
    }
    final price =
        double.tryParse(_priceCtrl.text.replaceAll(',', '.')) ??
        _selectedPrice;
    final discount =
        double.tryParse(_discountCtrl.text.replaceAll(',', '.')) ?? 0;

    final existing = _localLines
        .where((l) => l.itemId == _selectedItemId)
        .firstOrNull;
    if (existing != null) {
      setState(() {
        existing.quantity += qty;
        existing.unitPrice = price > 0 ? price : existing.unitPrice;
        existing.discountPercent = discount;
      });
    } else {
      setState(() {
        _localLines.add(_LineEntry()
          ..itemId = _selectedItemId!
          ..quantity = qty
          ..unitPrice = price > 0 ? price : 0
          ..discountPercent = discount);
      });
    }

    setState(() {
      _selectedItemId = null;
      _selectedStock = 0;
      _selectedPrice = 0;
      _selectedDiscount = 0;
      _skuCtrl.clear();
      _nameCtrl.clear();
      _priceCtrl.clear();
      _discountCtrl.text = '0';
      _qtyCtrl.text = '1';
    });
    FocusScope.of(context).unfocus();
  }

  double _getLocalGrandTotal() {
    return _localLines.fold(
        0, (sum, l) => sum + l.quantity * l.unitPrice * (1 - l.discountPercent / 100));
  }

  @override
  Widget build(BuildContext context) {
    final grandTotal = _getLocalGrandTotal();

    return DraggableScrollableSheet(
      initialChildSize: 0.9,
      minChildSize: 0.5,
      maxChildSize: 0.95,
      expand: false,
      builder: (ctx, scrollCtrl) => GestureDetector(
        onTap: () => FocusScope.of(context).unfocus(),
        child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            // Fixed top: handle bar + title + search fields
            Center(
              child: Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            const SizedBox(height: 12),

            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text('Добавить товар',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                TextButton(
                    onPressed: () => Navigator.pop(context, _localLines),
                    child: const Text('Готово')),
              ],
            ),
            const SizedBox(height: 12),

            _AutocompleteField(
              controller: _skuCtrl,
              label: 'Код (SKU)',
              options: widget.inventoryItems
                  .map((i) => '${i['sku']} - ${i['name']}')
                  .toList(),
              onSelected: (val) {
                final sku = val.split(' - ').first;
                final item = widget.inventoryItems
                    .where((i) => i['sku'] == sku)
                    .firstOrNull;
                if (item != null) _onItemSelected(item['id'] as String);
              },
            ),
            const SizedBox(height: 8),
            _AutocompleteField(
              controller: _nameCtrl,
              label: 'Название',
              options: widget.inventoryItems
                  .map((i) => '${i['name']} (${i['sku']})')
                  .toList(),
              onSelected: (val) {
                final item = widget.inventoryItems
                    .where((i) => '${i['name']} (${i['sku']})' == val)
                    .firstOrNull;
                if (item != null) _onItemSelected(item['id'] as String);
              },
            ),

            // Stock info + price/qty + add button (always visible)
            const SizedBox(height: 12),
            Text(
              _selectedItemId != null
                  ? 'На складе: ${_selectedStock.toStringAsFixed(0)}'
                  : 'На складе: —',
              style: const TextStyle(fontSize: 13, color: Colors.grey),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(
                  child: TextFormField(
                    controller: _priceCtrl,
                    focusNode: _priceFocus,
                    decoration: const InputDecoration(
                      labelText: 'Цена',
                      border: OutlineInputBorder(),
                      isDense: true,
                    ),
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[\d.,]')),
                    ],
                    onTap: () {
                      _priceCtrl.selection = TextSelection(
                        baseOffset: 0,
                        extentOffset: _priceCtrl.text.length,
                      );
                    },
                    onChanged: (v) {
                      final p = double.tryParse(v.replaceAll(',', '.')) ?? 0;
                      setState(() => _selectedPrice = p);
                    },
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  width: 80,
                  child: TextFormField(
                    controller: _qtyCtrl,
                    focusNode: _qtyFocus,
                    decoration: const InputDecoration(
                      labelText: 'Кол-во',
                      border: OutlineInputBorder(),
                      isDense: true,
                    ),
                    keyboardType: TextInputType.number,
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[\d.,]')),
                    ],
                    onTap: () {
                      _qtyCtrl.selection = TextSelection(
                        baseOffset: 0,
                        extentOffset: _qtyCtrl.text.length,
                      );
                    },
                    onChanged: (_) => setState(() {}),
                  ),
                ),
              ],
            ),
            // Discount + sum (always visible when item selected)
            const SizedBox(height: 4),
            Row(
              children: [
                SizedBox(
                  width: 80,
                  child: TextFormField(
                    controller: _discountCtrl,
                    focusNode: _discountFocus,
                    decoration: const InputDecoration(
                      labelText: 'Скидка %',
                      border: OutlineInputBorder(),
                      isDense: true,
                    ),
                    keyboardType: TextInputType.number,
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'[\d.,]')),
                    ],
                    onTap: () {
                      _discountCtrl.selection = TextSelection(
                        baseOffset: 0,
                        extentOffset: _discountCtrl.text.length,
                      );
                    },
                    onChanged: (v) {
                      final d = double.tryParse(v.replaceAll(',', '.')) ?? 0;
                      setState(() => _selectedDiscount = d);
                    },
                  ),
                ),
                const Spacer(),
                Text(
                  'Сумма: ${NumberFormat('#,##0.00').format(_selectedPrice * (double.tryParse(_qtyCtrl.text.replaceAll(',', '.')) ?? 1) * (1 - _selectedDiscount / 100))}',
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                ),
              ],
            ),
            const SizedBox(height: 8),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _selectedItemId != null ? _addCurrentItem : null,
                icon: const Icon(Icons.add),
                label: const Text('Добавить'),
              ),
            ),

            const SizedBox(height: 16),

            if (_localLines.isNotEmpty) ...[
              const Text('Добавленные товары',
                  style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold)),
              const SizedBox(height: 4),
              Expanded(
                child: ListView(
                  controller: scrollCtrl,
                  shrinkWrap: true,
                  children: [
                    _buildLocalGrid(),
                    const SizedBox(height: 4),
                    Align(
                      alignment: Alignment.centerRight,
                      child: Text(
                        'Итого: ${_localLines.length} поз., ${NumberFormat('#,##0.00').format(grandTotal)}',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
    ));
  }

  Widget _buildLocalGrid() {
    return Card(
      color: Colors.grey[50],
      child: Padding(
        padding: const EdgeInsets.all(6),
        child: Column(
          children: _localLines.asMap().entries.map((entry) {
            final i = entry.key;
            final line = entry.value;
            final item = widget.inventoryItems
                .where((it) => it['id'] == line.itemId)
                .firstOrNull;
            final name = item != null ? '${item['name']} (${item['sku']})' : '-';
            return Padding(
              padding: const EdgeInsets.symmetric(vertical: 2),
              child: Row(
                children: [
                  Expanded(
                    child: Text(name, style: const TextStyle(fontSize: 12)),
                  ),
                  Text(' x${line.quantity.toStringAsFixed(0)}',
                      style: const TextStyle(fontSize: 12)),
                  const SizedBox(width: 4),
                  Text(
                    '= ${NumberFormat('#,##0.00').format(line.quantity * line.unitPrice * (1 - line.discountPercent / 100))}',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
                  ),
                  SizedBox(
                    width: 28,
                    child: IconButton(
                      icon: const Icon(Icons.close, color: Colors.red, size: 16),
                      padding: EdgeInsets.zero,
                      constraints: const BoxConstraints(),
                      onPressed: () => setState(() => _localLines.removeAt(i)),
                    ),
                  ),
                ],
              ),
            );
          }).toList(),
        ),
      ),
    );
  }
}

class _AutocompleteField extends StatelessWidget {
  final TextEditingController controller;
  final String label;
  final List<String> options;
  final ValueChanged<String> onSelected;

  const _AutocompleteField({
    required this.controller,
    required this.label,
    required this.options,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    return Autocomplete<String>(
      optionsBuilder: (textEditingValue) {
        if (textEditingValue.text.isEmpty) return options;
        final q = textEditingValue.text.toLowerCase();
        return options.where((o) => o.toLowerCase().contains(q));
      },
      onSelected: onSelected,
      fieldViewBuilder: (ctx, ctrl, focusNode, onSubmit) {
        ctrl.text = controller.text;
        ctrl.addListener(() => controller.text = ctrl.text);
        return TextFormField(
          controller: ctrl,
          focusNode: focusNode,
          decoration: InputDecoration(
            labelText: label,
            border: const OutlineInputBorder(),
            isDense: true,
          ),
          onFieldSubmitted: (_) => onSubmit(),
        );
      },
      optionsViewBuilder: (ctx, onSel, opts) {
        return Align(
          alignment: Alignment.topLeft,
          child: Material(
            elevation: 4,
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxHeight: 200),
              child: ListView.builder(
                padding: EdgeInsets.zero,
                shrinkWrap: true,
                itemCount: opts.length,
                itemBuilder: (ctx, i) {
                  return ListTile(
                    dense: true,
                    title: Text(opts.elementAt(i),
                        style: const TextStyle(fontSize: 13)),
                    onTap: () => onSel(opts.elementAt(i)),
                  );
                },
              ),
            ),
          ),
        );
      },
    );
  }
}
