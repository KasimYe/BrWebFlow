$(function ($) {
    $("#loginin").click(function () {
        var loginID = $("#loginID").val();
        var password = $("#password").val();
        $.ajax({
            type: 'POST',
            dataType: "json",
            async: false,
            url: "../service/Service.aspx",//请求的action路径页面
            data: { method: "Login", LoginID: loginID, UserPass: password },
            error: function (XMLHttpRequest, textStatus, errorThrown) {//请求失败处理函数
                alert(XMLHttpRequest.readyState);
                alert(XMLHttpRequest.status);
                alert(textStatus);
                alert(errorThrown);
            },
            success: function (data) { //请求成功后处理函数。
                var myobj = eval(data);
                if (myobj.loginMsg == "ok") {
                    if (myobj.list[0].user_Status) {
                        window.location = "examples/main.html";
                    } else {
                        alert("账号已停用")
                    }
                } else {
                    alert(myobj.loginMsg)
                }

            }
        });
    });
});



