import 'dart:math';

import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'api_client.dart';
import 'models.dart';

class AppState extends ChangeNotifier {
  AppState() : api = ApiClient(apiBase: _defaultApiBase()) {
    api.onRefreshToken = refreshSession;
    api.onUnauthorized = forceLogout;
  }

  static const _apiBaseKey = 'gold.mobile.apiBase';
  static const _deviceIdKey = 'gold.mobile.deviceId';
  static const _refreshTokenKey = 'gold.mobile.refreshToken';

  final ApiClient api;
  SharedPreferences? _prefs;

  bool initialized = false;
  bool busy = false;
  bool globalLoading = false;
  UserProfile? user;
  CatalogMetadata metadata = CatalogMetadata.fallback();
  List<Branch> branches = const [];
  MarketSnapshot? marketData;

  String get apiBase => api.apiBase;
  String get deviceId => api.deviceId ?? '';
  bool get isLoggedIn => user != null;

  static String _defaultApiBase() {
    if (kIsWeb) {
      return 'http://localhost:5087';
    }
    if (defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5087';
    }
    return 'http://localhost:5087';
  }

  Future<void> initialize() async {
    _prefs = await SharedPreferences.getInstance();
    api.apiBase = _prefs?.getString(_apiBaseKey) ?? _defaultApiBase();
    api.deviceId = await _getOrCreateDeviceId();
    api.refreshToken = _prefs?.getString(_refreshTokenKey);

    // Tự động đăng nhập lại theo yêu cầu
    if (api.refreshToken != null && api.refreshToken!.isNotEmpty) {
      try {
        await refreshSession().timeout(const Duration(seconds: 2));
      } catch (e) {
        debugPrint('Lỗi tự động đăng nhập: $e');
      }
    }

    initialized = true;
    notifyListeners();
  }

  Future<String> _getOrCreateDeviceId() async {
    final existing = _prefs?.getString(_deviceIdKey);
    if (existing != null && existing.isNotEmpty) {
      return existing;
    }

    final random = Random.secure();
    final id = List<int>.generate(
      16,
      (_) => random.nextInt(256),
    ).map((byte) => byte.toRadixString(16).padLeft(2, '0')).join();
    await _prefs?.setString(_deviceIdKey, id);
    return id;
  }

  Future<void> login({
    required String apiBase,
    required String identifier,
    required String password,
    required bool rememberDevice,
  }) async {
    await _prefs?.setString(
      _apiBaseKey,
      apiBase.trim().replaceFirst(RegExp(r'/+$'), ''),
    );
    api.apiBase = _prefs?.getString(_apiBaseKey) ?? apiBase;
    busy = true;
    notifyListeners();
    try {
      final response = await api.login(
        identifier: identifier,
        password: password,
        deviceId: deviceId,
        rememberDevice: rememberDevice,
      );
      await _saveAuth(response, persistRefreshToken: rememberDevice);
      await bootstrap();
      await _registerPushNotification();
    } finally {
      busy = false;
      notifyListeners();
    }
  }

  Future<bool> refreshSession() async {
    final token = api.refreshToken;
    if (token == null || token.isEmpty || deviceId.isEmpty) {
      return false;
    }

    try {
      final response = await api.refreshSession(
        refreshToken: token,
        deviceId: deviceId,
      );
      await _saveAuth(response, persistRefreshToken: true);
      await bootstrap(silent: true);
      await _registerPushNotification();
      return true;
    } catch (_) {
      await forceLogout();
      return false;
    }
  }

  Future<void> _registerPushNotification() async {
    if (user == null) return;
    try {
      // Trong thực tế, bạn sẽ lấy token từ Firebase Messaging hoặc Expo
      // Ví dụ: final token = await FirebaseMessaging.instance.getToken();
      final mockToken = 'push_$deviceId';
      final platform = kIsWeb ? 'web' : defaultTargetPlatform.name.toLowerCase();

      await api.registerPushToken(mockToken, platform);
      debugPrint('Đã tự động đăng ký Push Token: $mockToken');
    } catch (e) {
      debugPrint('Lỗi khi tự động đăng ký Push Token: $e');
    }
  }


  Future<void> bootstrap({bool silent = false}) async {
    if (!silent) {
      busy = true;
      notifyListeners();
    }
    try {
      user = await api.me().timeout(const Duration(seconds: 2));
      final results = await Future.wait<Object?>([
        api.metadata().timeout(const Duration(seconds: 2)).catchError((_) => CatalogMetadata.fallback()),
        if (user?.can('branches:read') == true)
          api.branches(includeInactive: user?.can('branches:manage') == true).timeout(const Duration(seconds: 2))
        else
          Future.value(<Branch>[]),
        api.getMarketPrices().timeout(const Duration(seconds: 2)).catchError((_) => null),
      ]);
      metadata = results[0] as CatalogMetadata;
      branches = results[1] as List<Branch>;
      marketData = results[2] as MarketSnapshot?;
    } catch (e) {
      debugPrint('Lỗi bootstrap: $e');
    } finally {
      if (!silent) {
        busy = false;
      }
      notifyListeners();
    }
  }

  Future<void> refresh() async {
    globalLoading = true;
    notifyListeners();

    // Thực hiện đồng thời cả việc tải dữ liệu và chờ tối thiểu 1.5s
    // Nhưng tối đa không quá 2s (timeout trong bootstrap đã là 2s)
    await Future.wait([
      bootstrap(silent: true),
      Future.delayed(const Duration(milliseconds: 1500)),
    ]).timeout(
      const Duration(seconds: 2),
      onTimeout: () => [null, null], // Thoát ngay nếu quá 2s
    );

    globalLoading = false;
    notifyListeners();
  }


  Future<void> _saveAuth(
    AuthResponse response, {
    required bool persistRefreshToken,
  }) async {
    api.accessToken = response.accessToken;
    api.refreshToken = response.refreshToken;
    user = response.user;
    if (persistRefreshToken) {
      await _prefs?.setString(_refreshTokenKey, response.refreshToken);
    } else {
      await _prefs?.remove(_refreshTokenKey);
    }
  }

  Future<void> refreshBranches() async {
    if (user?.can('branches:read') != true) {
      branches = const [];
      notifyListeners();
      return;
    }
    branches = await api.branches(
      includeInactive: user?.can('branches:manage') == true,
    );
    notifyListeners();
  }

  Future<void> logout() async {
    try {
      await api.logout();
    } catch (_) {
      // Local logout must still clear the session.
    }
    await forceLogout();
  }

  Future<void> forceLogout() async {
    api.accessToken = null;
    api.refreshToken = null;
    user = null;
    branches = const [];
    await _prefs?.remove(_refreshTokenKey);
    notifyListeners();
  }
}
