layui.config({
    base: "/js/"
}).use(['form', 'vue', 'jquery', 'layer', 'table', 'treeTable', 'laydate', 'upload', 'element'], function () {
    var form = layui.form;
    var layer = layui.layer;
    var $ = layui.jquery;
    var table = layui.table;
    var laydate = layui.laydate;

    var start1 = laydate.render({
        elem: "#startShipDate",
        trigger: 'click',
        btns: ['clear', 'confirm'],
        done: function (value, date) {
            end1.config.min = {
                year: date.year,
                month: date.month - 1,
                date: date.date,
                hours: date.hours,
                minutes: date.minutes,
                seconds: date.seconds
            };
            end1.config.value = value;
        }
    });
    var end1 = laydate.render({
        elem: "#endShipDate",
        trigger: 'click',
        btns: ['clear', 'confirm'],
        done: function (value, date) {
            start1.config.max = {
                year: date.year,
                month: date.month - 1,
                date: date.date,
                hours: date.hours,
                minutes: date.minutes,
                seconds: date.seconds
            };
            start1.config.value = value;
        }
    });
    laydate.render(start1);
    laydate.render(end1);

    var productList = function () {
        $.getJSON('/Order/GetShipmentOrderList',
            {
                page: 1, limit: 1,
                keyword: $('#keyword').val(),
                startShipDate: $('#startShipDate').val(),
                endShipDate: $('#endShipDate').val(),
                Status: $('#Status option:selected').val()
            }, function (data) {
                table.render({
                    elem: '#productList',
                    data: data,
                    cols: [[
                          { width: 50, align: 'center', toolbar: '#barDemo' }
                        , { field: 'PO_NO', width: 80, title: 'PO NO' }
                        , { field: 'ITEM', width: 80, title: 'ITEM', hide: true }
                        , { field: 'Customer', width: 60, title: 'Customer' }
                        , { field: 'Buyer', width: 50, title: 'Buyer' }
                        , { field: 'PO_Date', width: 70, title: 'PO Date' }
                        , { field: 'Name', width: 100, title: 'Name' }
                        , { field: 'Description', width: 120, title: 'Description' }
                        , { field: 'Type', width: 60, title: 'Type' }
                        , { field: 'Project', width: 60, title: 'Project' }
                        , {
                            field: 'Qty', width: 50, title: 'Qty', templet: function (row) {
                                return parseFloat(row.Qty).toFixed(2);
                            }
                        }
                        , {
                            field: 'Product_Qty', width: 60, title: 'Product Qty', templet: function (row) {
                                if (row.Product_Qty != null) {
                                    return parseFloat(row.Product_Qty).toFixed(2);
                                }
                                return 0;
                            }
                        }
                        ,
                        {
                            field: 'Shipment_Qty', width: 60, title: '已出货数量', templet: function (row) {
                                return parseFloat(row.Shipment_Qty).toFixed(2);
                            }
                        },
                        {
                            field: 'UNShipment_Qty', width: 60, title: '未出货数量', templet: function (row) {
                                if (row.Product_Qty != null && row.Qty != null) {
                                    return (parseFloat(row.Qty) - parseFloat(row.Shipment_Qty)).toFixed(2);
                                }
                                return 0;
                            }
                        }
                        , {
                            field: 'Amount', width: 60, title: 'Amount', templet: function (row) {
                                return "$" + parseFloat(row.Shipment_Qty) * parseFloat(row.Price).toFixed(2);
                            }
                        }
                        , { field: 'Required_Shipping_Date', width: 70, title: 'R.S.Date' }
                        , { field: 'Delivery_Point', width: 100, title: 'Delivery Point' }
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
                table.on('rowDouble(list)', function (obj) { //注：tool 是工具条事件名，test 是 table 原始容器的属性 lay-filter="对应的值"
                    var data = obj.data; //获得当前行数据
                    detailDlg.show(data);
                });
            });
    }

    productList();

    var unsubmitList = function () {
        $.getJSON('/Order/GetUnSubmitShipmentList',
            {
                page: 1, limit: 1
            }, function (data) {
                table.render({
                    elem: '#unsubmitList',
                    data: data,
                    toolbar: '#toolbarDemo', //开启头部工具栏，并为其绑定左侧模板
                    cols: [[
                        { type: 'checkbox' }
                        , { field: 'ID', width: 70, title: 'ID', hide: true }
                        , { field: 'SIDR_NO', width: 85, title: 'SI/DR No.' }
                        , { field: 'PO_NO', width: 70, title: 'PO NO' }
                        , { field: 'Customer', width: 60, title: 'Customer' }
                        , { field: 'Name', width: 100, title: 'Name' }
                        , { field: 'Qty', width: 50, title: 'Qty' }
                        , { field: 'BILL_DATE', width: 80, title: 'BILL DATE' }
                        , { field: 'REMARK', width: 80, title: 'REMARK' }
                    ]]
                });
                //头工具栏事件
                table.on('toolbar(unsubmitList)', function (obj) {
                    var checkStatus = table.checkStatus(obj.config.id);
                    switch (obj.event) {
                        case 'submit':
                            var data = checkStatus.data;
                            if (data.length < 1) {
                                layer.msg("请选择提交的行！");
                                return;
                            }
                            var json = JSON.stringify(data);
                            $.ajax({
                                url: '/Order/SubmitShipment',
                                type: 'POST',
                                dataType: 'json',
                                data: { AllData: json },
                                success: function (resp) {
                                    layer.msg(JSON.parse(resp).msg);
                                    unsubmitList();
                                },
                                error: function (xhr) {
                                }
                            });
                            break;
                    };
                });
            });
    };

    unsubmitList();

    var shipmentList = function (options) {
        $.getJSON('/Order/GetShipmentList',
            {
                page: 1, limit: 1,
                PO_NO: options.PO_NO,
                ITEM: options.ITEM
            }, function (data) {
                table.render({
                    elem: '#shipList',
                    data: data,
                    cols: [[
                        { field: 'ID', width: 70, title: 'ID', hide: true }
                        , { field: 'PO_NO', width: 70, title: 'PO NO' }
                        , { field: 'SIDR_NO', width: 90, title: 'SI/DR No.' }
                        , { field: 'Qty', width: 50, title: 'Qty'}
                        , { field: 'BILL_DATE', width: 80, title: 'BILL DATE' }
                        , { field: 'REMARK', width: 120, title: 'REMARK' }
                    ]]
                });
            });
    };

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
                                data.Qty = parseFloat(data.Qty).toFixed(2);
                                data.UNShipment_Qty = parseFloat(data.Qty) - parseFloat(data.Shipment_Qty).toFixed(2);

                                form.verify({
                                    number: function (value) {
                                        if (isNaN(value)) {
                                            return "请填入数字";
                                        }
                                    },
                                    Qty: function (value) {
                                        if ((parseFloat($("#Qty").val()) - parseFloat($("#Shipment_Qty").val())) < parseFloat($("#UNShipment_Qty").val())) {
                                            return "超出最大出货数量";
                                        }
                                    }

                                });
                                form.render();
                            }
                        });
                    } else {
                        data.Qty = parseFloat(data.Qty).toFixed(2);
                        data.UNShipment_Qty = parseFloat(data.Qty) - parseFloat(data.Shipment_Qty).toFixed(2);
                        vm.tmp = Object.assign({}, vm.tmp, data)
                    }
                },
                end: productList
            });
            var url = "/Order/EditShipment";
            form.on('submit(formSubmit)',
            function (data) {
                    $.ajaxSettings.async = false; //关闭异步
                    $.post(url,
                        data.field,
                        function (resp) {
                            layer.msg(JSON.parse(resp).msg);
                            productList();
                            shipmentList({ PO_NO: data.field.PO_NO, ITEM: data.field.ITEM });
                            $(".layui-layer-close").click();
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

    var detailDlg = function () {
        var show = function (data) {
            layer.open({
                title: "出货明细",
                area: ["600px", "500px"],
                type: 1,
                content: $('#divShipDetail'),
                success: function () {
                    shipmentList({ PO_NO: data.PO_NO, ITEM: data.ITEM });
                },
                end: productList
            });
          
        }
        return {
            show : function (data) {
                show(data);
            }
        };
    }();

    table.on('row(list)', function (obj) {
        var data = obj.data;
        shipmentList({ PO_NO: data.PO_NO, ITEM: data.ITEM});
    });

    $('.toolList .button').click(function () {
        var type = $(this).data('type');
        active[type] ? active[type].call(this) : '';
    });

    $("#delivery").click(function () {
        unsubmitList();
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
       btnExport: function () {
            var param = {
                keyword: $('#keyword').val(),
                startShipDate: $('#startShipDate').val(),
                endShipDate: $('#endShipDate').val(),
                Status: $('#Status option:selected').val()
            };
            var url = '/Order/ExportShipmentList';
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