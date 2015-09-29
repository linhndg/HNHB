/* Your JS codes here */
function validateLogin() {
    var check = false;
    var x = document.forms["loginForm"]["UserName"].value;
    if (x == null || x == "") {
        $.amaran({
            content: {
                bgcolor: '#ED5441',
                color: '#fff',
                message: '<strong>Xin lỗi!</strong><p style="font-size:12px">Vui lòng nhập tên đăng nhập</p>'
            },
            theme: 'colorful',
            position: 'top right',
            delay: 1000
        });
        return false;
    }
    var y = document.forms["loginForm"]["Password"].value;
    if (y == null || y == "") {
        $.amaran({
            content: {
                bgcolor: '#ED5441',
                color: '#fff',
                message: '<strong>Xin lỗi!</strong><p style="font-size:12px">Vui lòng nhập mật khẩu</p>'
            },
            theme: 'colorful',
            position: 'top right',
            delay: 1000
        });
        return false;
    }
    $.ajax({
        url: '/Account/LoginValidate',
        type: 'POST',
        dataType: 'json',
        cache: false,
        async:   false,
        data: { username: x, password: y },
        success: function (data) {
            if (data.isValid) {
                check = true;
            }
            else
            {
                $.amaran({
                    content: {
                        bgcolor: '#ED5441',
                        color: '#fff',
                        message: '<strong>Xin lỗi!</strong><p style="font-size:12px">' + data.message + '</p>'
                    },
                    theme: 'colorful',
                    position: 'top right',
                    delay: 1000
                });
            }
        }
    });
    return check;
}
function doLogin() {
    $.amaran({
        content: {
            bgcolor: '#ED5441',
            color: '#fff',
            message: '<strong>Xin lỗi!</strong><p style="font-size:12px">Vui lòng đăng nhập vào hệ thống</p>'
        },
        theme: 'colorful',
        position: 'top right',
        delay: 1000
    });
    $('#loginLink').click();
    $('#UserName').focus();
}