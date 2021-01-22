layui.config({
    base: "/js/"
}).use(['form', 'vue', 'jquery', 'layer', 'table', 'treeTable', 'laydate', 'upload', 'element'], function () {
    var form = layui.form;
    var layer = layui.layer;
    var $ = layui.jquery;
    var table = layui.table;

    var invoiceList = function () {
        $.getJSON('/Order/GetInvoiceList',
            {
                page: 1, limit: 1,
                keyword: $('#keyword').val()
            }, function (data) {
                table.render({
                    elem: '#invoiceList',
                    data: data,
                    cols: [[
                        { type: 'checkbox' }
                        , { width: 50, align: 'center', toolbar: '#barDemo' }
                        , { field: 'ID', width: 70, title: 'ID', hide: true }
                        , { field: 'SIDR_NO', width: 85, title: 'SI/DR No.' }
                        , { field: 'PO_NO', width: 70, title: 'PO NO' }
                        , { field: 'Customer', width: 60, title: 'Customer' }
                        , { field: 'Name', width: 100, title: 'Name' }
                        , { field: 'Qty', width: 50, title: 'Qty' }
                        , { field: 'BILL_DATE', width: 80, title: 'INVOICE DATE' }
                        , { field: 'CONTAINER_NO', width: 80, title: 'CONTAINER NO' }
                        , { field: 'Seal_NO', width: 80, title: 'Seal NO' }
                        , { field: 'BL_NO', width: 80, title: 'B/L NO' }
                        , { field: 'VESSEL', width: 80, title: 'VESSEL' }
                        , { field: 'ETD', width: 80, title: 'ETD' }
                        , { field: 'ETA', width: 80, title: 'ETA' }
                        , { field: 'TERMS_OF_SALE', width: 80, title: 'TERMS OF SALE' }
                        , { field: 'COUNTRY_OF_ORIGIN', width: 80, title: 'COUNTRY OF ORIGIN' }
                        , { field: 'SHIPMENT_FROM', width: 80, title: 'SHIPMENT FROM' }
                        , { field: 'TAX', width: 80, title: 'TAX' }
                        , { field: 'FREIGHT', width: 80, title: 'FREIGHT' }
                    ]]
                });
                table.on('tool(list)', function (obj) { //注：tool 是工具条事件名，test 是 table 原始容器的属性 lay-filter="对应的值"
                    var data = obj.data; 
                    var layEvent = obj.event; 
                    if (layEvent === 'edit') { 
                        editDlg.update(data);
                    }
                });
            });
    }

    invoiceList();

    var editDlg = function () {
        var vm;
        var show = function (data) {
            layer.open({
                title: "编辑信息",
                area: ["660px", "500px"],
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
                                form.render();
                            }
                        });
                    } else {
                        vm.tmp = Object.assign({}, vm.tmp, data)
                    }
                },
                end: invoiceList
            });
            var url = "/Order/EditInvoice";
            form.on('submit(formSubmit)',
                function (data) {
                    $.post(url,
                        data.field,
                        function (resp) {
                            layer.msg(JSON.parse(resp).msg);
                            $(".layui-layer-close").click();
                            invoiceList();
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
        invoiceList();
    });

    $("#keyword").keyup(function () {
        invoiceList();
    });

    var active = {
        btnExport: function () {
            var checkStatus = table.checkStatus('invoiceList')
                , data = checkStatus.data;
            if (data.length != 1) {
                layer.msg("请选择一行数据");
                return false;
            }
            var url = '/Order/ExportInvoice';
            var form = $('<form method="POST" action="' + url + '">');
            $.each(data[0], function (k, v) {
            form.append($('<input type="hidden" name="' + k +
                '" value="' + v + '">'));
            });
            form.append('</form>');
            top.$("body").append(form);
            form.submit();
        }
    };

})