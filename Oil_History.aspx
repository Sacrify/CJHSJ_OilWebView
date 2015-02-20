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
