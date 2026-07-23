const API_BASE_URL = "http://localhost:5036/api";

function getAuthToken() {
    return localStorage.getItem("jwt_token");
}

function getCurrentUser() {
    const userJson = localStorage.getItem("jwt_user");
    return userJson ? JSON.parse(userJson) : null;
}

function setAuthSession(token, user) {
    localStorage.setItem("jwt_token", token);
    localStorage.setItem("jwt_user", JSON.stringify(user));
}

function clearAuthSession() {
    localStorage.removeItem("jwt_token");
    localStorage.removeItem("jwt_user");
    window.location.href = "/Auth/Login";
}

function getAuthHeaders() {
    const token = getAuthToken();
    const headers = {
        "Content-Type": "application/json"
    };
    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }
    return headers;
}

/**
 * Custom Friendly Alert Modal (Replaces browser alert)
 */
function showCustomAlert(message, title = "Thông Báo Hệ Thống", type = "info") {
    let modalId = "custom-global-alert-modal";
    let existing = document.getElementById(modalId);
    if (existing) existing.remove();

    let headerBg = "bg-primary text-white";
    if (type === "error" || type === "danger") headerBg = "bg-danger text-white";
    if (type === "success") headerBg = "bg-success text-white";
    if (type === "warning") headerBg = "bg-warning text-dark";

    const modalHtml = `
        <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow">
                    <div class="modal-header ${headerBg} py-3">
                        <h5 class="modal-title fw-bold fs-6">${title}</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4 fs-6">
                        ${message}
                    </div>
                    <div class="modal-footer border-top-0 pt-0">
                        <button type="button" class="btn btn-primary-gradient px-4" data-bs-dismiss="modal">Đã Hiểu</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modalEl = document.getElementById(modalId);
    const bsModal = new bootstrap.Modal(modalEl);
    bsModal.show();
}

/**
 * Custom Friendly Confirm Modal (Replaces browser confirm)
 */
function showCustomConfirm(message, onConfirm, title = "Xác Nhận Thao Tác") {
    let modalId = "custom-global-confirm-modal";
    let existing = document.getElementById(modalId);
    if (existing) existing.remove();

    const modalHtml = `
        <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow">
                    <div class="modal-header bg-primary text-white py-3">
                        <h5 class="modal-title fw-bold fs-6">${title}</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4 fs-6">
                        ${message}
                    </div>
                    <div class="modal-footer border-top-0 pt-0">
                        <button type="button" class="btn btn-light px-3 me-2" data-bs-dismiss="modal">Hủy Thao Tác</button>
                        <button type="button" class="btn btn-danger px-4 fw-bold" id="${modalId}-confirm-btn">Xác Nhận Đồng Ý</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modalEl = document.getElementById(modalId);
    const bsModal = new bootstrap.Modal(modalEl);

    document.getElementById(`${modalId}-confirm-btn`).addEventListener("click", () => {
        bsModal.hide();
        if (typeof onConfirm === "function") onConfirm();
    });

    bsModal.show();
}

/**
 * Client-Side Navigation Guard (Role Guard)
 * @param {Array<string>} allowedRoles Danh sách các vai trò được phép truy cập
 */
function requireRole(allowedRoles) {
    const token = getAuthToken();
    const user = getCurrentUser();

    if (!token || !user) {
        window.location.href = "/Auth/Login";
        return false;
    }

    if (allowedRoles && allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
        showCustomAlert(`Bạn không có quyền truy cập trang này. Yêu cầu quyền: ${allowedRoles.join(', ')}.`, "Quyền Truy Cập Từ Chối", "danger");
        if (user.role === "Admin") window.location.href = "/Admin/Dashboard";
        else if (user.role === "Staff") window.location.href = "/Staff/Reception";
        else if (user.role === "Doctor") window.location.href = "/Doctor/Workspace";
        else window.location.href = "/";
        return false;
    }

    return true;
}

/**
 * Render Admin Sub-Navbar with active link highlighting
 * @param {string} containerId ID of container element to render nav inside
 */
function renderAdminSubNav(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const currentPath = window.location.pathname.toLowerCase();

    const navItems = [
        { name: "Dashboard", url: "/admin/dashboard" },
        { name: "Ca Trực Bác Sĩ", url: "/admin/schedules" },
        { name: "Tạo Tài Khoản", url: "/admin/createaccount" },
        { name: "Chuyên Khoa", url: "/admin/specializations" },
        { name: "Bác Sĩ", url: "/admin/doctors" },
        { name: "Quầy Lễ Tân", url: "/staff/reception" }
    ];

    let html = '<div class="admin-subnav-container"><ul class="admin-nav">';
    navItems.forEach(item => {
        const isActive = currentPath === item.url.toLowerCase() ? "active" : "";
        html += `<li class="admin-nav-item"><a class="admin-nav-link ${isActive}" href="${item.url}">${item.name}</a></li>`;
    });
    html += '</ul></div>';

    container.innerHTML = html;
}

function renderAuthNav() {
    const user = getCurrentUser();
    const authNavContainer = document.getElementById("auth-nav-container");
    const roleNavContainer = document.getElementById("role-nav-container");

    if (!authNavContainer || !roleNavContainer) return;

    if (user) {
        let roleBadgeColor = "bg-secondary";
        if (user.role === "Admin") roleBadgeColor = "bg-danger";
        if (user.role === "Staff") roleBadgeColor = "bg-warning text-dark";
        if (user.role === "Doctor") roleBadgeColor = "bg-info text-dark";
        if (user.role === "Patient") roleBadgeColor = "bg-success";

        authNavContainer.innerHTML = `
            <li class="nav-item me-2 d-flex align-items-center">
                <span class="badge ${roleBadgeColor} me-2">${user.role}</span>
                <a href="/Auth/Profile" class="fw-semibold text-dark text-decoration-none me-3" title="Xem hồ sơ cá nhân">
                    ${user.fullName}
                </a>
            </li>
            <li class="nav-item me-2">
                <a class="btn btn-outline-secondary btn-sm rounded-pill px-3" href="/Auth/Profile">
                    Hồ Sơ
                </a>
            </li>
            <li class="nav-item">
                <button class="btn btn-outline-danger btn-sm rounded-pill px-3" onclick="clearAuthSession()">
                    Đăng xuất
                </button>
            </li>
        `;

        let roleLinks = "";
        if (user.role === "Admin") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-bold text-primary" href="/Admin/Dashboard">Admin Portal</a></li>
            `;
        } else if (user.role === "Staff") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Staff/Reception">Quầy Lễ Tân</a></li>
            `;
        } else if (user.role === "Doctor") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Doctor/Workspace">Workspace Bác Sĩ</a></li>
            `;
        } else if (user.role === "Patient") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Patient/Booking">Đặt Lịch Khám</a></li>
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Patient/History">Lịch Sử Cuộc Hẹn</a></li>
            `;
        }
        roleNavContainer.innerHTML = roleLinks;

    } else {
        authNavContainer.innerHTML = `
            <li class="nav-item">
                <a class="btn btn-primary-gradient px-4 rounded-pill fw-semibold" href="/Auth/Login">
                    Đăng Nhập
                </a>
            </li>
        `;
        roleNavContainer.innerHTML = "";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    renderAuthNav();
});
