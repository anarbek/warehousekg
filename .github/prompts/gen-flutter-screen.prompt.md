---
description: "Scaffold a new Flutter feature module: models, screens, repository with Riverpod providers, Dio API calls, GoRouter routes. Use when: adding a new mobile screen, creating a Flutter feature, building offline-first mobile modules."
agent: "agent"
argument-hint: "Feature name (e.g., transfers, shipments)..."
---

# Generate Flutter Feature Module

Scaffold all files for a new Flutter feature in `warehousekg_mobile/`, following the exact patterns from the existing audit and dispatching modules.

## What You'll Generate

For a feature called `{name}` (lowercase, singular), create:

### 1. Models
`lib/features/{name}/models/{name}_models.dart`
- Data classes with `fromJson`/`toJson` factory constructors
- Include: `id`, status enums, display properties, nested child models
- Example pattern from `dispatching_models.dart`:
```dart
class RouteModel {
  final String id;
  final String number;
  final String status;
  // ...
  factory RouteModel.fromJson(Map<String, dynamic> json) => RouteModel(
    id: json['id'] as String,
    number: json['number'] as String,
    status: json['status'] as String,
  );
}
```

### 2. Repository
`lib/features/{name}/{name}_repository.dart`
- API methods using Dio (from `api_client.dart`)
- Endpoint constants in `api_constants.dart`
- Riverpod providers: `FutureProvider` for data, `StateNotifierProvider` for mutable state
- Pattern:
```dart
final {name}ListProvider = FutureProvider<List<{Name}Model>>((ref) async {
  final dio = ref.read(dioProvider);
  final response = await dio.get(ApiConstants.{name}s);
  return (response.data as List).map((j) => {Name}Model.fromJson(j)).toList();
});
```

### 3. Screens
- `lib/features/{name}/screens/{name}_list_screen.dart` — list view with pull-to-refresh
- `lib/features/{name}/screens/{name}_detail_screen.dart` — detail view with status badges
- If workflow: `{name}_workflow_screen.dart` — action buttons (start/complete/cancel)

### 4. Routes
`lib/app/routes.dart` — add GoRouter route entries:
```dart
GoRoute(
  path: '/{name}',
  builder: (_, __) => const {Name}ListScreen(),
  routes: [
    GoRoute(
      path: ':id',
      builder: (_, state) => {Name}DetailScreen(id: state.pathParameters['id']!),
    ),
  ],
),
```

### 5. API Constants
`lib/core/api/api_constants.dart` — add endpoint constants:
```dart
static const String {name}s = '/api/v1/{name}s';
static const String {name}ById = '/api/v1/{name}s'; // append /{id}
```

### 6. Dashboard Tile (if needed)
`lib/features/dashboard/dashboard_screen.dart` — add module card with Russian label

## Patterns to Follow

- **Android emulator URL**: `http://10.0.2.2:5134` — NEVER `localhost`
- **Offline-first when applicable**: store data locally, sync when online
- **Riverpod**: `FutureProvider` for async data, `StateNotifierProvider` for mutable state
- **GoRouter**: auth redirect guard already in place
- **Dio**: JWT interceptor handles auth — just use `dioProvider`
- **Russian UI labels**: use Russian text directly (no i18n in mobile yet)

Reference existing features:
- `lib/features/audit/` — offline-first pattern with sync
- `lib/features/dispatching/` — workflow pattern with status transitions
