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

// Kết nối đến SchoolHub
const schoolConnection = new signalR.HubConnectionBuilder()
    .withUrl("/school-hub")
    .configureLogging(signalR.LogLevel.Information)
    .build();



// Bắt đầu cả hai kết nối
schoolConnection.start()
    .then(() => console.log("✅ Kết nối SchoolHub thành công!"))
    .catch(err => console.error("❌ Kết nối SchoolHub thất bại:", err));



// Khi nhận thông báo gửi đến lớp
schoolConnection.on("SendMessageToClass", function (content, createDate) {
    addNotification(content, createDate);
});

// Khi nhận thông báo gửi đến học sinh
schoolConnection.on("SendMessageToStudent", function (content, createDate) {
    addNotification(content, createDate);
});

// Khi nhận thông báo gửi đến tất cả mọi người
schoolConnection.on("SendMessageToAll", function (content, createDate) {
    addNotification(content, createDate);
});

// Hàm thêm thông báo mới lên đầu danh sách
function addNotification(content, createDate) {
 

    var newNotification = `
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">${content}</h5>
                    <p class="card-text">${formatDate(new Date(createDate))}</p>
                </div>
            </div>`;

    // Thêm thông báo lên đầu danh sách
    document.querySelector(".card-body").insertAdjacentHTML("afterbegin", newNotification);
}

