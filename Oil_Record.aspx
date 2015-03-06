<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Record.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Record" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="robots" content="noindex, nofollow" />
    <title></title>
    <script src="Scripts/jquery-easyui-1.4.1/jquery.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-easyui-1.4.1/jquery.easyui.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-easyui-1.4.1/easyloader.js" type="text/javascript"></script>
    <script src="Scripts/global.js" type="text/javascript"></script>
    <script src="Scripts/My97DatePicker/WdatePicker.js" type="text/javascript"></script>
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/default/easyui.css" />
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/icon.css" />
    <link rel="Stylesheet" type="text/css" href="Styles/global.css" />
    <style type="text/css">
        *
        {
            margin: 0;
            padding: 0;
            font-family: "宋体" , Arial, "新宋体";
            font-size: 12px;
            list-style: none;
        }
    </style>
    <script type="text/javascript">

        var preOilFill = null;
        var editcount = 0;

        var bal_months_IDs = new Array();
        bal_months_IDs[0] = "bal_jan";
        bal_months_IDs[1] = "bal_feb";
        bal_months_IDs[2] = "bal_mar";
        bal_months_IDs[3] = "bal_apr";
        bal_months_IDs[4] = "bal_may";
        bal_months_IDs[5] = "bal_jun";
        bal_months_IDs[6] = "bal_jul";
        bal_months_IDs[7] = "bal_aug";
        bal_months_IDs[8] = "bal_sep";
        bal_months_IDs[9] = "bal_oct";
        bal_months_IDs[10] = "bal_nov";
        bal_months_IDs[11] = "bal_dec";

        var fill_months_IDs = new Array();
        fill_months_IDs[0] = "fill_jan";
        fill_months_IDs[1] = "fill_feb";
        fill_months_IDs[2] = "fill_mar";
        fill_months_IDs[3] = "fill_apr";
        fill_months_IDs[4] = "fill_may";
        fill_months_IDs[5] = "fill_jun";
        fill_months_IDs[6] = "fill_jul";
        fill_months_IDs[7] = "fill_aug";
        fill_months_IDs[8] = "fill_sep";
        fill_months_IDs[9] = "fill_oct";
        fill_months_IDs[10] = "fill_nov";
        fill_months_IDs[11] = "fill_dec";

        var lastYearSaveRecord = 0;
        var thisYearFillRecords = new Array([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        var thisYearConsumeRecords = new Array([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        function EnsureMMSI() {
            if (IsValidValue(parent.cur_mmsi) == false) {
                $.messager.show({
                    title: '请选择船只',
                    msg: '请先选择需要查看加油记录的船只',
                    showType: 'show'
                });
                return false;
            }

            return true;
        }

        $(function () {


        });

        function ResetOilFillData() {
            for (var i = 0; i < 12; i++) {
                thisYearFillRecords[i] = 0;
                thisYearConsumeRecords[i] = 0;
            }
        }

        function SetOilFillRecords() {
            RefreshOilMonthFillRecords();
            RefreshOilFillRecords(false);
        }

        function UpdateYearOilFillRecord() {

            var cur_month = new Date().getMonth();

            if (cur_month == 0) {
                $("#lastMonthSave").html(EnsureNum(lastYearSaveRecord).toFixed(3));
            }

            for (var i = 0; i < 12; i++) {

                var bal_month = EnsureNum(lastYearSaveRecord);
                var fill_month = EnsurePositive(thisYearFillRecords[i]);

                for (var k = 0; k <= i; k++) {
                    bal_month += EnsurePositive(thisYearFillRecords[k]);
                    bal_month -= EnsurePositive(thisYearConsumeRecords[k]);
                }

                if (cur_month != 0 && i == (cur_month - 1)) {
                    $("#lastMonthSave").html(parseFloat(bal_month).toFixed(3));
                }

                if (cur_month == i) {
                    $("#thisMonthFill").html(EnsurePositive(thisYearFillRecords[i]).toFixed(3));
                    $("#thisMonthConsume").html(EnsurePositive(thisYearConsumeRecords[i]).toFixed(3));
                    $("#thisMonthSave").html(parseFloat(bal_month).toFixed(3));
                }

                $("#" + bal_months_IDs[i]).val(parseFloat(bal_month).toFixed(3));
                $("#" + fill_months_IDs[i]).html(parseFloat(fill_month).toFixed(3));
            }
        }


        function UpdateOilFillRecords() {
            $("#oil_fill_records_dlg").show();

            $("#oil_fill_records_dlg").dialog({
                width: 580,
                height: 300,
                modal: false,
                title: '加油记录',
                minimizable: false,
                maximizable: false
            });

            $('#oil_fill_records_table').datagrid({
                title: '编辑加油记录',
                iconCls: 'icon-edit',
                width: 530,
                height: 250,
                singleSelect: true,
                columns: [[
            { field: 'fillID', title: '加油ID', width: 50 },
            { field: 'fillDate', title: '加油时间', width: 100, editor: 'datebox' },
            { field: 'fillAmount', title: '加油油量', width: 100, editor: 'numberbox' },
            { field: 'action', title: '操作', width: 100, align: 'center',
                formatter: function (value, row, index) {
                    if (row.editing) {
                        var s = '<a href="#" onclick="saverow(' + index + ')">保存</a> ';
                        var c = '<a href="#" onclick="cancelrow(' + index + ')">取消</a>';
                        return s + c;
                    } else {
                        var e = '<a href="#" onclick="editrow(' + index + ')">编辑</a> ';
                        var d = '<a href="#" onclick="deleterow(' + index + ')">删除</a>';
                        return e + d;
                    }
                }
            }]],
                toolbar: [{
                    text: '增加',
                    iconCls: 'icon-add',
                    handler: addrow
                }],
                onBeforeEdit: function (index, row) {
                    row.editing = true;
                    $('#oil_fill_records_table').datagrid('refreshRow', index);
                    editcount++;
                },
                onAfterEdit: function (index, row) {
                    row.editing = false;
                    $('#oil_fill_records_table').datagrid('refreshRow', index);
                    editcount--;
                },
                onCancelEdit: function (index, row) {
                    row.editing = false;
                    $('#oil_fill_records_table').datagrid('refreshRow', index);
                    editcount--;
                }
            });

            RefreshOilFillRecords(true);
        }


        function editrow(index) {
            if (editcount > 0) {
                $.messager.alert('警告', '当前还有' + editcount + '记录正在编辑，不能编辑记录。');
                return;
            }

            $('#oil_fill_records_table').datagrid('beginEdit', index);
        }

        function deleterow(index) {
            if (editcount > 0) {
                $.messager.alert('警告', '当前还有' + editcount + '记录正在编辑，不能删除记录。');
                return;
            }

            $.messager.confirm('确认', '是否真的删除?', function (r) {
                if (r) {
                    $('#oil_fill_records_table').datagrid('deleteRow', index);
                    ApplyOilFillRecordsChanges();
                }
            });
        }
        function saverow(index) {

            // TODO: verify data here.

            $('#oil_fill_records_table').datagrid('endEdit', index);
            ApplyOilFillRecordsChanges();
        }

        function cancelrow(index) {
            $('#oil_fill_records_table').datagrid('cancelEdit', index);
        }

        function addrow() {
            if (editcount > 0) {
                $.messager.alert('警告', '当前还有' + editcount + '记录正在编辑，不能增加记录。');
                return;
            }
            $('#oil_fill_records_table').datagrid('appendRow', {
                fillID: '',
                fillDate: '',
                fillAmount: ''
            });
        }

        function RefreshOilMonthFillRecords() {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&fill_month=" + GetCurDateString(),
                url: "shipoil_ajax.aspx?oper=getOilSaveYearRecord",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (d) {
                    var jsondata = eval(d); // convert json data
                    if (jsondata.hasOwnProperty("OilYearSaveAmount")) {
                        lastYearSaveRecord = EnsureNum(jsondata.OilYearSaveAmount);
                    }
                }
            });

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&fill_month=" + GetCurDateString(),
                url: "shipoil_ajax.aspx?oper=getOilFillYearRecords",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (d) {

                    var jsondata = eval(d); // convert json data
                    if (jsondata.length <= 0) return;

                    ResetOilFillData();

                    for (var i = 0; i < jsondata.length; i++) {
                        var index = (ParseDateString(jsondata[i].OilMonthFillDate)).getMonth();
                        thisYearFillRecords[index] = EnsurePositive(jsondata[i].OilMonthFillAmount);
                        thisYearConsumeRecords[index] = EnsurePositive(jsondata[i].OilMonthConsumeAmount);
                    }

                    UpdateYearOilFillRecord();
                }
            });
        }

        function RefreshOilFillRecords(bUpdateGrid) {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&fill_month=" + GetCurDateString(),
                url: "shipoil_ajax.aspx?oper=getOilFillRecords",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (d) {

                    var jsondata = eval(d); // convert json data
                    if (jsondata.length <= 0) return;

                    ClearOilFillRecordsTable();

                    var oilFill = "<tr>";
                    for (var i = 0; i < jsondata.length; i++) {
                        oilFill += "<th scope='col'>";
                        oilFill += GetDateWithMonthAndDayLocalString(ParseDateString(jsondata[i].OilFillDate));
                        oilFill += "</th>";
                    }
                    oilFill += "</tr>";

                    oilFill += "<tr>";
                    for (var i = 0; i < jsondata.length; i++) {
                        oilFill += "<td><input class='txtA' type='text' maxlength='20' value=" +
                    jsondata[i].OilFillAmount.toString() + " /></td>";
                    }
                    oilFill += "</tr>";

                    preOilFill = $(oilFill).appendTo($("#oilFillRecordsTable"));

                    if (bUpdateGrid) {
                        var jsonDataShow = new Object();
                        jsonDataShow.total = jsondata.length;

                        var dataArr = [];

                        for (var i = 0; i < jsondata.length; i++) {
                            var record = jsondata[i];

                            if (record == null) {
                                continue;
                            }

                            var data = new Object();
                            data.fillID = record.OilFillID;
                            data.fillDate = GetDateString(ParseDateString(record.OilFillDate));
                            data.fillAmount = record.OilFillAmount;

                            dataArr.push(data);
                        }

                        jsonDataShow.rows = dataArr;

                        $('#oil_fill_records_table').datagrid('loadData', jsonDataShow);
                        $('#oil_fill_records_table').datagrid('acceptChanges');
                        editcount = 0;
                    }
                }
            });
        }

        function ClearOilFillRecordsTable() {
            if (preOilFill != null) {
                preOilFill.remove();
                preOilFill = null;
            }
        }

        function ApplyOilFillRecordsChanges() {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            if ($('#oil_fill_records_table').datagrid('getChanges').length) {
                var inserted = $('#oil_fill_records_table').datagrid('getChanges', "inserted");
                var deleted = $('#oil_fill_records_table').datagrid('getChanges', "deleted");
                var updated = $('#oil_fill_records_table').datagrid('getChanges', "updated");

                var effectRow = null;

                if (inserted.length) {
                    var rowDetail = "&inserted=" + JSON.stringify(inserted);
                    if (effectRow == null)
                        effectRow = rowDetail;
                    else
                        effectRow += rowDetail;
                }

                if (deleted.length) {
                    var rowDetail = "&deleted=" + JSON.stringify(deleted);
                    if (effectRow == null)
                        effectRow = rowDetail;
                    else
                        effectRow += rowDetail;
                }

                if (updated.length) {
                    var rowDetail = "&updated=" + JSON.stringify(updated);
                    if (effectRow == null)
                        effectRow = rowDetail;
                    else
                        effectRow += rowDetail;
                }

                if (effectRow == null) return;

                $.ajax({
                    type: "get",
                    dataType: "json",
                    data: "mmsi=" + mmsi + "&fill_month=" + GetCurDateString() + effectRow,
                    url: "shipoil_ajax.aspx?oper=updateOilFillRecords",
                    error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                    success: function (d) {
                        RefreshOilMonthFillRecords();
                        RefreshOilFillRecords(true);
                    }
                });
            }
        }

    </script>
</head>

<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO2">
            <div class="ch_content2" style="margin-top: 0; margin-bottom: 20px;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    每月结存记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                1月（公斤）
                            </th>
                            <th scope="col">
                                2月（公斤）
                            </th>
                            <th scope="col">
                                3月（公斤）
                            </th>
                            <th scope="col">
                                4月（公斤）
                            </th>
                            <th scope="col">
                                5月（公斤）
                            </th>
                            <th scope="col">
                                6月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <input id="bal_jan" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_feb" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_mar" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_apr" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_may" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_jun" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                        </tr>
                    </table>
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                7月（公斤）
                            </th>
                            <th scope="col">
                                8月（公斤）
                            </th>
                            <th scope="col">
                                9月（公斤）
                            </th>
                            <th scope="col">
                                10月（公斤）
                            </th>
                            <th scope="col">
                                11月（公斤）
                            </th>
                            <th scope="col">
                                12月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <input id="bal_jul" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_aug" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_sep" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_oct" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_nov" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_dec" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    本月加油记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1" id="oilFillRecordsTable">
                    </table>
                    <div style="width: 100%; float: left;">
                        <div class="ch_but0">
                            <input class="ch_but1" type="button" onclick="UpdateOilFillRecords();" value="编 辑" />
                        </div>
                    </div>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    每月加油记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                1月（公斤）
                            </th>
                            <th scope="col">
                                2月（公斤）
                            </th>
                            <th scope="col">
                                3月（公斤）
                            </th>
                            <th scope="col">
                                4月（公斤）
                            </th>
                            <th scope="col">
                                5月（公斤）
                            </th>
                            <th scope="col">
                                6月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="fill_jan">
                                空
                            </td>
                            <td id="fill_feb">
                                空
                            </td>
                            <td id="fill_mar">
                                空
                            </td>
                            <td id="fill_apr">
                                空
                            </td>
                            <td id="fill_may">
                                空
                            </td>
                            <td id="fill_jun">
                                空
                            </td>
                        </tr>
                    </table>
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                7月（公斤）
                            </th>
                            <th scope="col">
                                8月（公斤）
                            </th>
                            <th scope="col">
                                9月（公斤）
                            </th>
                            <th scope="col">
                                10月（公斤）
                            </th>
                            <th scope="col">
                                11月（公斤）
                            </th>
                            <th scope="col">
                                12月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="fill_jul">
                                空
                            </td>
                            <td id="fill_aug">
                                空
                            </td>
                            <td id="fill_sep">
                                空
                            </td>
                            <td id="fill_oct">
                                空
                            </td>
                            <td id="fill_nov">
                                空
                            </td>
                            <td id="fill_dec">
                                空
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    本月结存（每月31日24时更新数据）</div>
                <div style="width: 100%; float: left; margin-bottom: 20px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                上月结存（公斤）
                            </th>
                            <th scope="col">
                                本月加油（公斤）
                            </th>
                            <th scope="col">
                                本月油耗（公斤）
                            </th>
                            <th scope="col">
                                本月结存（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="lastMonthSave">
                                空
                            </td>
                            <td id="thisMonthFill">
                                空
                            </td>
                            <td id="thisMonthConsume">
                                空
                            </td>
                            <td style="color: #0F0;" id="thisMonthSave">
                                空
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <div id="oil_fill_records_dlg" style="display: none; overflow: scroll;">
        <table id="oil_fill_records_table">
        </table>
    </div>
</body>
</html>
