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
                <span class="fw-semibold text-dark me-3"><i class="bi bi-person-circle me-1"></i>${user.fullName}</span>
            </li>
            <li class="nav-item">
                <button class="btn btn-outline-danger btn-sm rounded-pill" onclick="clearAuthSession()">
                    <i class="bi bi-box-arrow-right me-1"></i>Đăng xuất
                </button>
            </li>
        `;

        // Render role-specific navigation links
        let roleLinks = "";
        if (user.role === "Admin") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Admin/Specializations"><i class="bi bi-bookmark-plus me-1"></i>Chuyên khoa</a></li>
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Admin/Doctors"><i class="bi bi-person-badge me-1"></i>Bác sĩ</a></li>
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Staff/Reception"><i class="bi bi-reception-4 me-1"></i>Lễ tân</a></li>
            `;
        } else if (user.role === "Staff") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Staff/Reception"><i class="bi bi-reception-4 me-1"></i>Quầy Lễ Tân</a></li>
            `;
        } else if (user.role === "Doctor") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Doctor/Workspace"><i class="bi bi-clipboard-pulse me-1"></i>Phòng Khám Bác Sĩ</a></li>
            `;
        } else if (user.role === "Patient") {
            roleLinks = `
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Patient/Booking"><i class="bi bi-calendar-plus me-1"></i>Đặt Lịch Khám</a></li>
                <li class="nav-item"><a class="nav-link fw-semibold" href="/Patient/History"><i class="bi bi-clock-history me-1"></i>Lịch Sử Cuộc Hẹn</a></li>
            `;
        }
        roleNavContainer.innerHTML = roleLinks;

    } else {
        authNavContainer.innerHTML = `
            <li class="nav-item">
                <a class="btn btn-primary-gradient px-4 rounded-pill" href="/Auth/Login">
                    <i class="bi bi-box-arrow-in-right me-1"></i>Đăng Nhập
                </a>
            </li>
        `;
        roleNavContainer.innerHTML = "";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    renderAuthNav();
});
