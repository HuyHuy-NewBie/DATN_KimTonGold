Object? _value(Map<String, dynamic> json, String key) {
  final pascal = key.isEmpty
      ? key
      : '${key[0].toUpperCase()}${key.substring(1)}';
  return json[key] ?? json[pascal];
}

String _string(Map<String, dynamic> json, String key, [String fallback = '']) {
  final value = _value(json, key);
  return value == null ? fallback : value.toString();
}

String? _nullableString(Map<String, dynamic> json, String key) {
  final value = _value(json, key);
  final text = value?.toString().trim();
  return text == null || text.isEmpty ? null : text;
}

int _int(Map<String, dynamic> json, String key, [int fallback = 0]) {
  final value = _value(json, key);
  if (value is int) {
    return value;
  }
  if (value is num) {
    return value.toInt();
  }
  return int.tryParse(value?.toString() ?? '') ?? fallback;
}

int? _nullableInt(Map<String, dynamic> json, String key) {
  final value = _value(json, key);
  if (value == null) {
    return null;
  }
  if (value is int) {
    return value;
  }
  if (value is num) {
    return value.toInt();
  }
  return int.tryParse(value.toString());
}

double _double(Map<String, dynamic> json, String key, [double fallback = 0]) {
  final value = _value(json, key);
  if (value is num) {
    return value.toDouble();
  }
  return double.tryParse(value?.toString() ?? '') ?? fallback;
}

double? _nullableDouble(Map<String, dynamic> json, String key) {
  final value = _value(json, key);
  if (value == null) {
    return null;
  }
  if (value is num) {
    return value.toDouble();
  }
  return double.tryParse(value.toString());
}

bool _bool(Map<String, dynamic> json, String key, [bool fallback = false]) {
  final value = _value(json, key);
  if (value is bool) {
    return value;
  }
  return bool.tryParse(value?.toString() ?? '') ?? fallback;
}

DateTime? _date(Map<String, dynamic> json, String key) {
  final value = _value(json, key);
  return value == null ? null : DateTime.tryParse(value.toString())?.toLocal();
}

List<String> _stringList(Map<String, dynamic> json, String key) {
  final value = _value(json, key);
  if (value is List) {
    return value.map((item) => item.toString()).toList();
  }
  return const [];
}

List<T> _objectList<T>(
  Map<String, dynamic> json,
  String key,
  T Function(Map<String, dynamic>) map,
) {
  final value = _value(json, key);
  if (value is! List) {
    return const [];
  }
  return value
      .whereType<Map>()
      .map((item) => map(Map<String, dynamic>.from(item)))
      .toList();
}

class ApiErrorBody {
  const ApiErrorBody(this.message);

  final String message;

  factory ApiErrorBody.fromJson(Map<String, dynamic> json) {
    return ApiErrorBody(_string(json, 'message', 'Không thể xử lý yêu cầu.'));
  }
}

class AuthResponse {
  const AuthResponse({
    required this.accessToken,
    required this.accessTokenExpiresAt,
    required this.refreshToken,
    required this.refreshTokenExpiresAt,
    required this.user,
  });

  final String accessToken;
  final DateTime? accessTokenExpiresAt;
  final String refreshToken;
  final DateTime? refreshTokenExpiresAt;
  final UserProfile user;

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: _string(json, 'accessToken'),
      accessTokenExpiresAt: _date(json, 'accessTokenExpiresAt'),
      refreshToken: _string(json, 'refreshToken'),
      refreshTokenExpiresAt: _date(json, 'refreshTokenExpiresAt'),
      user: UserProfile.fromJson(
        Map<String, dynamic>.from(_value(json, 'user') as Map),
      ),
    );
  }
}

class UserProfile {
  const UserProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.phoneNumber,
    required this.branchId,
    required this.branchName,
    required this.isActive,
    required this.roles,
    required this.highestRole,
    required this.permissions,
  });

  final String id;
  final String fullName;
  final String? email;
  final String? phoneNumber;
  final int? branchId;
  final String? branchName;
  final bool isActive;
  final List<String> roles;
  final String highestRole;
  final List<String> permissions;

  bool can(String permission) => permissions.contains(permission);

  bool hasRole(String role) =>
      roles.any((item) => item.toLowerCase() == role.toLowerCase());

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: _string(json, 'id'),
      fullName: _string(json, 'fullName', '--'),
      email: _nullableString(json, 'email'),
      phoneNumber: _nullableString(json, 'phoneNumber'),
      branchId: _nullableInt(json, 'branchId'),
      branchName: _nullableString(json, 'branchName'),
      isActive: _bool(json, 'isActive', true),
      roles: _stringList(json, 'roles'),
      highestRole: _string(json, 'highestRole', 'Khách hàng'),
      permissions: _stringList(json, 'permissions'),
    );
  }
}

class Branch {
  const Branch({
    required this.id,
    required this.branchName,
    required this.address,
    required this.phoneNumber,
    required this.isActive,
  });

  final int id;
  final String branchName;
  final String? address;
  final String? phoneNumber;
  final bool isActive;

  factory Branch.fromJson(Map<String, dynamic> json) {
    return Branch(
      id: _int(json, 'id'),
      branchName: _string(json, 'branchName', '--'),
      address: _nullableString(json, 'address'),
      phoneNumber: _nullableString(json, 'phoneNumber'),
      isActive: _bool(json, 'isActive', true),
    );
  }
}

class Product {
  const Product({
    required this.id,
    required this.name,
    required this.category,
    required this.goldType,
    required this.productLine,
    required this.catalogMode,
    required this.assignedProductLines,
    required this.weight,
    required this.processingFee,
    required this.sellPrice,
    required this.buyPrice,
    required this.branchId,
    required this.branchName,
    required this.images,
    required this.description,
    required this.status,
    required this.createdAt,
    required this.diamondShape,
    required this.diamondCut,
    required this.diamondColor,
    required this.diamondClarity,
    required this.diamondCarat,
    required this.diamondSize,
    required this.diamondCertificate,
  });

  final int id;
  final String name;
  final String category;
  final String goldType;
  final String productLine;
  final String catalogMode;
  final List<String> assignedProductLines;
  final double weight;
  final double processingFee;
  final double sellPrice;
  final double buyPrice;
  final int branchId;
  final String? branchName;
  final List<String> images;
  final String? description;
  final String status;
  final DateTime? createdAt;
  final String? diamondShape;
  final String? diamondCut;
  final String? diamondColor;
  final String? diamondClarity;
  final double? diamondCarat;
  final double? diamondSize;
  final String? diamondCertificate;

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      id: _int(json, 'id'),
      name: _string(json, 'name', '--'),
      category: _string(json, 'category', '--'),
      goldType: _string(json, 'goldType', '--'),
      productLine: _string(json, 'productLine', 'Gold'),
      catalogMode: _string(json, 'catalogMode', 'Single'),
      assignedProductLines: _stringList(json, 'assignedProductLines'),
      weight: _double(json, 'weight'),
      processingFee: _double(json, 'processingFee'),
      sellPrice: _double(json, 'sellPrice'),
      buyPrice: _double(json, 'buyPrice'),
      branchId: _int(json, 'branchId'),
      branchName: _nullableString(json, 'branchName'),
      images: _stringList(json, 'images'),
      description: _nullableString(json, 'description'),
      status: _string(json, 'status', 'Còn hàng'),
      createdAt: _date(json, 'createdAt'),
      diamondShape: _nullableString(json, 'diamondShape'),
      diamondCut: _nullableString(json, 'diamondCut'),
      diamondColor: _nullableString(json, 'diamondColor'),
      diamondClarity: _nullableString(json, 'diamondClarity'),
      diamondCarat: _nullableDouble(json, 'diamondCarat'),
      diamondSize: _nullableDouble(json, 'diamondSize'),
      diamondCertificate: _nullableString(json, 'diamondCertificate'),
    );
  }
}

class ProductPayload {
  ProductPayload({
    required this.name,
    required this.category,
    required this.goldType,
    required this.productLine,
    required this.catalogMode,
    required this.assignedProductLines,
    required this.weight,
    required this.processingFee,
    required this.sellPrice,
    required this.buyPrice,
    required this.branchId,
    required this.images,
    required this.description,
    required this.status,
    required this.diamondShape,
    required this.diamondCut,
    required this.diamondColor,
    required this.diamondClarity,
    required this.diamondCarat,
    required this.diamondSize,
    required this.diamondCertificate,
  });

  final String name;
  final String category;
  final String goldType;
  final String productLine;
  final String catalogMode;
  final List<String> assignedProductLines;
  final double weight;
  final double processingFee;
  final double sellPrice;
  final double buyPrice;
  final int branchId;
  final List<String> images;
  final String? description;
  final String status;
  final String? diamondShape;
  final String? diamondCut;
  final String? diamondColor;
  final String? diamondClarity;
  final double? diamondCarat;
  final double? diamondSize;
  final String? diamondCertificate;

  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'category': category,
      'goldType': goldType,
      'productLine': productLine,
      'catalogMode': catalogMode,
      'assignedProductLines': assignedProductLines,
      'weight': weight,
      'processingFee': processingFee,
      'sellPrice': sellPrice,
      'buyPrice': buyPrice,
      'branchId': branchId,
      'images': images,
      'description': description,
      'status': status,
      'diamondShape': diamondShape,
      'diamondCut': diamondCut,
      'diamondColor': diamondColor,
      'diamondClarity': diamondClarity,
      'diamondCarat': diamondCarat,
      'diamondSize': diamondSize,
      'diamondCertificate': diamondCertificate,
    };
  }
}

class OrderDetail {
  const OrderDetail({
    required this.id,
    required this.productId,
    required this.productName,
    required this.unitPrice,
    required this.quantity,
  });

  final int id;
  final int productId;
  final String? productName;
  final double unitPrice;
  final int quantity;

  factory OrderDetail.fromJson(Map<String, dynamic> json) {
    return OrderDetail(
      id: _int(json, 'id'),
      productId: _int(json, 'productId'),
      productName: _nullableString(json, 'productName'),
      unitPrice: _double(json, 'unitPrice'),
      quantity: _int(json, 'quantity'),
    );
  }
}

class Order {
  const Order({
    required this.id,
    required this.orderNumber,
    required this.customerName,
    required this.customerPhone,
    required this.totalAmount,
    required this.status,
    required this.orderDate,
    required this.branchId,
    required this.branchName,
    required this.staffName,
    required this.details,
  });

  final int id;
  final String orderNumber;
  final String? customerName;
  final String? customerPhone;
  final double totalAmount;
  final String status;
  final DateTime? orderDate;
  final int branchId;
  final String? branchName;
  final String? staffName;
  final List<OrderDetail> details;

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      id: _int(json, 'id'),
      orderNumber: _string(json, 'orderNumber'),
      customerName: _nullableString(json, 'customerName'),
      customerPhone: _nullableString(json, 'customerPhone'),
      totalAmount: _double(json, 'totalAmount'),
      status: _string(json, 'status', '--'),
      orderDate: _date(json, 'orderDate'),
      branchId: _int(json, 'branchId'),
      branchName: _nullableString(json, 'branchName'),
      staffName: _nullableString(json, 'staffName'),
      details: _objectList(json, 'details', OrderDetail.fromJson),
    );
  }
}

class RevenueSummary {
  const RevenueSummary({
    required this.from,
    required this.to,
    required this.bucket,
    required this.revenue,
    required this.grossAmount,
    required this.orderCount,
    required this.completedCount,
    required this.pendingApprovalCount,
    required this.cancelledCount,
    required this.daily,
    required this.monthly,
    required this.byStatus,
  });

  final DateTime? from;
  final DateTime? to;
  final String bucket;
  final double revenue;
  final double grossAmount;
  final int orderCount;
  final int completedCount;
  final int pendingApprovalCount;
  final int cancelledCount;
  final List<RevenueBucket> daily;
  final List<RevenueMonthBucket> monthly;
  final List<StatusRevenue> byStatus;

  factory RevenueSummary.fromJson(Map<String, dynamic> json) {
    return RevenueSummary(
      from: _date(json, 'from'),
      to: _date(json, 'to'),
      bucket: _string(json, 'bucket', 'day'),
      revenue: _double(json, 'revenue'),
      grossAmount: _double(json, 'grossAmount'),
      orderCount: _int(json, 'orderCount'),
      completedCount: _int(json, 'completedCount'),
      pendingApprovalCount: _int(json, 'pendingApprovalCount'),
      cancelledCount: _int(json, 'cancelledCount'),
      daily: _objectList(json, 'daily', RevenueBucket.fromJson),
      monthly: _objectList(json, 'monthly', RevenueMonthBucket.fromJson),
      byStatus: _objectList(json, 'byStatus', StatusRevenue.fromJson),
    );
  }
}

class RevenueBucket {
  const RevenueBucket({
    required this.date,
    required this.revenue,
    required this.orderCount,
  });

  final DateTime? date;
  final double revenue;
  final int orderCount;

  factory RevenueBucket.fromJson(Map<String, dynamic> json) {
    return RevenueBucket(
      date: _date(json, 'date'),
      revenue: _double(json, 'revenue'),
      orderCount: _int(json, 'orderCount'),
    );
  }
}

class RevenueMonthBucket {
  const RevenueMonthBucket({
    required this.month,
    required this.from,
    required this.to,
    required this.revenue,
    required this.orderCount,
  });

  final String month;
  final DateTime? from;
  final DateTime? to;
  final double revenue;
  final int orderCount;

  factory RevenueMonthBucket.fromJson(Map<String, dynamic> json) {
    return RevenueMonthBucket(
      month: _string(json, 'month'),
      from: _date(json, 'from'),
      to: _date(json, 'to'),
      revenue: _double(json, 'revenue'),
      orderCount: _int(json, 'orderCount'),
    );
  }
}

class StatusRevenue {
  const StatusRevenue({
    required this.status,
    required this.amount,
    required this.orderCount,
  });

  final String status;
  final double amount;
  final int orderCount;

  factory StatusRevenue.fromJson(Map<String, dynamic> json) {
    return StatusRevenue(
      status: _string(json, 'status', '--'),
      amount: _double(json, 'amount'),
      orderCount: _int(json, 'orderCount'),
    );
  }
}

class CatalogMetadata {
  const CatalogMetadata({
    required this.productLines,
    required this.catalogModes,
    required this.productStatuses,
    required this.orderStatuses,
    required this.roles,
    required this.goldCategories,
    required this.silverCategories,
    required this.diamondCategories,
    required this.goldMaterials,
    required this.silverMaterials,
    required this.diamondMaterials,
    required this.diamondShapes,
    required this.diamondCuts,
    required this.diamondColors,
    required this.diamondClarities,
    required this.diamondCertificates,
  });

  final List<String> productLines;
  final List<String> catalogModes;
  final List<String> productStatuses;
  final List<String> orderStatuses;
  final List<String> roles;
  final List<String> goldCategories;
  final List<String> silverCategories;
  final List<String> diamondCategories;
  final List<String> goldMaterials;
  final List<String> silverMaterials;
  final List<String> diamondMaterials;
  final List<String> diamondShapes;
  final List<String> diamondCuts;
  final List<String> diamondColors;
  final List<String> diamondClarities;
  final List<String> diamondCertificates;

  factory CatalogMetadata.fromJson(Map<String, dynamic> json) {
    return CatalogMetadata(
      productLines: _stringList(json, 'productLines'),
      catalogModes: _stringList(json, 'catalogModes'),
      productStatuses: _stringList(json, 'productStatuses'),
      orderStatuses: _stringList(json, 'orderStatuses'),
      roles: _stringList(json, 'roles'),
      goldCategories: _stringList(json, 'goldCategories'),
      silverCategories: _stringList(json, 'silverCategories'),
      diamondCategories: _stringList(json, 'diamondCategories'),
      goldMaterials: _stringList(json, 'goldMaterials'),
      silverMaterials: _stringList(json, 'silverMaterials'),
      diamondMaterials: _stringList(json, 'diamondMaterials'),
      diamondShapes: _stringList(json, 'diamondShapes'),
      diamondCuts: _stringList(json, 'diamondCuts'),
      diamondColors: _stringList(json, 'diamondColors'),
      diamondClarities: _stringList(json, 'diamondClarities'),
      diamondCertificates: _stringList(json, 'diamondCertificates'),
    );
  }

  static CatalogMetadata fallback() {
    return const CatalogMetadata(
      productLines: ['Gold', 'Silver', 'Diamond'],
      catalogModes: ['Single', 'Multi'],
      productStatuses: [
        'Mới',
        'Còn hàng',
        'Bán chạy',
        'Hết hàng',
        'Đã bán',
        'Đã xóa',
      ],
      orderStatuses: [
        'Chờ phê duyệt',
        'Đang xử lý',
        'Vận chuyển',
        'Hoàn thành',
        'Đã hủy',
      ],
      roles: [
        'Admin',
        'Branch Owner',
        'Manager',
        'Accountant',
        'Staff',
        'Khách hàng',
      ],
      goldCategories: [
        'Nhẫn',
        'Nhẫn Cưới',
        'Dây Chuyền',
        'Lắc Tay',
        'Bông Tai',
      ],
      silverCategories: [
        'Trang Sức Bạc',
        'Nhẫn',
        'Dây Chuyền',
        'Lắc Tay',
        'Bông Tai',
      ],
      diamondCategories: [
        'Nhẫn Kim Cương',
        'Bông Tai Kim Cương',
        'Dây Chuyền Kim Cương',
        'Kim Cương Viên',
      ],
      goldMaterials: ['Vàng 24K', 'Vàng 18K', 'Vàng 9999', 'Vàng Trắng'],
      silverMaterials: ['Bạc S925', 'Bạc Ý 925', 'Bạc Ta'],
      diamondMaterials: [
        'Kim Cương Thiên Nhiên',
        'Kim Cương Lab Grown',
        'Moissanite',
      ],
      diamondShapes: [
        'Round',
        'Princess',
        'Oval',
        'Emerald',
        'Cushion',
        'Pear',
      ],
      diamondCuts: ['Excellent', 'Very Good', 'Good'],
      diamondColors: ['D', 'E', 'F', 'G', 'H', 'I', 'J'],
      diamondClarities: ['IF', 'VVS1', 'VVS2', 'VS1', 'VS2', 'SI1', 'SI2'],
      diamondCertificates: ['GIA', 'IGI', 'AGS', 'Không chứng nhận'],
    );
  }
}

class MarketSnapshot {
  const MarketSnapshot({
    required this.isLive,
    required this.statusMessage,
    required this.retrievedAtUtc,
    required this.usdToVndRate,
    required this.gold,
    required this.silver,
  });

  final bool isLive;
  final String statusMessage;
  final DateTime? retrievedAtUtc;
  final double usdToVndRate;
  final PreciousMetalSnapshot gold;
  final PreciousMetalSnapshot silver;

  factory MarketSnapshot.fromJson(Map<String, dynamic> json) {
    return MarketSnapshot(
      isLive: _bool(json, 'isLive'),
      statusMessage: _string(json, 'statusMessage'),
      retrievedAtUtc: _date(json, 'retrievedAtUtc'),
      usdToVndRate: _double(json, 'usdToVndRate'),
      gold: PreciousMetalSnapshot.fromJson(
        Map<String, dynamic>.from(_value(json, 'gold') as Map),
      ),
      silver: PreciousMetalSnapshot.fromJson(
        Map<String, dynamic>.from(_value(json, 'silver') as Map),
      ),
    );
  }
}

class PreciousMetalSnapshot {
  const PreciousMetalSnapshot({
    required this.displayName,
    required this.price,
    required this.bid,
    required this.ask,
    required this.unit,
    required this.lastUpdatedUtc,
    required this.change24h,
  });

  final String displayName;
  final double price;
  final double bid;
  final double ask;
  final String unit;
  final DateTime? lastUpdatedUtc;
  final MarketChangeSnapshot change24h;

  factory PreciousMetalSnapshot.fromJson(Map<String, dynamic> json) {
    return PreciousMetalSnapshot(
      displayName: _string(json, 'displayName'),
      price: _double(json, 'price'),
      bid: _double(json, 'bid'),
      ask: _double(json, 'ask'),
      unit: _string(json, 'unit'),
      lastUpdatedUtc: _date(json, 'lastUpdatedUtc'),
      change24h: MarketChangeSnapshot.fromJson(
        Map<String, dynamic>.from(_value(json, 'change24H') as Map),
      ),
    );
  }
}

class MarketChangeSnapshot {
  const MarketChangeSnapshot({required this.amount, required this.percent});

  final double amount;
  final double percent;

  factory MarketChangeSnapshot.fromJson(Map<String, dynamic> json) {
    return MarketChangeSnapshot(
      amount: _double(json, 'amount'),
      percent: _double(json, 'percent'),
    );
  }
}

