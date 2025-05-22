


$("#formHelthNotification").submit(function (event) {
    event.preventDefault(); 

    var formData = new FormData();
    formData.append("studentId", $("#studentId").val());
    formData.append("HealthStatus", $("#HealthStatus").val());

  

    // Gửi AJAX
    $.ajax({
        url: "/admin/health-report/create",
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

