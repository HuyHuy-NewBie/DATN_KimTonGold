# Vai Tro Model (GoldManagementSystem)

Tai lieu nay tom tat nhiem vu, nghiep vu, va vai tro cua tung model trong he thong.

## Domain Models (Entities)

- AppUser
  Nhiem vu: Dai dien nguoi dung he thong (nhan vien/admin) voi thong tin tai khoan va Identity.
  Nghiep vu: Quan ly tai khoan va phan quyen.
  Vai tro: Xac thuc/phan quyen va gan nguoi thuc hien giao dich.

- Branch
  Nhiem vu: Dai dien chi nhanh kinh doanh (ten, dia chi, sdt, trang thai).
  Nghiep vu: Quan ly chi nhanh.
  Vai tro: Noi lien ket nguoi dung, san pham, don hang.

- Product
  Nhiem vu: Dai dien san pham vang (gia, khoi luong, trang thai, hinh anh).
  Nghiep vu: Quan ly ton kho va danh muc san pham.
  Vai tro: Mat hang chinh de ban/mua, gan voi Branch, ho tro tach danh sach hinh.

- FavoriteProduct
  Nhiem vu: Luu san pham yeu thich cua nguoi dung.
  Nghiep vu: So thich/quan tam san pham.
  Vai tro: Bang lien ket nhieu-nhieu giua AppUser va Product.

- Order
  Nhiem vu: Dai dien giao dich ban hang (tong tien, khach hang, trang thai).
  Nghiep vu: Xu ly giao dich.
  Vai tro: Goc giao dich, lien ket User va Branch.

- OrderDetail
  Nhiem vu: Chi tiet tung san pham trong Order (san pham, so luong, don gia).
  Nghiep vu: Chi tiet giao dich.
  Vai tro: Con cua Order, gan Product vao giao dich.

- MarketHistory
  Nhiem vu: Luu lich su gia thi truong (vang/bac/ty gia).
  Nghiep vu: Du lieu gia thi truong.
  Vai tro: Nguon du lieu cap nhat gia va bao cao lich su.

- MarketDashboardSnapshot
  Nhiem vu: Tong hop thong tin gia thi truong de hien thi dashboard.
  Nghiep vu: Hien thi du lieu gia thi truong.
  Vai tro: DTO phuc vu giao dien (khong luu tren DB).

- PreciousMetalSnapshot / MarketChangeSnapshot / MarketSourcePriceSnapshot
  Nhiem vu: Cac cau truc con de mo ta gia, bien dong, va nguon gia.
  Nghiep vu: Hien thi du lieu gia thi truong.
  Vai tro: Cau truc ho tro ben trong MarketDashboardSnapshot.

## API/Integration Models

- ExchangeRate
  Nhiem vu: Dai dien du lieu ty gia lay tu API ben ngoai.
  Nghiep vu: Du lieu ngoai he thong.
  Vai tro: DTO de doc/phan tich du lieu API, khong la entity EF.

## View Models (UI/Forms)

- ProductFormViewModel
  Nhiem vu: Model nhap lieu tao/sua san pham.
  Nghiep vu: Quan ly san pham (UI).
  Vai tro: Rang buoc validate cho form.

- BranchManagementViewModel
  Nhiem vu: Model nhap + danh sach quan ly chi nhanh.
  Nghiep vu: Quan ly chi nhanh (UI).
  Vai tro: Gom BranchManagementItemViewModel de hien thi danh sach.

- BranchManagementItemViewModel
  Nhiem vu: Dong thong tin tom tat tung chi nhanh.
  Nghiep vu: Quan ly chi nhanh (UI).
  Vai tro: Hien thi nhanh thong tin va thong ke.

- UserViewModel
  Nhiem vu: Dong thong tin user (vai tro, trang thai).
  Nghiep vu: Quan ly tai khoan (UI).
  Vai tro: Hien thi danh sach user.

- UserProfileViewModel
  Nhiem vu: Model cap nhat ho so user.
  Nghiep vu: Tu quan ly thong tin (UI).
  Vai tro: Form cap nhat ho so va theo doi thay doi dang cho.

## Common

- ErrorViewModel
  Nhiem vu: Thong tin loi chung cho man hinh loi.
  Nghiep vu: Xu ly loi giao dien.
  Vai tro: Model hien thi trang loi.
