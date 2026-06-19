enum AuthState { initial, loading, authenticated, unauthenticated, error }

class AuthStateData {
  final AuthState state;
  final String? errorMessage;
  final String? userName;

  const AuthStateData({
    this.state = AuthState.initial,
    this.errorMessage,
    this.userName,
  });

  AuthStateData copyWith({AuthState? state, String? errorMessage, String? userName}) {
    return AuthStateData(
      state: state ?? this.state,
      errorMessage: errorMessage,
      userName: userName ?? this.userName,
    );
  }
}
