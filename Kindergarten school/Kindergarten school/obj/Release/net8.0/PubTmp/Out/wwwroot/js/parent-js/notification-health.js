function formatDate(date) {
    const options = {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    };
    return date.toLocaleDateString('vi-VN', options);
}

const healthConnection = new signalR.HubConnectionBuilder()
    .withUrl("/health-hub")
    .configureLogging(signalR.LogLevel.Information)
    .build();



// Bắt đầu cả hai kết nối
healthConnection.start()
    .then(() => console.log("✅ Kết nối healthConnection thành công!"))
    .catch(err => console.error("❌ Kết nối healthConnection thất bại:", err));



// Khi nhận thông báo gửi đến học sinh
healthConnection.on("HealthReportToStudent", function (content, createDate,name) {
    addNotificationHealth(content, createDate, name);
});


// Hàm thêm thông báo mới lên đầu danh sách
function addNotificationHealth(content, createDate, name) {
    var newNotification = `
        <div class="card mb-3 border-left-primary shadow-sm animate__animated animate__fadeInDown">
            <div class="card-body">
                <h5 class="card-title text-primary font-weight-bold">
                    🏥 ${content}
                </h5>
                <p class="card-text text-muted">
                    👦 Học sinh: <strong>${name}</strong>
                </p>
                <p class="card-text">
                    📅 Ngày cập nhật: <strong>${formatDate(new Date(createDate))}</strong>
                </p>
            </div>
        </div>`;

    // Thêm thông báo lên đầu danh sách
    document.querySelector("#cardHealth").insertAdjacentHTML("afterbegin", newNotification);
}
