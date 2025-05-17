

// Khi chọn loại thông báo
$("#type").change(function () {
    var selectedType = $(this).val();

    if (selectedType === "ToAll") {
        $("#selectClass").hide();
        $("#selectUser").hide();
    } else if (selectedType === "ToClass") {
        $("#selectClass").show();
        $("#selectUser").hide();
       
    } else if (selectedType === "ToStudent") {
        $("#selectClass").hide();
        $("#selectUser").show();
      
    }
});

$("#formNotification").submit(function (event) {
    event.preventDefault(); 

    var formData = new FormData();
    formData.append("type", $("#type").val());
    formData.append("content", $("#content").val());

    if ($("#type").val() === "ToClass") {
        formData.append("classId", $("#ClassID").val());
    } else if ($("#type").val() === "ToStudent") {
        formData.append("studentId", $("#StudentID").val());
    }

    // Gửi AJAX
    $.ajax({
        url: "/admin/notification/create",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Đã thông báo thành công',
                confirmButtonText: 'OK'
            });
        },
        error: function (xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: `Đã xảy ra lỗi`,
                confirmButtonText: 'OK'
            }).then((result) => {
                if (result.isConfirmed) {
                    return;
                }
            });
           
        }
    });
});

