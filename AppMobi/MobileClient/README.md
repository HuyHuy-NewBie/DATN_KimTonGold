# Kim Tôn Mobile

Ứng dụng mobile Flutter cho hệ thống quản lý vàng bạc đá quý Kim Tôn.

App kết nối tới `AppMobi/MobileApi` và dùng chung database với web tại `Web/GoldManagementSystem`. Không cần chỉnh sửa code web để chạy mobile.

## Chạy nhanh

```powershell
cd D:\DATN\AppMobi\MobileApi
dotnet restore
dotnet run --launch-profile http
```

```powershell
cd D:\DATN\AppMobi\MobileClient
flutter pub get
flutter devices
flutter run -d chrome
```

Xem hướng dẫn đầy đủ tại `AppMobi/HUONG_DAN_CHAY_APP_MOBILE.md`.
