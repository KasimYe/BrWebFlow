$(function ($) {

    showDate();

    $("#search").click(function () {
        initTable();
    });
});

function showDate() {
    var datetime = new Date();
    var year = datetime.getFullYear();
    var month = datetime.getMonth() + 1 < 10 ? "0" + (datetime.getMonth() + 1) : datetime.getMonth() + 1;
    var date = datetime.getDate() < 10 ? "0" + datetime.getDate() : datetime.getDate();
    var hour = datetime.getHours() < 10 ? "0" + datetime.getHours() : datetime.getHours();
    var minute = datetime.getMinutes() < 10 ? "0" + datetime.getMinutes() : datetime.getMinutes();
    var second = datetime.getSeconds() < 10 ? "0" + datetime.getSeconds() : datetime.getSeconds();
    var week = datetime.getDay();
    var showD = year + "-" + month + "-" + date;

    $("#startDate").val(showD);
    $("#endDate").val(showD);
}

//ajax获取后台数据
function initTable() {

    var strhtml;
    var stratD = $("#startDate").val();
    var endD = $("#endDate").val();
    if (stratD == "" || endD == "") {
        alert("请选择起始截至日期");
        return;
    }
    var formN = $("#formNumber").val();
    var clientC = $("#clientCode").val();
    var formT = $("#formTypeID").val();

    $.ajax({
        type: 'POST',
        dataType: "json",
        async: false,
        url: "../service/Service.aspx",//请求的action路径页面
        data: { method: "GetSaleBill", pageIndex: "1", startDate: stratD, endDate: endD, formNumber: formN, clientCode: clientC, formType: formT },
        error: function () {//请求失败处理函数
            alert('请求失败');
        },
        success: function (data) { //请求成功后处理函数。
            strhtml = "";
            var myobj = eval(data);
            $.each(myobj.list, function (index, item) { //遍历返回的json
                strhtml += ('<tr>');
                strhtml += ('<td>' + (index + 1) + '</td>');
                strhtml += ('<td>' + item.SystemDate + '</td>');
                strhtml += ('<td><button type="button" class="btn btn-link" onclick=javascript:itemclick(' + item.FID + ')>' + item.FormNumber + '</button></td>');
                strhtml += ('<td>' + item.ClientName + '</td>');
                strhtml += ('<td>' + item.TaxSum + '</td>');
                strhtml += ('</tr>');
            });
            $("#list").html(strhtml);

            var pageCount = myobj.pageCount; //取到pageCount的值
            var currentPage = myobj.CurrentPage; //得到currentPage
            var pageSize = myobj.pageSize;//每页的条数

            var options = {
                bootstrapMajorVersion: 3, //版本
                currentPage: currentPage, //当前页数
                totalPages: pageCount, //总页数
                numberOfPages: 5,
                itemTexts: function (type, page, current) {
                    switch (type) {
                        case "first":
                            return "首页";
                        case "prev":
                            return "上一页";
                        case "next":
                            return "下一页";
                        case "last":
                            return "末页";
                        case "page":
                            return page;
                    }
                },//点击事件，用于通过Ajax来刷新整个list列表
                onPageClicked: function (event, originalEvent, type, page) {
                    $.ajax({
                        url: "../service/Service.aspx",
                        type: "Post",
                        dataType: "json",
                        data: { method: "GetSaleBill", pageIndex: page, startDate: stratD, endDate: endD, formNumber: formN, clientCode: clientC, formType: formT },
                        success: function (data) {
                            var myobj = eval(data);
                            strhtml = "";
                            $.each(myobj.list, function (index, item) { //遍历返回的json
                                strhtml += ('<tr>');
                                strhtml += ('<td>' + (pageSize * (page - 1) + index + 1) + '</td>');
                                strhtml += ('<td>' + item.SystemDate + '</td>');
                                strhtml += ('<td><button type="button" class="btn btn-link" onclick=javascript:itemclick(' + item.FID + ')>' + item.FormNumber + '</button></td>');
                                strhtml += ('<td>' + item.ClientName + '</td>');
                                strhtml += ('<td>' + item.TaxSum + '</td>');
                                strhtml += ('</tr>');
                            });
                            $("#list").html(strhtml);
                        }
                    });
                }
            };
            $('#example').bootstrapPaginator(options);
        }
    });
}

function itemclick(e) {
    var strhtml;
    $.ajax({
        type: 'POST',
        dataType: "json",
        async: false,
        url: "../service/Service.aspx",//请求的action路径页面
        data: { method: "GetSaleBillDetail", FID: e },
        error: function () {//请求失败处理函数
            alert('请求失败');
        },
        success: function (data) { //请求成功后处理函数。
            strhtml = "";
            var myobj = eval(data);
            $.each(myobj, function (index, item) { //遍历返回的json
                strhtml += ('<tr>');
                strhtml += ('<td>' + (index + 1) + '</td>');
                strhtml += ('<td>' + item.PName + '</td>');
                strhtml += ('<td>' + item.Model + '</td>');
                strhtml += ('<td>' + item.FromPlace + '</td>');
                strhtml += ('<td>' + item.BaseUnit + '</td>');
                strhtml += ('<td>' + item.Batch + '</td>');
                strhtml += ('<td>' + item.Quantity + '</td>');
                strhtml += ('<td>' + item.TaxPrice + '</td>');
                strhtml += ('<td>' + item.TaxTotal + '</td>');
                strhtml += ('<td><button type="button" class="btn btn-link" onclick=javascript:reverseclick(' + item.FDetailID + ')>冲票</button></td>');
                strhtml += ('</tr>');
            });
            $("#detail").html(strhtml);
        }
    });
}

function reverseclick(e) {
    if (!confirm("确定提交冲票该明细？")) {
        return;
    }
    $.ajax({
        type: 'POST',
        dataType: "json",
        async: false,
        url: "../service/Service.aspx",//请求的action路径页面
        data: { method: "ReverseSaleBillDetail", DetailID: e },
        error: function () {//请求失败处理函数
            alert('请求失败');
        },
        success: function (data) { //请求成功后处理函数。
            var myobj = eval(data);
            alert("冲票成功");
        }
    });
}