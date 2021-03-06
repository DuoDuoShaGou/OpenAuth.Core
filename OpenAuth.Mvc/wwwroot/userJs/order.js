﻿layui.config({
    base: "/js/"
}).use(['form','vue', 'jquery', 'layer', 'table', 'treeTable', 'laydate', 'upload', 'element'], function () {
    var form = layui.form;
    var layer = layui.layer;
    var $ = layui.jquery;
    var upload = layui.upload;
    var table = layui.table;
    var laydate = layui.laydate;
    var start = laydate.render({
        elem: "#startPODate",
        trigger: 'click',
        btns: ['clear', 'confirm'],
        done: function (value, date) {
            end.config.min = {
                year: date.year,
                month: date.month - 1,
                date: date.date,
                hours: date.hours,
                minutes: date.minutes,
                seconds: date.seconds
            };
            end.config.value = value;
        }
    });
    var end = laydate.render({
        elem: "#endPODate",
        trigger: 'click',
        btns: ['clear', 'confirm'],
        done: function (value, date) {
            start.config.max = {
                year: date.year,
                month: date.month - 1,
                date: date.date,
                hours: date.hours,
                minutes: date.minutes,
                seconds: date.seconds
            };
            start.config.value = value;
        }
    });
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
    laydate.render(start);
    laydate.render(end);
    laydate.render(start1);
    laydate.render(end1);
    upload.render({
        elem: '#btnImport'
        , url: '/Order/ImportOrderList'
        , method: 'POST'
        , before: function () {
            top.loading(true);
        },
        done: function (res, index, upload) { //上传后的回调
            top.loading(false);
            layer.msg(res.msg);
            orderList();
        }
        , accept: 'file' //允许上传的文件类型
        , size: 2147483
        , error: function (res) {
        }
    })

    var orderList = function () {
        $.getJSON('/Order/GetOrderList',
            {
                page: 1, limit: 1,
                keyword: $('#keyword').val(),
                startPODate: $('#startPODate').val(),
                endPODate: $('#endPODate').val(),
                startShipDate: $('#startShipDate').val(),
                endShipDate: $('#endShipDate').val()
            }, function (data) {
                table.render({
                    elem: '#orderList',
                    data: data,
                    cols: [[
                        { type: 'checkbox' }
                        , { field: 'PO_NO', width: 80, title: 'PO NO' }
                        , { field: 'ITEM', width: 80, title: 'ITEM', hide: true }
                        , { field: 'Customer', width: 60, title: 'Customer' }
                        , { field: 'Buyer', width: 100, title: 'Buyer' }
                        , {
                            field: 'PO_Date', width: 70, title: 'PO Date'
                        }
                        , { field: 'Name', width: 130, title: 'Name' }
                        , { field: 'Description', width: 130, title: 'Description' }
                        , { field: 'Type', width: 60, title: 'Type' }
                        , { field: 'Project', width: 60, title: 'Project' }
                        , {
                            field: 'Qty', width: 50, title: 'Qty', templet: function (row) {
                                return parseFloat(row.Qty).toFixed(2);
                            }
                        }
                        , {
                            field: 'Price', width: 50, title: 'Price', templet: function (row) {
                                return "$" + parseFloat(row.Price).toFixed(2);
                            }
                        }
                        , {
                            field: 'Amount', width: 60, title: 'Amount', templet: function (row) {
                                return "$" + parseFloat(row.Qty) * parseFloat(row.Price).toFixed(2);
                            }
                        }
                        , {
                            field: 'Required_Shipping_Date', width: 70, title: 'Required Shipping Date'
                        }
                        , { field: 'Delivery_Point', width: 160, title: 'Delivery Point' }
                        , {
                            field: 'Status', width: 50, title: 'Status', templet: function (row) {
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
            });
    }

    orderList();

    var editDlg = function () {
        var vm;
        var update = false;  //是否为更新
        var show = function (data) {
            var title = update ? "编辑信息" : "添加";
            layer.open({
                title: title,
                area: ["660px", "580px"],
                type: 1,
                content: $('#divOrderEdit'),
                success: function () {
                    if (vm == undefined) {
                        vm = new Vue({
                            el: "#mOrderEdit",
                            data: function () {
                                return {
                                    tmp: data  //使用一个tmp封装一下，后面可以直接用vm.tmp赋值
                                }
                            },
                            mounted() {
                                if (data.Qty != null && data.Qty!="") {
                                    data.Qty = parseFloat(data.Qty).toFixed(2);
                                    if (data.Price != null) {
                                        data.Price = parseFloat(data.Price).toFixed(2);
                                    }
                                }
                                laydate.render({
                                    elem: "#PO_Date",
                                    trigger: "click",
                                    done: value => { 
                                        this.tmp.PO_Date = value;
                                    }
                                });

                                laydate.render({
                                    elem: "#Required_Shipping_Date",
                                    trigger: "click",
                                    done: value => {
                                        this.tmp.Required_Shipping_Date = value;
                                    }
                                });
                                form.verify({
                                    number: function (value) {
                                        if (isNaN(value)) {
                                            return "请填入数字";
                                        }
                                    }
                                });
                                form.render();
                            }
                        });
                    } else {
                        vm.tmp = Object.assign({}, vm.tmp, data)
                    }
                },
                end: orderList
            });
            var url = "/Order/AddOrder";
            if (update) {
                url = "/Order/UpdateOrder";
            }
            //提交数据
            form.on('submit(formSubmit)',
                function (data) {
                    $.post(url,
                        data.field,
                        function (resp) {
                            layer.msg(JSON.parse(resp).msg);
                            orderList();
                            $(".layui-layer-close").click();
                        },"json");
                    return false;
                });
        }
        return {
            add: function () { //弹出添加
                update = false;
                show({
                    PO_NO : ''
                });
            },
            update: function (data) { //弹出编辑框
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
        orderList();
    });

    $("#keyword").keyup(function () {
        orderList();
    });

    var active = {
        btnAdd: function () {  //添加模块
            editDlg.add();
        },
        btnEdit: function () {  //编辑
            var checkStatus = table.checkStatus('orderList')
                , data = checkStatus.data;
            if (data.length != 1) {
                layer.msg("请选择编辑的行，且同时只能编辑一行");
                return;
            }
            editDlg.update(data[0]);
        }
        , btnDel: function () {
            var checkStatus = table.checkStatus('orderList'), data = checkStatus.data;
            if (data.length < 1) {
                layer.msg("请选择删除的行！");
                return;
            }
            var json = JSON.stringify(data);
            $.ajax({
                url: '/Order/DeleteOrder',
                type: 'POST',
                dataType: 'json',
                data: { AllData: json },
                success: function (resp) {
                    layer.msg(JSON.parse(resp).msg);
                    orderList();
                },
                error: function (xhr) {
                }
            });
        }
        , btnExport: function () {
            var param = {
                keyword: $('#keyword').val(),
                startPODate: $('#startPODate').val(),
                endPODate: $('#endPODate').val(),
                startShipDate: $('#startShipDate').val(),
                endShipDate: $('#endShipDate').val()
            };
            var url = '/Order/ExportOrderList';
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