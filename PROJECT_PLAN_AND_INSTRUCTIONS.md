# CLINIC APPOINTMENT BOOKING SYSTEM - PROJECT MASTER PLAN & AGENT INSTRUCTIONS (UPDATED WITH GRACE PERIOD)

> **Note for AntiGravity Agent:** Document này chứa toàn bộ định hướng kiến trúc, ràng buộc kỹ thuật, quy trình nghiệp vụ đã được tối ưu sát thực tế (bao gồm Grace Period 15 phút khi hủy lịch) và tiến độ thực hiện của dự án. Hãy tuân thủ nghiêm ngặt các quy chuẩn kỹ thuật và danh sách công việc được phân công bên dưới trong từng giai đoạn phát triển.

---

## 1. TỔNG QUAN DỰ ÁN (PROJECT OVERVIEW)

* **Tên dự án:** Clinic Appointment Booking System
* **Mục tiêu:** Xây dựng hệ thống Web App + RESTful API quản lý và đặt lịch khám bệnh trực tuyến cho phòng khám quy mô vừa và nhỏ.
* **Quy mô phát triển:** 02 Thành viên (Thành viên A & Thành viên B).
* **Kiến trúc chủ đạo:** Monolith Multi-Tier (ASP.NET Core 8 Web API + Razor Pages + jQuery AJAX).

---

## 2. RÀNG BUỘC KỸ THUẬT (TECHNICAL CONSTRAINTS & STACK)

AntiGravity cần tuân thủ đúng danh sách công việc và bộ công nghệ (Tech Stack) sau:

### 2.1. Backend Tech Stack
* **Framework:** ASP.NET Core 8.0 Web API.
* **ORM & DB:** Entity Framework Core 8.0 (Code-First), SQL Server.
* **Concurrency Control:** Sử dụng Database Transaction khi Đặt lịch để chống trùng Slot.
* **Querying:** ASP.NET Core OData 8.x cho các endpoint truy vấn danh sách (`$filter`, `$orderby`, `$select`, `$top`, `$skip`).
* **Authentication & Authorization:** JWT (JSON Web Token), ASP.NET Core Identity / BCrypt. Sử dụng Role-Based Authorization (`Patient`, `Doctor`, `Staff`, `Admin`).
* **Validation:** FluentValidation (tách biệt hoàn toàn validation logic khỏi Controller/DTOs).
* **Documentation & Testing:** Swagger / OpenAPI, Postman.

### 2.2. Frontend Tech Stack
* **Framework:** ASP.NET Core Razor Pages (Server-Side Rendering kết hợp AJAX).
* **Client Interactivity:** jQuery, Bootstrap 5, AJAX, DataTables.js.
* **AJAX Pattern:** Tất cả các thao tác tương tác dữ liệu (Đặt lịch, Hủy lịch, Check-in, Đổi trạng thái, Load danh sách OData) phải sử dụng `$.ajax()` bất đồng bộ, truyền JWT Token qua Request Header `Authorization: Bearer <token>`. Không lặp lại trang/reload toàn bộ.

---

## 3. CÁC TÁC NHÂN CHÍNH & LUỒNG NGHIỆP VỤ THỰC TẾ (ACTORS & WORKFLOW)

### 3.1. Các Tác Nhân (Actors)
1. **Patient (Bệnh nhân):** Tra cứu bác sĩ (qua OData API), xem khung giờ trống (Slot), đăng ký lịch hẹn, hủy lịch hẹn (tuân theo luật > 2 tiếng hoặc Grace Period 15 phút sau khi đặt thành công), xem lịch sử.
2. **Doctor (Bác sĩ):** Xem lịch khám cá nhân theo ngày, hoàn tất ca khám (`Completed`), cập nhật ghi chú chẩn đoán.
3. **Staff / Receptionist (Lễ tân):** Tiếp nhận bệnh nhân tại sảnh (`Check-in`), xử lý trường hợp bệnh nhân không đến (`No-Show`), hỗ trợ đặt/hủy lịch trực tiếp.
4. **Admin (Quản trị viên):** Quản lý bác sĩ, chuyên khoa, phòng khám và xem báo cáo thống kê qua OData.

### 3.2. Luồng Nghiệp Vụ Cốt Lõi & Quy Tắc Hủy Lịch (Flexible Cancellation Rule)

```
                    +----------------------------------------------+
                    |                                              |
                    v                                              |
  [ Booking ] ---> [ Confirmed ] ---> [ Checked-in ] ---> [ Completed ]
                    |                      |
                    +---> [ Cancelled ]    +---> [ No-Show ]
```

#### Quy Tắc Hủy Lịch Mềm Dẻo (Grace Period Rule):
Yêu cầu Hủy lịch (`PATCH /api/appointments/{id}/cancel`) được chấp nhận khi thỏa mãn **ÍI NHẤT 1 TRONG 2 ĐIỀU KIỆN**:
1. **Condition 1 (Theo giờ khám):** `(Slot.StartTime - DateTime.Now).TotalHours >= 2`
2. **Condition 2 (Grace Period 15 phút):** `(DateTime.Now - Appointment.CreatedAt).TotalMinutes <= 15` (Bất kể ca khám đó còn bao nhiêu phút nữa là bắt đầu).

*Nếu không thỏa mãn cả 2 điều kiện, Backend trả về HTTP Status Code `400 Bad Request` kèm thông báo lỗi rõ ràng.*

---

## 4. KẾ HOẠCH TRIỂN KHAI THEO TIẾN ĐỘ (MILESTONES & TASKS)

**THÀNH VIÊN A: LUỒNG VẬN HÀNH & QUẢN TRỊ (ADMIN & STAFF CORE)**
Thành viên A chịu trách nhiệm toàn bộ các tính năng dành cho Lễ tân, Quản trị viên và Bác sĩ.

### **Backend & Security (API Layer):**
- **Auth & Identity:** API Login/Register, cấp JWT Token, phân quyền Role-based (Admin, Staff, Doctor).
- **Category & Doctor Management API:** RESTful API + OData CRUD Chuyên khoa (Specialization), Hồ sơ Bác sĩ (Doctor).
- **Staff Reception API:** API Check-in (PATCH /check-in), API Đánh dấu bỏ hẹn (PATCH /no-show).
- **Doctor Dashboard API:** API xem danh sách ca khám theo ngày của bác sĩ, API cập nhật kết quả/ghi chú khám (PATCH /complete).

### **Frontend (Razor Pages + jQuery AJAX):**
- **Admin Portal:** Giao diện Quản lý Bác sĩ/Chuyên khoa, xem báo cáo danh sách bằng jQuery DataTables + OData filtering ($filter, $orderby).
- **Staff Reception UI:** Giao diện Lễ tân gõ SĐT/Mã hẹn để tìm kiếm nhanh, nút bấm thao tác nhanh Check-in và Đánh dấu No-Show.
- **Doctor Workspace UI:** Giao diện cho Bác sĩ xem danh sách bệnh nhân chờ khám và Form nhập ghi chú chẩn đoán.

**THÀNH VIÊN B: LUỒNG BỆNH NHÂN & ĐẶT LỊCH (PATIENT JOURNEY CORE)**
Thành viên B chịu trách nhiệm toàn bộ trải nghiệm của người dùng Bệnh nhân từ lúc tìm kiếm đến lúc đặt/hủy lịch.

### **Backend & Business Logic (API Layer):**
Schedule & Slot API: API tạo ca trực (Slot), OData API lấy danh sách slot rảnh (/api/schedules/available-slots).
Booking Core API: API Đặt lịch (POST /api/appointments) có xử lý DB Transaction chống trùng slot và FluentValidation cho dữ liệu đầu vào.
Cancellation API: API Hủy lịch (PATCH /api/appointments/{id}/cancel) tích hợp Quy tắc Grace Period 15 phút / Hủy trước > 2 tiếng và auto-restore slot.
Patient History API: API xem danh sách lịch sử đặt khám của bệnh nhân.
Frontend (Razor Pages + jQuery AJAX):
Public/Patient Home UI: Trang chủ tra cứu Bác sĩ theo chuyên khoa (gọi OData API).
Booking Flow UI: Giao diện chọn ngày, render danh sách Slot động, Form điền triệu chứng và xử lý Submit qua jQuery AJAX (bắt lỗi 400 Validation / 409 Conflict).
Patient Management UI: Trang cá nhân xem lịch sử khám, trạng thái cuộc hẹn và nút Hủy lịch hẹn.

---

## 5. BẢNG KIỂM TRA TIÊU CHÍ HOÀN THÀNH (CHECKLIST CHO ANTIGRAVITY)

- [ ] API tuân thủ đúng RESTful Naming Conventions & Return Status Codes (bao gồm 200, 201, 400, 401, 403, 404, 409).
- [ ] OData hoạt động chuẩn xác với `$filter`, `$orderby`, `$select`, `$top`, `$skip`.
- [ ] Tất cả Request Body DTOs đều đi qua FluentValidation.
- [ ] Sử dụng DB Transaction khi tạo Appointment để đảm bảo không bị trùng Slot.
- [ ] Hỗ trợ đầy đủ vòng đời cuộc hẹn: `Confirmed`, `CheckedIn`, `Completed`, `Cancelled`, `NoShow`.
- [ ] Xử lý Hủy lịch mềm dẻo với Grace Period 15 phút hoặc hủy trước > 2 tiếng, tự động hoàn trả Slot về `Available`.
- [ ] Tất cả endpoint riêng tư có đính kèm `[Authorize]` và `[Authorize(Roles = "...")]`.
- [ ] Giao diện Razor Pages gọi API 100% bằng jQuery AJAX, không submit form truyền thống reload trang.
- [ ] Đã publish và test thành công bằng Swagger & Postman.
