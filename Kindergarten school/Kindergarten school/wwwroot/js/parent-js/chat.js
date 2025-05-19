const connectionParent = new signalR.HubConnectionBuilder()
    .withUrl("/chat-hub")
    .build();

connectionParent.start().then(() => {
    console.log("Connected to ChatHub!");

}).catch(err => console.error(err));


let parentId = $("#ParentId").val(); // Lấy ID phụ huynh từ Razor
let conversationId = null;
let selectedTeacherId = null;

// 🟢 Khi click vào giáo viên, lấy cuộc trò chuyện
$(".nav-user").click(function () {
    selectedTeacherId = $(this).attr("data-teacherid");

    // Xóa nội dung chat cũ
    $("#messageChannel").empty();
    $("#sendMessage").prop("disabled", true);
    $("#bottomSend").show();

    // Gửi request lấy ID hội thoại
    $.get(`/parent/conversation/${parentId}/${selectedTeacherId}`, function (data) {
        conversationId = data;
        loadMessages(conversationId);
        $("#sendMessage").prop("disabled", false); // Kích hoạt nút gửi tin
        if (conversationId) {
            console.log(conversationId)
            connectionParent.invoke("JoinConversation", conversationId);
        }
    });

    // Đánh dấu giáo viên đang được chọn
    $(".nav-user").removeClass("active");
    $(this).addClass("active");
});

// 🟢 Load tin nhắn của cuộc trò chuyện
function loadMessages(conversationId) {
    $.get(`/parent/messages/${conversationId}`, function (messages) {
        $("#messageChannel").empty();
        messages.forEach(msg => {
            let isUser = msg.senderId === parentId;
            let messageHtml = `
                        <div class="message ${isUser ? 'this-user' : ''}">
                            ${isUser ? '' : '<img class="pfp-small" src="/images/8065351.png"/>'}

                            <div class="message-content">
                                <p>${msg.message}</p>

                            </div>
                            ${isUser ? '<img class="pfp-small" src="/images/2829825.png"/>' : ''}

                        </div>`;
            $("#messageChannel").append(messageHtml);
        });
        scrollToBottom();
    });
}

// 🟢 Gửi tin nhắn
$("#sendMessage").click(function () {
    let messageText = $("#messageInput").val().trim();
    if (messageText === "" || !conversationId) return;

    $.post(`/parent/send`, {
        conversationId: conversationId,
        senderId: parentId,
        message: messageText
    }, function () {
        let messageHtml = `
                    <div class="message this-user">
                        <div class="message-content">
                            <p>${messageText}</p>
                        </div>
                        <img class="pfp-small" src="/images/2829825.png"/>
                    </div>`;
        $("#messageChannel").append(messageHtml);
        $("#messageInput").val(""); // Xóa ô nhập
        scrollToBottom();
    });
});
connectionParent.on("ReceiveMessageChat", (senderId, message) => {
    if (senderId == parentId) return;

    let messageHtml = `
        <div class="message">
            <img class="pfp-small" src="/images/8065351.png"/>
            <div class="message-content">
                <p>${message}</p>
            </div>
        </div>`;

    $("#messageChannel").append(messageHtml);
    scrollToBottom();
});

function scrollToBottom() {
    let messageChannel = $("#messageChannel");
    messageChannel.scrollTop(messageChannel[0].scrollHeight);
}
