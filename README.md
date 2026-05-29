# Hệ thống Phân vùng & Dịch thuật Truyện tranh Tự động (Manga AI Segmentation & Translation)

Dự án này là một hệ thống Full-stack tích hợp Trí tuệ Nhân tạo (AI) giúp tự động phát hiện các bong bóng thoại (Speech Bubbles) trong truyện tranh, trích xuất chữ (OCR), dịch thuật sang tiếng Việt và vẽ đè bản dịch lên ảnh gốc ngay trên giao diện Web.

Dự án được tổ chức theo cấu trúc **Monorepo** (tất cả các thành phần nằm chung trong một thư mục gốc) giúp nhóm dễ dàng quản lý, phát triển và cấu hình.

---

## 🏗️ Kiến trúc Hệ thống (Monorepo Structure)

Thư mục gốc của dự án bao gồm 3 thành phần cốt lõi:

1. **`Manga_Frontend/`**: Giao diện người dùng (HTML5 Canvas & Vanilla JavaScript). Chịu trách nhiệm hiển thị ảnh, gửi yêu cầu phân tích và vẽ các khối văn bản đã dịch.
2. **`MangaPortal_Backend/MangaAPI/`**: Hệ thống kết nối trung gian (Backend C# ASP.NET Core Web API & SQL Server). Chịu trách nhiệm quản lý logic nghiệp vụ, lưu trữ tọa độ/bản dịch vào Database và điều phối luồng dữ liệu giữa Frontend và AI.
3. **`MangaAI_API/`**: Bộ não trí tuệ nhân tạo (Python FastAPI). Tích hợp mô hình **YOLO11** để phân vùng vật thể (Segmentation) và thư viện **EasyOCR** kết hợp với **Google Translate API** để đọc chữ và dịch thuật.

---

## ⚙️ Yêu cầu Hệ thống (Prerequisites)

Trước khi khởi động dự án, các thành viên trong đội ngũ phát triển cần cài đặt sẵn các công cụ sau:
* **C# Backend**: [.NET SDK 6.0 hoặc mới hơn](https://dotnet.microsoft.com/download)
* **Python AI**: [Python 3.10 hoặc mới hơn](https://www.python.org/downloads/)
* **Cơ sở dữ liệu**: SQL Server & [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
* **Frontend Tool**: Extension **Live Server** trên VS Code.

---

## 🚀 Hướng dẫn Cấu hình & Khởi chạy Hệ thống

Để hệ thống hoạt động đồng bộ, bạn cần mở các cổng dịch vụ theo đúng thứ tự dưới đây. 

> 💡 **Mẹo phát triển**: Bạn nên sử dụng tính năng **Split Terminal** trong VS Code để chạy đồng thời cả Python và C# Backend trên cùng một cửa sổ quản lý.

### 1. Khởi chạy Bộ não AI (Python FastAPI - Port 8000)

Do dự án đã được chuyển vị trí vào thư mục Monorepo, môi trường ảo `venv` cần được khởi tạo lại tại thư mục mới để tránh lỗi đường dẫn hệ thống.

Mở một Terminal mới trong VS Code và chạy chuỗi lệnh sau:

# Di chuyển vào thư mục AI
cd MangaAI_API

# Xóa môi trường ảo cũ (nếu có) và tạo môi trường ảo mới
Remove-Item -Recurse -Force venv
python -m venv venv

# Kích hoạt môi trường ảo (Nếu Windows chặn, xem mục Sửa lỗi ở cuối file)
.\venv\Scripts\Activate.ps1

# Cài đặt toàn bộ các thư viện AI cần thiết
pip install uvicorn fastapi ultralytics pillow python-multipart easyocr deep-translator

# Khởi động dịch vụ AI cổng 8000
uvicorn main:app --reload

Khi màn hình hiển thị Uvicorn running on http://127.0.0.1:8000, cổng AI đã sẵn sàng.

2. Khởi chạy Trạm điều phối (C# Backend - Port 5293)
Mở một Terminal thứ hai (Split Terminal) bên cạnh Terminal Python và gõ các lệnh sau:

Bash
# Di chuyển vào thư mục chứa file dự án C#
cd MangaPortal_Backend\MangaAPI

# Cấu hình Cơ sở dữ liệu (Nếu chạy lần đầu)
# Hãy đảm bảo Connection String trong file 'appsettings.json' đã trỏ đúng về SQL Server của bạn
dotnet ef database update

# Khởi động Web API Backend
dotnet run
Khi màn hình hiển thị Now listening on: http://localhost:5293, cổng kết nối trung gian đã sẵn sàng.

3. Khởi chạy Giao diện (HTML/JS Frontend - Port 5500)
Trong cây thư mục của VS Code, tìm đến file Manga_Frontend/index.html.

Click chuột phải vào file index.html và chọn Open with Live Server.

Trình duyệt Cốc Cốc / Chrome của bạn sẽ tự động mở trang web tại địa chỉ http://127.0.0.1:5500/Manga_Frontend/index.html.

🕹️ Quy trình Sử dụng Hệ thống (Workflow)
Khi giao diện web đã mở thành công, bạn thực hiện theo các bước sau để chạy toàn bộ luồng AI:

Chọn tệp: Bấm nút chọn một bức ảnh truyện tranh (Tiếng Anh/Tiếng Nhật) từ máy tính của bạn.

Bước 1 - Phân vùng AI: Bấm nút "1. Phân vùng AI".

Luồng đi: Web gửi ảnh sang C# -> C# bắn sang Python YOLO11 -> Python trả về danh sách tọa độ khung -> C# lưu các khung này vào bảng PageRegion trong SQL Server -> Trả về Web để vẽ các khung màu đỏ nhạt lên màn hình.

Bước 2 - Dịch tự động: Bấm nút "2. Dịch tự động".

Luồng đi: Web gửi yêu cầu dịch -> C# lấy danh sách khung đỏ lên, đóng gói kèm ảnh gửi sang Python -> Python tiến hành cắt ảnh (Crop), làm nét ảnh đầu vào -> Đưa qua EasyOCR đọc chữ văn bản -> Gửi qua Google Translate dịch sang tiếng Việt -> Trả kết quả về C# lưu đè vào cột original_text -> Web nhận dữ liệu bản dịch thật, tô nền trắng xóa chữ gốc và căn giữa khối chữ tiếng Việt lên Canvas.

📝 Câu lệnh Kiểm tra Dữ liệu (Database Verification)
Để kiểm tra xem hệ thống đã lưu thành công các tọa độ phân vùng và nội dung dịch thuật của AI vào hệ quản trị cơ sở dữ liệu hay chưa, các thành viên trong nhóm có thể mở SSMS và chạy câu lệnh SQL sau:

SQL
-- Trỏ vào đúng database của dự án
USE MangaManagementDB;

-- Kiểm tra bảng lưu trữ vùng truyện tranh và nội dung dịch thuật
SELECT page_region_id, chapter_page_version_id, x, y, width, height, original_text, type_code 
FROM manga.PageRegion
ORDER BY page_region_id DESC;
🛠️ Sổ tay Sửa lỗi nhanh cho Đội ngũ (Troubleshooting)
🚨 Lỗi 1: Không kích hoạt được venv trên PowerShell (UnauthorizedAccess)
Triệu chứng: Khi chạy lệnh .\venv\Scripts\Activate.ps1 báo lỗi đỏ chữ do hệ thống Windows chặn script.

Cách fix: Chạy lệnh giải phóng quyền cho User hiện tại ngay tại terminal đó rồi kích hoạt lại:

PowerShell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
# Gõ 'Y' khi hệ thống hỏi, sau đó chạy lại lệnh kích hoạt.
🚨 Lỗi 2: Bị treo hoặc lỗi Timeout khi bấm nút "Dịch tự động" lần đầu
Triệu chứng: Giao diện báo lỗi Timeout of 100 seconds elapsing hoặc đứng im rất lâu.

Nguyên nhân: Ở lần chạy đầu tiên, thư viện easyocr của Python bắt buộc phải tải mô hình nhận diện ngôn ngữ (Model Weights nặng vài trăm MB) từ máy chủ về máy cục bộ nên sẽ tốn từ 2-3 phút tùy tốc độ mạng.

Cách fix: Chúng ta đã nâng cấp cấu hình chờ trong file MangaAiService.cs lên 5 phút (_httpClient.Timeout = TimeSpan.FromMinutes(5);). Đội phát triển chỉ cần kiên nhẫn đợi ở lần dịch đầu tiên, từ lần thứ 2 trở đi hệ thống sẽ phản hồi gần như lập tức do model đã được lưu trong bộ nhớ đệm (cache).