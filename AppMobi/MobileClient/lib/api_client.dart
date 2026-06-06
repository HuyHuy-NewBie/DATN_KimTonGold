import 'dart:convert';

import 'package:http/http.dart' as http;

import 'models.dart';

class ApiException implements Exception {
  ApiException(this.message, {required this.statusCode});

  final String message;
  final int statusCode;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({required this.apiBase});

  String apiBase;
  String? accessToken;
  String? refreshToken;
  String? deviceId;
  Future<bool> Function()? onRefreshToken;
  Future<void> Function()? onUnauthorized;

  Uri _uri(String path, [Map<String, String?>? query]) {
    final cleanBase = apiBase.replaceFirst(RegExp(r'/+$'), '');
    final cleanQuery = <String, String>{};
    query?.forEach((key, value) {
      if (value != null && value.trim().isNotEmpty) {
        cleanQuery[key] = value;
      }
    });
    return Uri.parse(
      '$cleanBase$path',
    ).replace(queryParameters: cleanQuery.isEmpty ? null : cleanQuery);
  }

  Future<dynamic> _send(
    String path, {
    String method = 'GET',
    Map<String, String?>? query,
    Object? body,
    bool auth = true,
    bool retryOnUnauthorized = true,
  }) async {
    final headers = <String, String>{
      'Accept': 'application/json',
      if (body != null) 'Content-Type': 'application/json',
      if (auth && accessToken != null && accessToken!.isNotEmpty)
        'Authorization': 'Bearer $accessToken',
    };

    final requestBody = body == null ? null : jsonEncode(body);
    final response = switch (method) {
      'POST' => await http.post(
        _uri(path, query),
        headers: headers,
        body: requestBody,
      ),
      'PUT' => await http.put(
        _uri(path, query),
        headers: headers,
        body: requestBody,
      ),
      'DELETE' => await http.delete(
        _uri(path, query),
        headers: headers,
        body: requestBody,
      ),
      _ => await http.get(_uri(path, query), headers: headers),
    };

    if (response.statusCode == 401 &&
        auth &&
        retryOnUnauthorized &&
        onRefreshToken != null) {
      final refreshed = await onRefreshToken!.call();
      if (refreshed) {
        return _send(
          path,
          method: method,
          query: query,
          body: body,
          auth: auth,
          retryOnUnauthorized: false,
        );
      }
      await onUnauthorized?.call();
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw ApiException(_readError(response), statusCode: response.statusCode);
    }

    if (response.statusCode == 204 || response.body.trim().isEmpty) {
      return null;
    }

    return jsonDecode(utf8.decode(response.bodyBytes));
  }

  String _readError(http.Response response) {
    final fallback =
        '${response.statusCode} ${response.reasonPhrase ?? 'Lỗi API'}';
    if (response.body.trim().isEmpty) {
      return fallback;
    }
    try {
      final json = jsonDecode(utf8.decode(response.bodyBytes));
      if (json is Map<String, dynamic>) {
        return ApiErrorBody.fromJson(json).message;
      }
    } catch (_) {
      return utf8.decode(response.bodyBytes);
    }
    return fallback;
  }

  Future<AuthResponse> login({
    required String identifier,
    required String password,
    required String deviceId,
    required bool rememberDevice,
  }) async {
    final json = await _send(
      '/api/auth/login',
      method: 'POST',
      auth: false,
      body: {
        'identifier': identifier,
        'password': password,
        'deviceId': deviceId,
        'rememberDevice': rememberDevice,
      },
    );
    return AuthResponse.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<AuthResponse> refreshSession({
    required String refreshToken,
    required String deviceId,
  }) async {
    final json = await _send(
      '/api/auth/refresh',
      method: 'POST',
      auth: false,
      body: {'refreshToken': refreshToken, 'deviceId': deviceId},
    );
    return AuthResponse.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<UserProfile> me() async {
    final json = await _send('/api/auth/me');
    return UserProfile.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<void> logout() async {
    await _send(
      '/api/auth/logout',
      method: 'POST',
      body: {'refreshToken': refreshToken, 'deviceId': deviceId},
    );
  }

  Future<CatalogMetadata> metadata() async {
    final json = await _send('/api/metadata');
    return CatalogMetadata.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<List<Branch>> branches({bool includeInactive = false}) async {
    final json = await _send(
      '/api/branches',
      query: {'includeInactive': includeInactive.toString()},
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Branch.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<Branch> createBranch({
    required String branchName,
    String? address,
    String? phoneNumber,
  }) async {
    final json = await _send(
      '/api/branches',
      method: 'POST',
      body: {
        'branchName': branchName,
        'address': address,
        'phoneNumber': phoneNumber,
      },
    );
    return Branch.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<Branch> updateBranchStatus(int id, bool isActive) async {
    final json = await _send(
      '/api/branches/$id/status',
      method: 'PUT',
      body: {'isActive': isActive},
    );
    return Branch.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<List<Product>> products({
    String? search,
    String? line,
    String? category,
    String? status,
    int? branchId,
    bool includeDeleted = false,
  }) async {
    final json = await _send(
      '/api/products',
      query: {
        'search': search,
        'line': line,
        'category': category,
        'status': status,
        'branchId': branchId?.toString(),
        'includeDeleted': includeDeleted.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Product.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<Product> product(int id) async {
    final json = await _send('/api/products/$id');
    return Product.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<Product> createProduct(ProductPayload payload) async {
    final json = await _send(
      '/api/products',
      method: 'POST',
      body: payload.toJson(),
    );
    return Product.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<Product> updateProduct(int id, ProductPayload payload) async {
    final json = await _send(
      '/api/products/$id',
      method: 'PUT',
      body: payload.toJson(),
    );
    return Product.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<void> deleteProduct(int id) async {
    await _send('/api/products/$id', method: 'DELETE');
  }

  Future<List<Order>> orders({
    String? from,
    String? to,
    String? status,
    int? branchId,
    int take = 200,
  }) async {
    final json = await _send(
      '/api/orders',
      query: {
        'from': from,
        'to': to,
        'status': status,
        'branchId': branchId?.toString(),
        'take': take.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Order.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<List<Order>> orderHistory({
    required String from,
    required String to,
    String? status,
    int? branchId,
    int take = 200,
  }) async {
    final json = await _send(
      '/api/orders/history',
      query: {
        'from': from,
        'to': to,
        'status': status,
        'branchId': branchId?.toString(),
        'take': take.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Order.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<List<Order>> todayOrders({
    String? status,
    int? branchId,
    int take = 200,
  }) async {
    final json = await _send(
      '/api/orders/today',
      query: {
        'status': status,
        'branchId': branchId?.toString(),
        'take': take.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Order.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<List<Order>> pendingOrders({
    String? from,
    String? to,
    int? branchId,
    int take = 100,
  }) async {
    final json = await _send(
      '/api/orders/pending-approval',
      query: {
        'from': from,
        'to': to,
        'branchId': branchId?.toString(),
        'take': take.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => Order.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<Order> getOrder(int id) async {
    final json = await _send('/api/orders/$id');
    return Order.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<Order> updateOrderStatus(int id, String status) async {
    final json = await _send(
      '/api/orders/$id/status',
      method: 'PUT',
      body: {'status': status},
    );
    return Order.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<Order> decideOrder(int id, String action) async {
    final json = await _send('/api/orders/$id/$action', method: 'POST');
    return Order.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<RevenueSummary> revenueDay({
    required String date,
    int? branchId,
  }) async {
    final json = await _send(
      '/api/reports/revenue/day',
      query: {'date': date, 'branchId': branchId?.toString()},
    );
    return RevenueSummary.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<RevenueSummary> revenueMonth({
    required String month,
    int? branchId,
    String bucket = 'day',
  }) async {
    final json = await _send(
      '/api/reports/revenue/month',
      query: {
        'month': month,
        'branchId': branchId?.toString(),
        'bucket': bucket,
      },
    );
    return RevenueSummary.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<RevenueSummary> revenueRange({
    required String from,
    required String to,
    int? branchId,
    String bucket = 'day',
  }) async {
    final json = await _send(
      '/api/reports/revenue/range',
      query: {
        'from': from,
        'to': to,
        'branchId': branchId?.toString(),
        'bucket': bucket,
      },
    );
    return RevenueSummary.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<void> registerPushToken(String expoPushToken, String platform) async {
    await _send(
      '/api/devices/push-token',
      method: 'POST',
      body: {
        'deviceId': deviceId,
        'expoPushToken': expoPushToken,
        'platform': platform,
      },
    );
  }

  Future<List<UserProfile>> users({
    String? search,
    String? role,
    int? branchId,
    bool includeInactive = true,
  }) async {
    final json = await _send(
      '/api/users',
      query: {
        'search': search,
        'role': role,
        'branchId': branchId?.toString(),
        'includeInactive': includeInactive.toString(),
      },
    );
    return (json as List)
        .whereType<Map>()
        .map((item) => UserProfile.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<UserProfile> createUser({
    required String fullName,
    required String email,
    required String password,
    required String role,
    int? branchId,
  }) async {
    final json = await _send(
      '/api/users',
      method: 'POST',
      body: {
        'fullName': fullName,
        'email': email,
        'password': password,
        'role': role,
        'branchId': branchId,
      },
    );
    return UserProfile.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<UserProfile> updateUserRole(String id, String role) async {
    final json = await _send(
      '/api/users/$id/role',
      method: 'PUT',
      body: {'role': role},
    );
    return UserProfile.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<UserProfile> updateUserStatus(String id, bool isActive) async {
    final json = await _send(
      '/api/users/$id/status',
      method: 'PUT',
      body: {'isActive': isActive},
    );
    return UserProfile.fromJson(Map<String, dynamic>.from(json as Map));
  }

  Future<MarketSnapshot> getMarketPrices() async {
    final json = await _send('/api/market/prices', auth: false);
    return MarketSnapshot.fromJson(Map<String, dynamic>.from(json as Map));
  }
}
