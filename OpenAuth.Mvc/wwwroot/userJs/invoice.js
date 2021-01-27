layui.config({
    base: "/js/"
}).use(['form', 'vue', 'jquery', 'layer', 'table', 'treeTable', 'laydate', 'upload', 'element'], function () {
    var form = layui.form;
    var layer = layui.layer;
    var $ = layui.jquery;
    var table = layui.table;

    var collapseTable = function(options) {
        var trObj = options.elem;
        if (!trObj) return;
        var accordion = options.accordion,
            success = options.success,
            content = options.content || '';
        var tableView = trObj.parents('.layui-table-view'); //当前表格视图
        var id = tableView.attr('lay-id'); //当前表格标识
        var index = trObj.data('index'); //当前行索引
        var leftTr = tableView.find('.layui-table-fixed.layui-table-fixed-l tr[data-index="' + index + '"]'); //左侧当前固定行
        var rightTr = tableView.find('.layui-table-fixed.layui-table-fixed-r tr[data-index="' + index + '"]'); //右侧当前固定行
        var colspan = trObj.find('td').length; //获取合并长度
        var tds = trObj.find(".layui-table-col-special");
        var leftWidth = 0;
        for (var i = 0; i < tds.length; i++) {
            leftWidth += tds.eq(i).width();
        }
        var trObjChildren = trObj.next(); //展开行Dom
        var indexChildren = id + '-' + index + '-children'; //展开行索引
        var leftTrChildren = tableView.find('.layui-table-fixed.layui-table-fixed-l tr[data-index="' + indexChildren + '"]'); //左侧展开固定行
        var rightTrChildren = tableView.find('.layui-table-fixed.layui-table-fixed-r tr[data-index="' + indexChildren + '"]'); //右侧展开固定行
        var lw = leftTr.width() + leftWidth; //左宽
        var rw = rightTr.width(); //右宽
        
        
        //开启手风琴折叠和折叠箭头
        if (accordion) {
            //不存在就创建展开行
        if (trObjChildren.data('index') != indexChildren) {
            //装载HTML元素
            var tr = '<tr  data-index="' + indexChildren + '"><td style="padding:0;" colspan="' + colspan + '"><div style="height: auto;padding-left:' + lw + 'px;padding-right:' + rw + 'px" class="layui-table-cell">' + content + '</div></td></tr>';
            trObjChildren = trObj.after(tr).next().hide(); //隐藏展开行
            var fixTr = '<tr data-index="' + indexChildren + '"></tr>';//固定行
            leftTrChildren = leftTr.after(fixTr).next().hide(); //左固定
            rightTrChildren = rightTr.after(fixTr).next().hide(); //右固定
        }
            trObj.find("td i.layui-icon.layui-colla-icon.layui-icon-right").removeClass("layui-icon-right").addClass("layui-icon-down");
            //trObjChildren.siblings('[data-index$="-children"]').hide(); //展开
            //rightTrChildren.siblings('[data-index$="-children"]').hide(); //左固定
            //leftTrChildren.siblings('[data-index$="-children"]').hide(); //右固定
            //显示|隐藏展开行
            trObjChildren.show();
            success(trObjChildren, indexChildren); //回调函数
        } else {
            trObj.find("td i.layui-icon.layui-colla-icon.layui-icon-down").removeClass("layui-icon-down").addClass("layui-icon-right");
            trObj.next().eq(0).remove();
        }

        //heightChildren = trObjChildren.height(); //展开高度固定
        //rightTrChildren.height(heightChildren + 115).toggle(); //左固定
        //leftTrChildren.height(heightChildren + 115).toggle(); //右固定
    }

    var invoiceList = function () {
        $.getJSON('/Order/GetInvoiceList',
            {
                page: 1, limit: 1,
                keyword: $('#keyword').val(),
                status: $('#Status option:selected').val()
            }, function (data) {
                table.render({
                    elem: '#invoiceList',
                    data: data,
                    cols: [[
                        { type: 'checkbox' }
                        , { width: 50, align: 'center', toolbar: '#barDemo' }
                        , { field: 'ID', width: 70, title: 'ID', hide: true }
                        , {
                            field: 'SIDR_NO', width: 120, title: 'SI/DR No.', templet: function (row) {
                                return '<div style="position: relative;\n' + '    padding: 0 10px 0 20px;">' + row.SIDR_NO + '<i style="left: 0px;" lay-tips="展开" lay-event="collapse" class="layui-icon layui-colla-icon layui-icon-right"></i></div>'
                            }
                        }
                        , {
                            field: 'STATUS', width: 50, title: 'STATUS', templet: function (row) {
                                return row.STATUS == "1" ? "已付款" : "未付款";
                            }
                        }
                        , { field: 'INVOICE_DATE', width: 80, title: 'INVOICE DATE' }
                        , { field: 'CONTAINER_NO', width: 100, title: 'CONTAINER NO' }
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
                //监听工具条
                table.on('tool(list)',function (obj) {
                    var data = obj.data;
                    var layEvent = obj.event;
                    if (layEvent === 'edit') {
                        editDlg.update(data);
                    }
                    else if (layEvent === 'collapse') {
                        $.getJSON('/Order/GetSIDRList', { SIDR_NO: data.SIDR_NO }, function (data) {
                            var trObj = obj.tr; //当前行
                            var collapse = trObj.find("td i.layui-icon.layui-colla-icon.layui-icon-right").length > 0;
                            var accordion = collapse; //开启手风琴，那么在进行折叠操作时，始终只会展现当前展开的表格。
                            var content = '<table></table>' //内容
                            collapseTable({
                                elem: trObj,
                                accordion: accordion,
                                content: content,
                                success: function (trObjChildren, index) {
                                    trObjChildren.find('table').attr("id", index);
                                    table.render({
                                        elem: "#" + index,
                                        cellMinWidth: 50,
                                        data: data,
                                        cols: [[
                                            { field: 'ID', width: 70, title: 'ID', hide: true }
                                            , { field: 'PO_NO', width: 70, title: 'PO NO' }
                                            , { field: 'SIDR_NO', width: 90, title: 'SI/DR No.' }
                                            , { field: 'Qty', width: 50, title: 'Qty' }
                                            , { field: 'BILL_DATE', width: 80, title: 'BILL DATE' }
                                            , { field: 'REMARK', width: 120, title: 'REMARK' }
                                        ]]
                                    });
                                }
                            });
                        });
                           

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
                area: ["700px", "500px"],
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
                                        form.render();  
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

    form.on('select(status)', function (data) {
        $("#Status").get(0).value = data.value;
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