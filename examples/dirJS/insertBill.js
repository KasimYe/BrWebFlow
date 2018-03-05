$(function ($) {
    var clientID = 0;
    var pID = 0;

    var clientCode = $("#clientCode");
    clientCode.focusout(function () {
        var strClientCode = clientCode.val().toString();
        if (strClientCode.length >= 5) {
            $.ajax({
                type: 'POST',
                dataType: "json",
                async: false,
                url: "../service/Service.aspx",//请求的action路径页面
                data: { method: "GetClientByClientCode", clientCode: strClientCode },
                error: function () {//请求失败处理函数
                    alert('请求失败');
                },
                success: function (data) { //请求成功后处理函数。
                    var myobj = eval(data);
                    clientCode.val(myobj[0].ClientName);
                    clientID = myobj[0].ClientID;
                    //clientCode.attr("disabled", "disabled");
                }
            });
        } else {
            clientID = 0;
        }
    });

    var pCode = $("#pCode");
    pCode.focusout(function () {
        var strpCode = pCode.val().toString();
        if (strpCode.length >= 5) {
            $.ajax({
                type: 'POST',
                dataType: "json",
                async: false,
                url: "../service/Service.aspx",//请求的action路径页面
                data: { method: "GetProductByPCode", pCode: strpCode },
                error: function () {//请求失败处理函数
                    alert('请求失败');
                },
                success: function (data) { //请求成功后处理函数。
                    var myobj = eval(data);
                    pCode.val(myobj[0].PName);
                    $("#pInfo").html("商品编码：" + myobj[0].PCode5 + "          规格：" + myobj[0].Model + "          生产厂商：" + myobj[0].FromPlace + "")
                    pID = myobj[0].PID;
                    //pCode.attr("disabled", "disabled");
                }
            });
        } else {
            pID = 0;
        }
    });

    $("#submit").click(function () {
        if (!confirm("确定提交单据信息？")) {
            return;
        }
        if (clientID == 0 || pID == 0) {
            alert("请输入客户和商品");
            return;
        }
        var strBatch = $("#batch").val();
        if (strBatch == "") {
            alert("请输入批号");
            return;
        }
        var strQuantity = $("#quantity").val();
        if (strQuantity == "") {
            alert("请输入数量");
            return;
        }
        var strTaxPrice = $("#taxprice").val();
        if (strTaxPrice == "") {
            alert("请输入单价");
            return;
        }
        var strTaxTotal = strTaxPrice * strQuantity;
        $("#taxtotal").val(strTaxTotal);

        $.ajax({
            type: 'POST',
            dataType: "json",
            async: false,
            url: "../service/Service.aspx",//请求的action路径页面
            data: { method: "InsertSaleBillDetail", ClientID: clientID, PID: pID, Batch: strBatch, Quantity: strQuantity, TaxPrice: strTaxPrice },
            error: function () {//请求失败处理函数
                alert('请求失败');
            },
            success: function (data) { //请求成功后处理函数。
                var myobj = eval(data);
                alert("新增成功");
            }
        });
    });
});