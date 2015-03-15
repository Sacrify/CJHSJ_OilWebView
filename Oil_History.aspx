<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_History.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_History" %>

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
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/default/easyui.css" />
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/icon.css" />
    <link rel="Stylesheet" type="text/css" href="Styles/global.css" />

    <script type="text/javascript">

        $(function () {
            UpdateHistory();
        });

        function UpdateHistory() {
            UpdatePerUnit("yesterday");
            UpdatePerUnit("today");
            UpdatePerUnit("last_week");
            UpdatePerUnit("this_week");
            UpdatePerUnit("last_month");
            UpdatePerUnit("this_month");
            UpdatePerUnit("this_year");
        }

        function UpdatePerUnit(unit) {
            if (
            unit != "yesterday" &&
            unit != "today" &&
            unit != "last_week" &&
            unit != "this_week" &&
            unit != "last_month" &&
            unit != "this_month" &&
            unit != "this_year") {
                return;
            }

            var mmsi = parent.cur_mmsi;
            var timeName = "accu_time_" + unit;
            var milName = "accu_" + unit + "_mil";
            var sailTimeName = "accu_" + unit + "_sail_time";
            var runningTimeName = "accu_" + unit + "_running_time";
            var oilName = "accu_" + unit + "_oil";
            var oilCostName = "accu_" + unit + "_oil_cost";

            var btime = new Date();
            var etime = new Date();

            if (unit == "yesterday") {
                btime.setHours(0, 0, 0, 0);
                var btime_s = btime.getTime();

                btime.setTime(btime_s - 1000 * 60 * 60 * 24);
                etime.setTime(btime_s);

                $("#" + timeName).html((btime.getMonth() + 1) + "-" + btime.getDate());
            }

            else if (unit == "today") {
                btime.setHours(0, 0, 0, 0);
                var btime_s = btime.getTime();

                btime.setTime(btime_s);
                etime.setTime(btime_s + 1000 * 60 * 60 * 24);

                $("#" + timeName).html((btime.getMonth() + 1) + "-" + btime.getDate());
            }
            else if (unit == "last_week") {
                btime.setHours(0, 0, 0, 0);
                var btime_s = btime.getTime();

                btime_s -= 1000 * 60 * 60 * 24 * btime.getDay();
                btime.setTime(btime_s - 1000 * 60 * 60 * 24 * 7);
                etime.setTime(btime_s);

                $("#" + timeName).html(
                (btime.getMonth() + 1) + "-" + btime.getDate() + "--" +
                (etime.getMonth() + 1) + "-" + etime.getDate());
            }
            else if (unit == "this_week") {
                btime.setHours(0, 0, 0, 0);
                var btime_s = btime.getTime();

                btime_s -= 1000 * 60 * 60 * 24 * btime.getDay()
                btime.setTime(btime_s);
                etime.setTime(btime_s + 1000 * 60 * 60 * 24 * 7);

                $("#" + timeName).html(
                (btime.getMonth() + 1) + "-" + btime.getDate() + "--" +
                (etime.getMonth() + 1) + "-" + etime.getDate());
            }
            else if (unit == "last_month") {
                btime.setHours(0, 0, 0, 0);
                btime.setDate(1);
                var btime_s = btime.getTime();
                etime.setTime(btime_s);
                btime.setTime(btime_s - 1000 * 60 * 60);
                btime.setDate(1);
                btime.setHours(0, 0, 0, 0);

                $("#" + timeName).html(
                (btime.getMonth() + 1) + "-" + btime.getDate() + "--" +
                (etime.getMonth() + 1) + "-" + etime.getDate());
            }
            else if (unit == "this_month") {
                btime.setHours(0, 0, 0, 0);
                btime.setDate(1);

                var btime_s = btime.getTime();
                etime.setTime(btime_s + 1000 * 60 * 60 * 24 * 31);
                etime.setDate(1);

                $("#" + timeName).html(
                (btime.getMonth() + 1) + "-" + btime.getDate() + "--" +
                (etime.getMonth() + 1) + "-" + etime.getDate());
            }
            else if (unit == "this_year") {
                var bYear = btime.getFullYear();
                btime.setFullYear(bYear, 0, 1);
                btime.setHours(0, 0, 0, 0);

                etime.setFullYear(bYear + 1, 0, 1);
                etime.setHours(0, 0, 0, 0);

                $("#" + timeName).html(btime.getFullYear() + "年");
            }

            if (mmsi != "" && mmsi != "undefine") {
                $.ajax({
                    type: "get",
                    dataType: "json",
                    data: "mmsi=" + mmsi + "&btime=" + GetDateTimeString(btime) + "&etime=" + GetDateTimeString(etime),
                    url: "shipoil_ajax.aspx?oper=getOilHistoryStatistics",
                    error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                    success: function (json) {
                        if (json) {

                            $("#" + milName).html(OilHelper_GetMil(json));
                            $("#" + sailTimeName).html(OilHelper_GetSailTime(json));
                            $("#" + runningTimeName).html(OilHelper_GetRunningTime(json));
                            $("#" + oilName).html(OilHelper_GetOil(json));
                            $("#" + oilCostName).html(OilHelper_GetOilCost(json));
                        }
                    }
                });
            }
        }
    </script>

</head>
<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO">
            <div class="ch_content2" style="margin-top: 0;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left; margin-bottom: 2%;">
                    累计耗油</div>
                <table width="100%">
                    <tr>
                        <th scope="col">
                            累计单位
                        </th>
                        <th scope="col">
                            时间范围
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
                        <th scope="row">
                            昨日
                        </th>
                        <td id="accu_time_yesterday">
                            0
                        </td>
                        <td id="accu_yesterday_mil">
                            0
                        </td>
                        <td id="accu_yesterday_sail_time">
                            0
                        </td>
                        <td id="accu_yesterday_running_time">
                            0
                        </td>
                        <td id="accu_yesterday_oil">
                            0
                        </td>
                        <td id="accu_yesterday_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            今日
                        </th>
                        <td id="accu_time_today">
                            0
                        </td>
                        <td id="accu_today_mil">
                            0
                        </td>
                        <td id="accu_today_sail_time">
                            0
                        </td>
                        <td id="accu_today_running_time">
                            0
                        </td>
                        <td id="accu_today_oil">
                            0
                        </td>
                        <td id="accu_today_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            上周
                        </th>
                        <td id="accu_time_last_week">
                            0
                        </td>
                        <td id="accu_last_week_mil">
                            0
                        </td>
                        <td id="accu_last_week_sail_time">
                            0
                        </td>
                        <td id="accu_last_week_running_time">
                            0
                        </td>
                        <td id="accu_last_week_oil">
                            0
                        </td>
                        <td id="accu_last_week_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            本周
                        </th>
                        <td id="accu_time_this_week">
                            0
                        </td>
                        <td id="accu_this_week_mil">
                            0
                        </td>
                        <td id="accu_this_week_sail_time">
                            0
                        </td>
                        <td id="accu_this_week_running_time">
                            0
                        </td>
                        <td id="accu_this_week_oil">
                            0
                        </td>
                        <td id="accu_this_week_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            上月
                        </th>
                        <td id="accu_time_last_month">
                            0
                        </td>
                        <td id="accu_last_month_mil">
                            0
                        </td>
                        <td id="accu_last_month_sail_time">
                            0
                        </td>
                        <td id="accu_last_month_running_time">
                            0
                        </td>
                        <td id="accu_last_month_oil">
                            0
                        </td>
                        <td id="accu_last_month_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            本月
                        </th>
                        <td id="accu_time_this_month">
                            0
                        </td>
                        <td id="accu_this_month_mil">
                            0
                        </td>
                        <td id="accu_this_month_sail_time">
                            0
                        </td>
                        <td id="accu_this_month_running_time">
                            0
                        </td>
                        <td id="accu_this_month_oil">
                            0
                        </td>
                        <td id="accu_this_month_oil_cost">
                            0
                        </td>
                    </tr>
                    <tr>
                        <th scope="row">
                            本年
                        </th>
                        <td id="accu_time_this_year">
                            0
                        </td>
                        <td id="accu_this_year_mil">
                            0
                        </td>
                        <td id="accu_this_year_sail_time">
                            0
                        </td>
                        <td id="accu_this_year_running_time">
                            0
                        </td>
                        <td id="accu_this_year_oil">
                            0
                        </td>
                        <td id="accu_this_year_oil_cost">
                            0
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</body>
</html>
