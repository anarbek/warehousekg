import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_constants.dart';

class ApiClient {
  late final Dio dio;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  static const String _tokenKey = 'jwt_token';
  static const String _tenantIdKey = 'tenant_id';

  ApiClient() {
    dio = Dio(BaseOptions(
      baseUrl: ApiConstants.baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {'Content-Type': 'application/json'},
    ));

    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _storage.read(key: _tokenKey);
        final tenantId = await _storage.read(key: _tenantIdKey);
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        if (tenantId != null) {
          options.headers['X-Tenant-Id'] = tenantId;
        }
        handler.next(options);
      },
      onError: (error, handler) {
        if (error.response?.statusCode == 401) {
          _storage.deleteAll();
        }
        handler.next(error);
      },
    ));
  }

  Future<void> saveToken(String token) async {
    await _storage.write(key: _tokenKey, value: token);
  }

  Future<void> saveTenantId(String tenantId) async {
    await _storage.write(key: _tenantIdKey, value: tenantId);
  }

  Future<String?> getToken() async => _storage.read(key: _tokenKey);

  Future<void> clearAuth() async => _storage.deleteAll();
}
