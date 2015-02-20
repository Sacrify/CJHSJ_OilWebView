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

        function getLastMonth(date) {
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

        function getLast3Month(date) {
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
            var mmsi = parent.cur_mmsi;

            if (IsValidValue(mmsi) == false) {
                $.messager.show({
                    title: '请选择船只',
                    msg: '请先选择需要导出报表的船只',
                    showType: 'show'
                });
                return false;
            }

            return true;
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
