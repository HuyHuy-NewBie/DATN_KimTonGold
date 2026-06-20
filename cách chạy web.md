Hướng dẫn chạy dự án

### Tóm tắt nhanh

1. Mở terminal vào thư mục `GoldManagementSystem`
2. Chạy `dotnet restore`
3. Chạy `dotnet ef database update`
4. Chạy `dotnet run`

---

## Phương pháp 1: Dùng Visual Studio 2022 (Khuyên dùng)

### Bước 1: Mở terminal trong thư mục project

1. Mở PowerShell hoặc Terminal trong Visual Studio Code.
2. Chuyển đến thư mục `GoldManagementSystem`:
   ```powershell
   cd "C:\Users\ASUS\Downloads\DATN_KimTonGold-main\GoldManagementSystem"
   ```

### Bước 2: Tải gói nuget

1.Chạy trên powershell/terminal/cmd

```l
dotnet restore
```

2. Lệnh này sẽ tải các thư viện cần thiết cho project.

### Bước 3: Cài `dotnet-ef` nếu cần

1. Nếu bạn thấy lỗi `dotnet ef` không tìm thấy: chạy trên powershell/terminal/cmd
   ```
   dotnet tool install --global dotnet-ef
   ```
2. Nếu đã cài rồi, có thể bỏ qua bước này.

### Bước 4: Tạo hoặc cập nhật database

1. Chạy trên powershell/terminal/cmd
   ```
   dotnet ef database update
   ```
2. Lệnh này sẽ tạo các bảng trong SQL Server theo cấu hình trong `appsettings.json`.

### Bước 5: Chạy ứng dụng

1. Sau khi database cập nhật xong, chạy:
   ```powershell
   dotnet run
   ```
2. Đợi đến khi dòng sau xuất hiện:
   ```text
   Now listening on: http://localhost:5240
   ```
3. Mở trình duyệt và gõ:
   ```text
   http://localhost:5240
   ```

### Bước 6: Dừng ứng dụng

- Nếu muốn ngừng ứng dụng, nhấn trong terminal:
  ```text
  Ctrl + C
  ```

---

## Tài khoản mặc định_có thể tạo thêm tài khoản cá nhân để chạy test web

- Email: `admin@goldsys.com`
- Mật khẩu: `Admin@123`

---

## Lưu ý quan trọng

- Nếu chạy `dotnet run` và ứng dụng báo lỗi file bị khóa, hãy đóng terminal hoặc dừng app đang chạy trước đó bằng `Ctrl + C`.
- Nếu bạn dùng SQL Server Express, đảm bảo đã bật dịch vụ SQL Server và tên instance đúng trong `appsettings.json`.
- Nếu ứng dụng cảnh báo về `decimal` trong `MarketHistory`, đây chỉ là cảnh báo cấu hình kiểu giá trị. Ứng dụng vẫn chạy được.
