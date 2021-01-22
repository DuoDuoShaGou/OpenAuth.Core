layui.config({
    base: "/js/"
}).use(['form', 'vue', 'jquery', 'layer', 'table', 'treeTable', 'laydate', 'upload', 'element'], function () {
    var form = layui.form;
    var layer = layui.layer;
    var $ = layui.jquery;
    var table = layui.table;
    var laydate = layui.laydate;

    var productList = function () {
        $.getJSON('/Order/GetProuctOrderList',
            {
                page: 1, limit: 1,
                keyword: $('#keyword').val(),
                Status: $('#Status option:selected').val()
            }, function (data) {
                table.render({
                    elem: '#productList',
                    data: data,
                    cols: [[
                        { type: 'checkbox' }
                        ,{  width: 60, align: 'center', toolbar: '#barDemo' }
                        , { field: 'PO_NO', width: 80, title: 'PO NO' }
                        , { field: 'ITEM', width: 80, title: 'ITEM', hide: true }
                        , { field: 'Customer', width: 100, title: 'Customer' }
                        , { field: 'Buyer', width: 80, title: 'Buyer' }
                        , { field: 'PO_Date', width: 80, title: 'PO Date'}
                        , { field: 'Name', width: 120, title: 'Name' }
                        , { field: 'Description', width: 120, title: 'Description' }
                        , { field: 'Type', width: 60, title: 'Type' }
                        , { field: 'Project', width: 60, title: 'Project' }
                        , {
                            field: 'Qty', width: 80, title: 'Qty', templet: function (row) {
                                return parseFloat(row.Qty).toFixed(2);
                            }
                        }
                        , {
                            field: 'Product_Qty', width: 80, title: 'Product Qty', templet: function (row) {
                                if (row.Product_Qty != null) {
                                    return parseFloat(row.Product_Qty).toFixed(2);
                                }
                                return 0;
                            }
                        }
                        , {
                            field: 'UNProduct_Qty', width: 80, title: '未生产数量', templet: function (row) {
                                if (row.Product_Qty != null && row.Qty != null) {
                                    return (parseFloat(row.Qty) - parseFloat(row.Product_Qty)).toFixed(2);
                                }
                                return 0;
                            }
                        }
                        , {
                            field: 'Amount', width: 80, title: 'Amount', templet: function (row) {
                                return "$" + parseFloat(row.Qty) * parseFloat(row.Price).toFixed(2);
                            }
                        }
                        , { field: 'Required_Shipping_Date', width: 120, title: 'Required Shipping Date' }
                        , { field: 'Delivery_Point', width: 120, title: 'Delivery Point' }
                        , {
                            field: 'Status', width: 60, title: 'Status', templet: function (row) {
                                switch (parseInt(row.Status)) {
                                    case 0:
                                        return "已录入";
                                    case 1:
                                        return "生产中";
                                    case 2:
                                        return "已完成";
                                    case 3:
                                        return "已出货";
                                    case 4:
                                        return "已收款";
                                    default:
                                        return "状态有误";
                                }
                            }
                        }
                    ]]
                });
                table.on('tool(list)', function (obj) { //注：tool 是工具条事件名，test 是 table 原始容器的属性 lay-filter="对应的值"
                    var data = obj.data; //获得当前行数据
                    var layEvent = obj.event; //获得 lay-event 对应的值（也可以是表头的 event 参数对应的值）
                    if (layEvent === 'edit') { //编辑
                        editDlg.update(data);
                    } 
                });
            });
    }

    productList();

    var editDlg = function () {
        var vm;
        var show = function (data) {
            layer.open({
                title: "编辑信息",
                area: ["660px", "430px"],
                type: 1,
                content: $('#divOrderEdit'),
                success: function () {
                    if (vm == undefined) {
                        vm = new Vue({
                            el: "#mOrderEdit",
                            data: function () {
                                return {
                                    tmp: data 
                                }
                            },
                            watch: {
                                tmp(val) {
                                    this.$nextTick(function () {
                                        form.render();  //刷新select等
                                    })
                                }
                            },
                            mounted() {
                                if (data.Qty != null && data.Qty != "") {
                                    data.Qty = parseFloat(data.Qty).toFixed(2);
                                }
                                if (data.Product_Qty != null && data.Product_Qty != "") {
                                    data.Product_Qty = (parseFloat(data.Qty) - parseFloat(data.Product_Qty) ).toFixed(2);
                                }
                                form.verify({
                                    Qty: function (value, item) {
                                        if (isNaN(value)) {
                                            return "请填入数字";
                                        }
                                        if (parseFloat($("#Qty").val()) < parseFloat(value))
                                            return "生产数量不能超过订单数量！";
                                    }
                                });
                                form.render();
                            }
                        });
                    } else {
                        vm.tmp = Object.assign({}, vm.tmp, data)
                    }
                },
                end: productList
            });
            var url = "/Order/UpdateProduct";
            form.on('submit(formSubmit)',
            function (data) {
                $.post(url,
                    data.field,
                    function (resp) {
                        layer.msg(JSON.parse(resp).msg);
                        $(".layui-layer-close").click();
                        productList();
                    }, "json");
                return false;
            });
        }
        return {
            update: function (data) { 
                update = true;
                show(data);
            }
        };
    }();

    $('.toolList .button').click(function () {
        var type = $(this).data('type');
        active[type] ? active[type].call(this) : '';
    });

    $("#searchBtn").click(function () {
        productList();
    });

    $("#keyword").keyup(function () {
        productList();
    });

    form.on('select(status)', function (data) {
        $("#Status").get(0).value = data.value; 
        productList();
    });

    var active = {
        btnConfirm: function () {
            var checkStatus = table.checkStatus('productList')
                , data = checkStatus.data;
            var json = JSON.stringify(data);
            if (data.length < 1) {
                layer.msg("请选择确认的订单");
                return;
            }
            for (var i = 0; i < data.length; i++) {
                if (data[i].Status!="0") {
                    layer.msg("当前状态不可确认！");
                    return;
                }
            }
            $.ajax({
                url: '/Order/ConfirmOrder',
                type: 'POST',
                dataType: 'json',
                data: { AllData: json },
                success: function (resp) {
                    layer.msg(JSON.parse(resp).msg);
                    productList();
                },
                error: function (xhr) {
                }
            });
        }
        ,btnExport: function () {
            var param = {
                keyword: $('#keyword').val(),
                Status: $('#Status option:selected').val()
            };
            var url = '/Order/ExportProductList';
            var form = $('<form method="POST" action="' + url + '">');
            $.each(param, function (k, v) {
                form.append($('<input type="hidden" name="' + k +
                    '" value="' + v + '">'));
            });
            form.append('</form>');
            top.$("body").append(form);
            form.submit();
        }
    };

})