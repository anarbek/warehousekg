import 'package:go_router/go_router.dart';
import '../features/auth/login_screen.dart';
import '../features/dashboard/dashboard_screen.dart';
import '../features/audit/audit_list_screen.dart';
import '../features/audit/audit_detail_screen.dart';
import '../features/audit/audit_count_screen.dart';

final appRoutes = [
  GoRoute(
    path: '/login',
    builder: (context, state) => const LoginScreen(),
  ),
  GoRoute(
    path: '/dashboard',
    builder: (context, state) => const DashboardScreen(),
  ),
  GoRoute(
    path: '/audits',
    builder: (context, state) => const AuditListScreen(),
    routes: [
      GoRoute(
        path: ':id',
        builder: (context, state) => AuditDetailScreen(
          auditId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: ':id/count',
        builder: (context, state) => AuditCountScreen(
          auditId: state.pathParameters['id']!,
        ),
      ),
    ],
  ),
];
