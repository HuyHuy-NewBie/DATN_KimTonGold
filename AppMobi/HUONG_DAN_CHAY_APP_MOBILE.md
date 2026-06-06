# Hướng Dẫn Chạy App Mobile

Tài liệu này dùng để chạy ứng dụng Flutter tại `AppMobi/MobileClient` trên Windows/Web/Desktop và chạy Mobile API tại `AppMobi/MobileApi` làm nền dữ liệu cho app.

## 1. Chuẩn bị

- .NET SDK 8.0 trở lên.
- Flutter SDK đã cài và chạy được lệnh `flutter doctor`.
- SQL Server/SQLEXPRESS đang chạy database `GoldDb_Advanced`.

Mobile app dùng chung database với web. Connection string hiện tại của `AppMobi/MobileApi/appsettings.json`:

```json
Server=DESKTOP-J3NDM9B\\SQLEXPRESS;Database=GoldDb_Advanced;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true
```

Nếu máy khác SQL Server instance, sửa connection string trong `AppMobi/MobileApi/appsettings.json` cho đúng database web đang dùng.

## 2. Chạy nền dữ liệu cho app

Mở terminal thứ nhất để chạy Mobile API:

```powershell
cd D:\DATN\AppMobi\MobileApi
dotnet restore
dotnet run --launch-profile http
```

Giữ terminal này đang chạy. API mặc định:

```text
http://localhost:5087
```

Swagger:

```text
http://localhost:5087/swagger
```

Mobile API đọc trực tiếp dữ liệu từ database web `GoldDb_Advanced`. Khi khởi động, API tự tạo các bảng hỗ trợ mobile nếu chưa có: `MobileRefreshTokens`, `MobileDeviceTokens`, `MobileOrderNotificationLogs`.

## 3. Chạy App Flutter trên Windows/Web/Desktop

Mở terminal thứ hai:

```powershell
cd D:\DATN\AppMobi\MobileClient
flutter pub get
flutter devices
flutter run -d chrome
```

Nếu muốn chọn đúng môi trường chạy:

```powershell
flutter run -d windows
flutter run -d chrome
flutter run -d edge
```

API base khi chạy trên Windows/Web/Desktop:

```text
http://localhost:5087
```

Đăng nhập bằng tài khoản đã có trên web. App sẽ lấy dữ liệu thông qua Mobile API đang chạy ở terminal thứ nhất.

## 4. Lệnh kiểm tra nhanh

Kiểm tra Flutter client:

```powershell
cd D:\DATN\AppMobi\MobileClient
flutter analyze
flutter test
```

Kiểm tra Mobile API:

```powershell
cd D:\DATN\AppMobi\MobileApi
dotnet build
```

Nếu app không lấy được dữ liệu, kiểm tra lại:

- SQL Server/SQLEXPRESS đang chạy.
- Mobile API vẫn đang chạy tại `http://localhost:5087`.
- API base trong app là `http://localhost:5087`.
- Connection string trong `AppMobi/MobileApi/appsettings.json` đúng với database web.
