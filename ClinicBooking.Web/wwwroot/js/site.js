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

function clearAuthSession(returnUrl) {
    localStorage.removeItem("jwt_token");
    localStorage.removeItem("jwt_user");

    if (returnUrl) {
        redirectToLogin(returnUrl);
        return;
    }

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

function handleAuthError(xhr, returnUrl) {
    if (xhr.status !== 401 && xhr.status !== 403) {
        return false;
    }

    clearAuthSession(returnUrl);
    return true;
}

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value ?? "";
    return element.innerHTML;
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
function getLocalReturnUrl(url = window.location.href) {
    const targetUrl = new URL(url, window.location.origin);
    if (targetUrl.origin !== window.location.origin) return "/";

    return `${targetUrl.pathname}${targetUrl.search}` || "/";
}

function redirectToLogin(returnUrl) {
    window.location.href = `/Auth/Login?returnUrl=${encodeURIComponent(returnUrl)}`;
}

function requireRole(allowedRoles) {
    const token = getAuthToken();
    const user = getCurrentUser();

    if (!token || !user) {
        redirectToLogin(getLocalReturnUrl());
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
        { name: "Tổng quan", icon: "bi-grid-1x2", url: "/admin/dashboard" },
        { name: "Ca trực", icon: "bi-calendar3", url: "/admin/schedules" },
        { name: "Tạo tài khoản", icon: "bi-person-plus", url: "/admin/createaccount" },
        { name: "Chuyên khoa", icon: "bi-bookmark-heart", url: "/admin/specializations" },
        { name: "Bác sĩ", icon: "bi-person-badge", url: "/admin/doctors" },
        { name: "Lễ tân", icon: "bi-reception-4", url: "/staff/reception" }
    ];

    let html = '<nav class="admin-subnav-container" aria-label="Điều hướng quản trị"><ul class="admin-nav">';
    navItems.forEach(item => {
        const isActive = currentPath === item.url.toLowerCase() ? "active" : "";
        const ariaCurrent = isActive ? ' aria-current="page"' : "";
        html += `
            <li class="admin-nav-item">
                <a class="admin-nav-link ${isActive}" href="${item.url}"${ariaCurrent}>
                    <i class="bi ${item.icon}" aria-hidden="true"></i>
                    <span>${item.name}</span>
                </a>
            </li>`;
    });
    html += '</ul></nav>';

    container.innerHTML = html;
}

function renderAuthNav() {
    const user = getCurrentUser();
    const authNavContainer = document.getElementById("auth-nav-container");
    const roleNavContainer = document.getElementById("role-nav-container");

    if (!authNavContainer || !roleNavContainer) return;

    const currentPath = window.location.pathname.toLowerCase();
    const roleLabels = {
        Admin: "Quản trị viên",
        Staff: "Nhân viên lễ tân",
        Doctor: "Bác sĩ",
        Patient: "Bệnh nhân"
    };
    const renderRoleLinks = (items) => items.map(item => {
        const isActive = currentPath === item.url.toLowerCase();
        return `
            <li class="nav-item">
                <a class="nav-link${isActive ? " active" : ""}" href="${item.url}"${isActive ? ' aria-current="page"' : ""}>
                    <i class="bi ${item.icon}" aria-hidden="true"></i>${item.name}
                </a>
            </li>`;
    }).join("");

    if (user) {
        const fullName = escapeHtml(user.fullName || "Người dùng");
        const initial = escapeHtml((user.fullName || "U").trim().charAt(0).toUpperCase());
        const roleLabel = roleLabels[user.role] || escapeHtml(user.role);

        authNavContainer.innerHTML = `
            <li class="nav-item dropdown">
                <button class="user-menu dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <span class="user-avatar" aria-hidden="true">${initial}</span>
                    <span class="user-copy">
                        <strong>${fullName}</strong>
                        <small>${roleLabel}</small>
                    </span>
                </button>
                <ul class="dropdown-menu dropdown-menu-end">
                    <li>
                        <a class="dropdown-item" href="/Auth/Profile">
                            <i class="bi bi-person-circle me-2" aria-hidden="true"></i>Hồ sơ cá nhân
                        </a>
                    </li>
                    <li><hr class="dropdown-divider"></li>
                    <li>
                        <button class="dropdown-item text-danger" type="button" onclick="clearAuthSession()">
                            <i class="bi bi-box-arrow-right me-2" aria-hidden="true"></i>Đăng xuất
                        </button>
                    </li>
                </ul>
            </li>
        `;

        let navItems = [];
        if (user.role === "Admin") {
            navItems = [
                { name: "Tổng quan", icon: "bi-grid-1x2", url: "/Admin/Dashboard" }
            ];
        } else if (user.role === "Staff") {
            navItems = [
                { name: "Quầy lễ tân", icon: "bi-reception-4", url: "/Staff/Reception" }
            ];
        } else if (user.role === "Doctor") {
            navItems = [
                { name: "Không gian làm việc", icon: "bi-clipboard2-pulse", url: "/Doctor/Workspace" }
            ];
        } else if (user.role === "Patient") {
            navItems = [
                { name: "Đặt lịch khám", icon: "bi-calendar2-plus", url: "/Booking/Index" },
                { name: "Lịch hẹn của tôi", icon: "bi-clock-history", url: "/Patient/History" }
            ];
        }
        roleNavContainer.innerHTML = renderRoleLinks(navItems);

    } else {
        authNavContainer.innerHTML = currentPath === "/auth/login"
            ? ""
            : `
                <li class="nav-item">
                    <a class="btn btn-primary-gradient px-4" href="/Auth/Login">
                        <i class="bi bi-box-arrow-in-right me-1" aria-hidden="true"></i>Đăng nhập
                    </a>
                </li>
            `;
        roleNavContainer.innerHTML = renderRoleLinks([
            { name: "Đặt lịch khám", icon: "bi-calendar2-plus", url: "/Booking/Index" }
        ]);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    renderAuthNav();
});
