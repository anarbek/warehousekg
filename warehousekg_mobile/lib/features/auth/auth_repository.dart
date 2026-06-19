import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api/api_client.dart';
import '../../core/api/api_constants.dart';
import 'auth_provider.dart';

final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());

class AuthNotifier extends StateNotifier<AuthStateData> {
  final ApiClient _api;

  AuthNotifier(this._api) : super(const AuthStateData());

  Future<void> login(String username, String password) async {
    state = state.copyWith(state: AuthState.loading);
    try {
      final response = await _api.dio.post(ApiConstants.login, data: {
        'username': username,
        'password': password,
      });
      final data = response.data;
      await _api.saveToken(data['accessToken']);
      await _api.saveTenantId(data['tenantId'] ?? '');
      state = AuthStateData(
        state: AuthState.authenticated,
        userName: data['userName'] ?? username,
      );
    } catch (e) {
      state = state.copyWith(
        state: AuthState.error,
        errorMessage: 'Неверное имя пользователя или пароль',
      );
    }
  }

  Future<void> checkAuth() async {
    final token = await _api.getToken();
    if (token != null) {
      state = const AuthStateData(state: AuthState.authenticated);
    } else {
      state = const AuthStateData(state: AuthState.unauthenticated);
    }
  }

  Future<void> logout() async {
    await _api.clearAuth();
    state = const AuthStateData(state: AuthState.unauthenticated);
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthStateData>((ref) {
  return AuthNotifier(ref.watch(apiClientProvider));
});
