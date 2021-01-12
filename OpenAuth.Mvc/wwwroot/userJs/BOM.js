layui.config({
    base: "../../layui/lay/modules/"
}).use(['form', 'jquery', 'layer', 'table', 'treeTable', 'upload', 'element'], function () {
    var form = layui.form;
    $ = layui.jquery;
    var treetable = layui.treeTable;
    var upload = layui.upload; 
    var table = layui.table;
    var element = layui.element;
    //创建一个上传组件
    upload.render({
        elem: '#uploadBtn'
        , url: '/BOM/Upload'
        , method: 'POST'
        , data: { "BOM_NO": $("#BOM_NO").val() }
        , before: function () {
            top.loading(true);
            this.data = {
                BOM_NO: $("#BOM_NO").val()
            }
        },
        done: function (res, index, upload) { //上传后的回调
            top.loading(false);
            layer.msg(res.msg);
            fileQuery();
        }
        , accept: 'file' //允许上传的文件类型
        , size: 2147483648 //2G以内
        , error: function (res) {
        }
    })

    form.on('select(select_show)', function (data) {   //赋值给input框
        $("#select_input").val(data.value);
        $("#select_show").css({ "display": "none" });
        form.render();
    });

    window.search = function (value) {
        if (value!="") {
            $.getJSON('/BOM/GetList',
                { keyword: value, page: 1, limit: 1 },
                function (data) {
                    treetable.render({
                        elem: '#mainList',
                        data: data,  // 数据
                        tree: {
                            iconIndex: 0,    // 折叠图标显示在第几列
                            isPidData: true,
                            idName: "ID_NO",
                            pidName: "BOM_NO"
                        },
                        title: 'BOM资料',
                        cols: [[
                            { field: 'LEVEL1', width: 100, title: 'Class' }
                            , { field: 'ID_NO', width: 120, title: 'Component Node' }
                            , { field: 'BOM_NO', width: 150, title: 'Product BOM code' }
                            , { field: 'ChildPrdNo', width: 120, title: 'Component part No.' }
                            , { field: 'ChildName', width: 200, title: 'Component name' }
                            , { field: 'ChildSPC', width: 180, title: 'Component description' }
                            , { field: 'NAME_ENG', width: 150, title: 'Component English name' }
                            , { field: 'SNM', width: 90, title: 'Finish & Color' }
                            , {
                                field: 'QTY', width: 80, title: 'Usgage Qty', templet: function (row) {
                                    return parseFloat(row.QTY).toFixed(2);
                                }
                            }
                        ]]
                    });
                });
            fileQuery();
        }
    }

    window.fuzzyquery = function () {
        var value = $("#select_input").val();
        var select_show = $("#select_show");
        select_show.css("display", "block");
        if (value != "") {
            $.getJSON('/BOM/GetBOMList',
                { keyword: value, page: 1, limit: 1 },
                function (data) {
                    table.render({
                        elem: '#prdNoList',
                        data: data,  // 数据
                        cols: [[
                            { field: 'NAME_ENG', width: 200, title: 'Name' }
                            , { field: 'BOM_NO', width: 150, title: 'BOM code' }
                            , { field: 'PRD_NO', width: 145, title: 'Material part number' }
                        ]]
                    });
                    table.on('row(prdNo)', function (obj) {
                        var data = obj.data;
                        $("#select_show").css({ "display": "none" });
                        $("#BOM_NO").val(data.BOM_NO);
                        $("#keyValue").html(data.NAME_ENG);
                        search(data.PRD_NO);
                    });
                });
        }
    }

    window.fileQuery = function () {
       var  value=$("#BOM_NO").val();
        $.getJSON('/BOM/GetFileList',
            { keyword: value, page: 1, limit: 1 },
            function (data) {
                table.render({
                    id:'fileList',
                    elem: '#fileList',
                    data: data,  // 数据
                    cols: [[
                        { field: 'FileName', width: 300, title: 'File name' }
                        , { field: 'UploadTime', width: 150, title: 'Upload time' }
                        ,{
                            field: 'BOM_NO', width: 150, title: 'Download', templet: function (row) {
                                var data = row.BOM_NO.replace('>', '$');
                                return "<a class='data-file' BOM_NO='" + data + "' FileName='" + row.FileName + "'  >Download</href>";
                            }
                        }
                    ]]
                });
                $('.data-file').on('click', function () {
                    var BOM_NO = $(this).attr("BOM_NO");
                    var FileName = $(this).attr("FileName");
                    var param = { BOM_NO: BOM_NO, FileName: FileName };
                    var url = '/BOM/DownloadBigFile';
                    var form = $('<form method="POST" action="' + url + '">');
                    $.each(param, function (k, v) {
                        form.append($('<input type="hidden" name="' + k +
                            '" value="' + v + '">'));
                    });
                    $('body').append(form);
                    form.submit();
                });
            });
    }

    
})
