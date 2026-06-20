class ApiConstants {
  static const String baseUrl = 'http://10.0.2.2:5134'; // Android emulator → host
  static const String apiPrefix = '/api/v1';

  static const String login = '$apiPrefix/auth/login';
  static const String warehouses = '$apiPrefix/warehouses';
  static const String inventoryItems = '$apiPrefix/inventory-items';
  static const String warehouseStock = '$apiPrefix/reports/warehouse-stock';
  static const String stockAudits = '$apiPrefix/stock-audits';
  static const String employees = '$apiPrefix/employees';

  static String auditComplete(String id) => '$stockAudits/$id/complete';
  static String auditCancel(String id) => '$stockAudits/$id/cancel';

  // ─── Dispatching / Driver ──────────────────────────
  static const String myRoutes = '$apiPrefix/routes/my';
  static String myRouteDetail(String id) => '$apiPrefix/routes/my/$id/detail';
  static String routeStart(String id) => '$apiPrefix/routes/$id/start';
  static String routeComplete(String id) => '$apiPrefix/routes/$id/complete';
  static String stopArrive(String routeId, String stopId) => '$apiPrefix/routes/$routeId/stops/$stopId/arrive';
  static String stopComplete(String routeId, String stopId) => '$apiPrefix/routes/$routeId/stops/$stopId/complete';
  static String stopSkip(String routeId, String stopId) => '$apiPrefix/routes/$routeId/stops/$stopId/skip';
}
