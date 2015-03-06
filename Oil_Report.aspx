<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Report.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Report" %>

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

    <script type="text/javascript">

        function GetLastMonth(date) {
            var daysInMonth = new Array([0], [31], [28], [31], [30], [31], [30], [31], [31], [30], [31], [30], [31]);
            var strYear = date.getFullYear();
            var strDay = date.getDate();
            var strMonth = date.getMonth() + 1;
            if (strYear % 4 == 0 && strYear % 100 != 0) {
                daysInMonth[2] = 29;
            }
            if (strMonth - 1 == 0) {
                strYear -= 1;
                strMonth = 12;
            }
            else {
                strMonth -= 1;
            }
            strDay = daysInMonth[strMonth] >= strDay ? strDay : daysInMonth[strMonth];
            if (strMonth < 10) {
                strMonth = "0" + strMonth;
            }
            if (strDay < 10) {
                strDay = "0" + strDay;
            }

            date.setFullYear(strYear, strMonth - 1, strDay);
        }

        function GetLast3Month(date) {
            var daysInMonth = new Array([0], [31], [28], [31], [30], [31], [30], [31], [31], [30], [31], [30], [31]);
            var strYear = date.getFullYear();
            var strDay = date.getDate();
            var strMonth = date.getMonth() + 1;
            if (strYear % 4 == 0 && strYear % 100 != 0) {
                daysInMonth[2] = 29;
            }
            if (strMonth - 3 <= 0) {
                strYear -= 1;
                strMonth += 9;
            }
            else {
                strMonth -= 3;
            }
            strDay = daysInMonth[strMonth] >= strDay ? strDay : daysInMonth[strMonth];
            if (strMonth < 10) {
                strMonth = "0" + strMonth;
            }
            if (strDay < 10) {
                strDay = "0" + strDay;
            }

            date.setFullYear(strYear, strMonth - 1, strDay);
        }

        function EnsureMMSI() {
            if (IsValidValue(parent.cur_mmsi) == false) {
                $.messager.show({
                    title: '请选择船只',
                    msg: '请先选择需要导出报表的船只',
                    showType: 'show'
                });
                return false;
            }

            return true;
        }

        $(function () {

            //查询
            $("#search_btn").click(function () {
                UpdateHistoryStat();
            });

            //导出
            $("#export_year_btn").click(function () {
                if (EnsureMMSI() == false) return;

                $("#yearDatePicker").val(GetCurMonthString());
                $('#month_dlg').dialog('close');
                $('#year_dlg').dialog('open');
                $('#year_dlg').panel('resize', { width: 320, height: 120 });
//                $('#year_dlg').panel('move', {
//                    top: 200,
//                    left: 200
//                });
            });

            $("#export_year_btn2").click(function () {
                if (EnsureMMSI() == false) return;
                var btime = $('#yearDatePicker').val();
                ExportHistoryStat("year", btime);
                $('#year_dlg').dialog('close');
            });

            $("#export_cur_btn").click(function () {
                var btime = $('#beginTime').val();
                var etime = $('#endTime').val();
                var bdate = ParseDateString(btime.toString());
                var edate = ParseDateString(etime.toString());
                GetLast3Month(edate);
                if (bdate.getTime() < edate.getTime()) {
                    $.messager.show({
                        title: '时间限制(三个月以内)',
                        msg: '时间查询范围在三个月以内',
                        showType: 'show'
                    });
                    return;
                }

                ExportHistoryStat("cur", btime, etime);
            });

            $("#export_month_btn").click(function () {
                if (EnsureMMSI() == false) return;
                $("#monthDatePicker").val(GetCurMonthString());
                $('#year_dlg').dialog('close');
                $('#month_dlg').dialog('open');
                $('#month_dlg').panel('resize', { width: 320, height: 120 });
//                $('#month_dlg').panel('move', {
//                    top: 200,
//                    left: 200
//                });
            });

            $("#export_month_btn2").click(function () {
                if (EnsureMMSI() == false) return;
                var btime = $('#monthDatePicker').val();
                ExportHistoryStat("month", btime);
                $('#month_dlg').dialog('close');
            });

            $("#swiftTime_1month").click(function () {
                var now = new Date();
                var start = new Date();

                GetLastMonth(start);

                $("#endTime").val(GetDateString(now));
                $("#beginTime").val(GetDateString(start));
            });

            $("#swiftTime_3month").click(function () {
                var now = new Date();
                var start = new Date();

                GetLast3Month(start);

                $("#endTime").val(GetDateString(now));
                $("#beginTime").val(GetDateString(start));
            });

            //初始化默认值
            var mydate = new Date();
            var etime = GetDateString(mydate);
            $("#endTime").val(etime);
            GetLastMonth(mydate);
            var stime = GetDateString(mydate);
            $("#beginTime").val(stime);

            $('#month_dlg').dialog({
                title: '月报表打印,请选择月份:',
                iconCls: 'icon-save',
                closed: true,
                modal: false,
                draggable: true,
                resizable: false,
                zIndex: 9100
            });

            $('#year_dlg').dialog({
                title: '年报表打印,请选择年份:',
                iconCls: 'icon-save',
                closed: true,
                modal: false,
                draggable: true,
                resizable: false,
                zIndex: 9100
            });

        });

        var historyData;
        var onePage = 10;
        var curPage;
        var allPages;

        function reduceOnePage() {
            var tmpCurPage = curPage;
            curPage--;
            if (curPage < 0 || curPage > allPages) curPage = 0;
            if (curPage == tmpCurPage) return;
            UpdateHistoryTable();
        }

        function increaseOnePage() {
            var tmpCurPage = curPage;
            curPage++;
            if (curPage < 0 || curPage > allPages) curPage = allPages;
            if (curPage == tmpCurPage) return;
            UpdateHistoryTable();
        }

        function gotoOnePage() {
            var gotoPage = parseInt($("#gotoPage").val()) - 1;
            if (curPage == gotoPage) return;
            curPage = gotoPage;
            UpdateHistoryTable();
        }

        var previousRowStr = null;
        function UpdateHistoryTable() {

            if (curPage < 0 || curPage > allPages) curPage = 0;
            var rowStr = "";

            var index = 0;
            for (var i = curPage * onePage; i < (curPage * onePage + onePage) && i < historyData.length; i++, index++) {

                rowStr += "<tr>";

                rowStr += "<td>";
                rowStr += "<span>";
                rowStr += historyData[i].stime;
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "<td>";
                rowStr += "<span>";
                rowStr += OilHelper_GetMil(historyData[i]);
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "<td>";
                rowStr += "<span>";
                rowStr += OilHelper_GetSailTime(historyData[i]);
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "<td>";
                rowStr += "<span>";
                rowStr += OilHelper_GetRunningTime(historyData[i]);
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "<td>"
                rowStr += "<span>";
                rowStr += OilHelper_GetOil(historyData[i]);
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "<td>";
                rowStr += "<span>";
                rowStr += OilHelper_GetOilCost(historyData[i]);
                rowStr += "</span>";
                rowStr += "</td>";

                rowStr += "</tr>"
            }

            rowStr += "<tr align='center'>";
            rowStr += "<td colspan='6' style='color:White'>";

            rowStr += ("第" + (curPage + 1) + "页");
            rowStr += "&nbsp;&nbsp;";
            rowStr += ("共" + (allPages + 1) + "页");
            rowStr += "&nbsp;&nbsp;";

            rowStr += "到第";
            rowStr += "<input type='text' class='inputConfig' id='gotoPage' />";
            rowStr += "页";
            rowStr += "&nbsp;&nbsp;";
            rowStr += "<a id='hist_goto_btn' name='hist_goto_btn' href='#' style='color:#6AA011;' onclick='gotoOnePage();'>跳转</a>";

            rowStr += "&nbsp;&nbsp;&nbsp;&nbsp;";
            rowStr += "<a id='hist_pre_btn' name='hist_pre_btn' href='#' style='color:#6AA011;' onclick='reduceOnePage();'>上一页</a>";
            rowStr += "&nbsp;&nbsp;&nbsp;&nbsp;";
            rowStr += "<a id='hist_next_btn' name='hist_next_btn' href='#' style='color:#6AA011;' onclick='increaseOnePage();'>下一页</a>";
            rowStr += "</td>";
            rowStr += "</tr>";

            ClearHisStaTable();
            previousRowStr = $(rowStr).insertAfter($("#oil_hist_table_head"));
        }

        function UpdateHistoryStat() {

            if (IsValidValue(parent.cur_mmsi) == false) {
                return;
            }
            var mmsi = parent.cur_mmsi;

            var btime = $('#beginTime').val();
            var etime = $('#endTime').val();
            var bdate = ParseDateString(btime.toString());
            var edate = ParseDateString(etime.toString());
            GetLast3Month(edate);
            if (bdate.getTime() < edate.getTime()) {
                $.messager.show({
                    title: '时间限制(三个月以内)',
                    msg: '时间查询范围在三个月以内',
                    showType: 'show'
                });
                return;
            }

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&btime=" + $('#beginTime').val() + "&etime=" + $('#endTime').val(),
                url: "shipoil_ajax.aspx?oper=getOilHistoryStatistics",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (json) {
                    if (json) {
                        $("#query_btime").html($('#beginTime').val());
                        $("#query_etime").html($('#endTime').val());
                        $("#query_dist").html(OilHelper_GetMil(json));
                        $("#query_sail_time").html(OilHelper_GetSailTime(json));
                        $("#query_running_time").html(OilHelper_GetRunningTime(json));
                        $("#query_oil").html(OilHelper_GetOil(json));
                        $("#query_cost").html(OilHelper_GetOilCost(json));
                    }
                }
            });

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&btime=" + $('#beginTime').val() + "&etime=" + $('#endTime').val(),
                url: "shipoil_ajax.aspx?oper=getOilHistListNoPaging",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (d) {
                    var jsondata = eval(d); // convert json data
                    historyData = jsondata;

                    allPages = parseInt(historyData.length / onePage, 10);
                    curPage = 0;

                    // [0, allPages];

                    UpdateHistoryTable();
                }
            });
        }

        function ClearHisStaTable() {
            if (previousRowStr != null) {
                previousRowStr.remove();
                previousRowStr = null;
            }
        }

        function ClearHistStaResult() {
            $("#query_btime").html(0);
            $("#query_etime").html(0);
            $("#query_dist").html(0);
            $("#query_sail_time").html(0);
            $("#query_running_time").html(0);
            $("#query_oil").html(0);
            $("#query_cost").html(0);
            ClearHisStaTable();
        }

        function ExportHistoryStat() {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;
            var form = $("#exportExcelForm");
            var inputMMSI = $("#exportMMSI");
            inputMMSI.attr('value', mmsi);

            if (arguments.length >= 1) {
                var exportType = $("#exportType");
                exportType.attr('value', arguments[0]);
            }

            if (arguments.length >= 2) {
                var btime = $("#btime");
                btime.attr('value', arguments[1]);
            }

            if (arguments.length >= 3) {
                var etime = $("#etime");
                etime.attr('value', arguments[2]);
            }

            form.submit();
        }

    </script>

    <style type="text/css">
        *
        {
            margin: 0;
            padding: 0;
            font-family: "宋体" , Arial, "新宋体";
            font-size: 12px;
            list-style: none;
        }
        .clear
        {
            clear: both;
            height: 0;
            visibility: hidden;
        }
        a
        {
            text-decoration: none;
            cursor: pointer;
        }
        a:hover
        {
            color: red;
            text-decoration: underline;
        }
        a img
        {
            border: 0;
            vertical-align: middle;
        }
    </style>

</head>

<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO2">
            <div class="ch_content2" style="margin-top: 0;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    查询条件</div>
                <div style="width: 100%; float: left;">
                    <div class="ch_nmb">
                        <div style="width: 100%; height: 15px; float: left; margin-bottom: 10px;">
                        &nbsp;&nbsp;开始时间：
                        <input id="beginTime" name="beginTime" type="text" onclick="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                            class="Wdate dataTime" style="width: 140px;" value="" />

                        &nbsp;&nbsp;结束时间：
                        <input id="endTime" name="endTime" type="text" onclick="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                            class="Wdate dataTime" style="width: 140px;" value="" />
                        </div>
                        <br />
                        <input class="ch_but3" id="swiftTime_1month" type="button" value="一个月内" />
                        <input class="ch_but3" id="swiftTime_3month" type="button" value="三个月内" />
                        <input class="ch_but3" id="search_btn" type="button" value="查&nbsp;&nbsp;询" />

                        &nbsp;&nbsp;
                        <input id="export_cur_btn" name="export_cur_btn" class="ch_but5" type="button" value="查询打印" />
                        <input id="export_year_btn" name="export_year_btn" class="ch_but5" type="button" value="年报表打印" />
                        <input id="export_month_btn" name="export_month_btn" class="ch_but5" type="button" value="月报表打印" />

                    </div>
                </div>

                <div style="width: 100%; float: left; margin: 2% 0;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                起始时间
                            </th>
                            <th scope="col">
                                结束时间
                            </th>
                            <th scope="col">
                                里程（公里）
                            </th>
                            <th scope="col">
                                航行时间（小时）
                            </th>
                            <th scope="col">
                                运行时间（小时）
                            </th>
                            <th scope="col">
                                油耗（公斤）
                            </th>
                            <th scope="col">
                                参考油价（元）
                            </th>
                        </tr>
                        <tr>
                            <td id="query_btime">
                                0
                            </td>
                            <td id="query_etime">
                                0
                            </td>
                            <td id="query_dist">
                                0
                            </td>
                            <td id="query_sail_time">
                                0
                            </td>
                            <td id="query_running_time">
                                0
                            </td>
                            <td id="query_oil">
                                0
                            </td>
                            <td id="query_cost">
                                0
                            </td>
                        </tr>
                    </table>
                </div>
                <div style="width: 100%; float: left;">
                    <table id="oil_hist_table" width="100%" border="0" cellspacing="1" cellpadding="1">
                        <tr id="oil_hist_table_head">
                            <th scope="col">
                                起始时间
                            </th>
                            <th scope="col">
                                里程（公里）
                            </th>
                            <th scope="col">
                                航行时间（小时）
                            </th>
                            <th scope="col">
                                运行时间（小时）
                            </th>
                            <th scope="col">
                                油耗（公斤）
                            </th>
                            <th scope="col">
                                参考油价（元）
                            </th>
                        </tr>
                    </table>
                </div>

             <div id="report_buttons">
                    <div id="month_dlg" style="text-align:center; margin-top:20px;">
                            <input id="monthDatePicker" name="monthDatePicker" type="text" onclick="WdatePicker({dateFmt:'yyyy-MM'})"
                                class="Wdate dataTime" style="width: 140px;" value="" />
                            &nbsp;
                            <input id="export_month_btn2" name="export_month_btn2" class="ch_but3" type="button"
                                value="月报表打印" />
                    </div>

                    <div id="year_dlg" style="text-align:center; margin-top:20px;">
                            <input id="yearDatePicker" name="yearDatePicker" type="text" onclick="WdatePicker({dateFmt:'yyyy-MM'})"
                                class="Wdate dataTime" style="width: 140px;" value="" />
                            &nbsp;
                            <input id="export_year_btn2" name="export_year_btn2" class="ch_but3" type="button"
                                value="年报表打印" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div style="display: none">
        <form id="exportExcelForm" method="get" action="shipoil_ajax.aspx" target="_self" style="display: none">
        <input name="oper" type="text" value="exportHistoryStatusToExcel" />
        <input name="mmsi" type="text" id="exportMMSI" />
        <input name="exportType" type="text" id="exportType" />
        <input name="btime" type="text" id="btime" />
        <input name="etime" type="text" id="etime" />
        </form>
    </div>
</body>
</html>
