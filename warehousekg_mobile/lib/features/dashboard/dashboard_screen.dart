import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../auth/auth_repository.dart';
import '../../core/network/connectivity.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  static final _menuItems = [
    (_Menu.warehouse, 'Склад', Icons.warehouse),
    (_Menu.items, 'Товары', Icons.inventory_2),
    (_Menu.receipts, 'Поступления', Icons.receipt_long),
    (_Menu.dispatching, 'Доставка', Icons.local_shipping),
    (_Menu.transfers, 'Перемещения', Icons.swap_horiz),
    (_Menu.audit, 'Аудиты', Icons.checklist),
    (_Menu.reports, 'Отчёты', Icons.bar_chart),
  ];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authProvider);
    final isOnline = ref.watch(isOnlineProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('WarehouseKG'),
        actions: [
          if (!isOnline)
            const Padding(
              padding: EdgeInsets.only(right: 8),
              child: Icon(Icons.cloud_off, color: Colors.orange, size: 20),
            ),
          if (auth.userName != null)
            Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Center(child: Text(auth.userName!, style: const TextStyle(fontSize: 14))),
            ),
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Выйти',
            onPressed: () => ref.read(authProvider.notifier).logout(),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: GridView.builder(
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            crossAxisSpacing: 12,
            mainAxisSpacing: 12,
            childAspectRatio: 1.3,
          ),
          itemCount: _menuItems.length,
          itemBuilder: (context, index) {
            final (menu, title, icon) = _menuItems[index];
            final enabled = menu == _Menu.audit || menu == _Menu.dispatching;
            return Card(
              color: enabled ? null : Colors.grey.shade200,
              child: InkWell(
                borderRadius: BorderRadius.circular(12),
                onTap: enabled
                    ? () {
                        if (menu == _Menu.audit) context.push('/audits');
                        if (menu == _Menu.dispatching) context.push('/dispatching');
                      }
                    : null,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(icon, size: 36, color: enabled ? Theme.of(context).colorScheme.primary : Colors.grey.shade400),
                      const SizedBox(height: 8),
                      Text(title, textAlign: TextAlign.center, style: TextStyle(fontWeight: FontWeight.w500, color: enabled ? null : Colors.grey.shade500)),
                      if (!enabled) Icon(Icons.lock_outline, size: 16, color: Colors.grey.shade400),
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

enum _Menu { warehouse, items, receipts, transfers, audit, dispatching, reports }
