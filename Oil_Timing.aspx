<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Timing.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Timing" %>

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

        var isTiming = false;

        var timingStartTime = null; // set as localtime string
        var timingEndTime = null;  // set as localtime string

        var oilAccu = 0;
        var oilAccuCost = 0;
        var milAccu = 0;

        var oilDyna = 0;
        var oilDynaCost = 0;
        var milDyna = 0;

        var oilEx = 0;
        var oilExCost = 0;

        var sailRime = 0;
        var runningRime = 0;

        var oilTotal = 0;
        var oilCostTotal = 0;
        var milTotal = 0;


        $(function () {
            //初始化累计cookie
            LoadCookie();
        });

        function EnsureMMSI() {
            if (IsValidValue(parent.mmsi) == false) {
                $.messager.show({
                    title: '请选择船只',
                    msg: '请先选择需要计时的船只',
                    showType: 'show'
                });
                return false;
            }

            return true;
        }

        function RefreshRealTimeUI() {
            $("#timing_start_time").html(timingStartTime);
            $("#timing_mil").html(EnsurePositive(milTotal).toFixed(2));
            $("#timing_sail_time").html(EnsurePositive(sailTime).toFixed(1));
            $("#timing_running_time").html(EnsurePositive(runningTime).toFixed(1));
            $("#timing_oil").html(EnsurePositive(oilTotal).toFixed(3));
            $("#timing_oil_cost").html(EnsurePositive(oilCostTotal).toFixed(2));
        }

        function ResetRealTimeUI() {
            $("#timing_start_time").html(0);
            $("#timing_mil").html(0);
            $("#timing_sail_time").html(0);
            $("#timing_running_time").html(0);
            $("#timing_oil").html(0);
            $("#timing_oil_cost").html(0);
        }

        function ResetHistTimeUI() {
            $("#hist_timing_start_time").html(0);
            $("#hist_timing_end_time").html(0);
            $("#hist_timing_mil").html(0);
            $("#hist_timing_sail_time").html(0);
            $("#hist_timing_running_time").html(0);
            $("#hist_timing_oil").html(0);
            $("#hist_timing_oil_cost").html(0);
        }

        function ResetUI() {
            ResetRealTimeUI();
            ResetHistTimeUI();
        }

        function ResetRealTimeData() {
            oilAccu = 0;
            oilAccuCost = 0;
            milAccu = 0;

            oilDyna = 0;
            oilDynaCost = 0;
            milDyna = 0;

            oilEx = 0;
            oilExCost = 0;

            sailTime = 0;
            runningTime = 0;
        }

        function LoadCookie() {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            ResetUI();

            var histstime = getCookie("timing_hist_stime" + mmsi);
            var histetime = getCookie("timing_hist_etime" + mmsi);
            var histmil = getCookie("timing_hist_mil" + mmsi);
            var histsailtime = getCookie("timing_hist_sailtime" + mmsi);
            var histruntime = getCookie("timing_hist_runningtime" + mmsi);
            var histoil = getCookie("timing_hist_oil" + mmsi);
            var histoilcost = getCookie("timing_hist_oil_cost" + mmsi);

            $("#hist_timing_start_time").html(IsValidValue(histstime) ? "0" : histstime);
            $("#hist_timing_end_time").html(IsValidValue(histetime) ? "0" : histetime);
            $("#hist_timing_mil").html(IsValidValue(histmil) ? "0" : histmil);
            $("#hist_timing_sail_time").html(IsValidValue(histsailtime) ? "0" : histsailtime);
            $("#hist_timing_running_time").html(IsValidValue(histruntime) ? "0" : histruntime);
            $("#hist_timing_oil").html(IsValidValue(histoil) ? "0" : histoil);
            $("#hist_timing_oil_cost").html(IsValidValue(histoilcost) ? "0" : histoilcost);

            //初始化累计变量
            isTiming = getCookie("isTiming" + mmsi);
            if (IsValidValue(isTiming) && isTiming == "true") {
                isTiming = true;
                var cdt = getCookie("sTime" + mmsi);
                timingEndTime = timingStartTime = IsValidValue(cdt) ? cdt : GetCurDateTimeString();
                UpdateTiming();
            } else {
                isTiming = false;
            }
        }

        function SaveTiming2Hist() {
            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            $("#hist_timing_start_time").html(EnsureValue(timingStartTime));
            $("#hist_timing_end_time").html(EnsureValue(timingEndTime));
            $("#hist_timing_mil").html(EnsurePositive(milTotal).toFixed(2));
            $("#hist_timing_sail_time").html(EnsurePositive(sailTime).toFixed(1));
            $("#hist_timing_running_time").html(EnsurePositive(runningTime).toFixed(1));
            $("#hist_timing_oil").html(EnsurePositive(oilTotal).toFixed(3));
            $("#hist_timing_oil_cost").html(EnsurePositive(oilCostTotal).toFixed(2));

            delCookie("timing_hist_stime" + mmsi);
            delCookie("timing_hist_etime" + mmsi);
            delCookie("timing_hist_sailtime" + mmsi);
            delCookie("timing_hist_runningtime" + mmsi);
            delCookie("timing_hist_oil" + mmsi);
            delCookie("timing_hist_oil_cost" + mmsi);
            delCookie("timing_hist_mil" + mmsi);

            setCookie("timing_hist_stime" + mmsi, EnsureValue(timingStartTime));
            setCookie("timing_hist_etime" + mmsi, EnsureValue(timingEndTime));
            setCookie("timing_hist_mil" + mmsi, EnsurePositive(milTotal).toFixed(2));
            setCookie("timing_hist_sailtime" + mmsi, EnsurePositive(sailTime).toFixed(1));
            setCookie("timing_hist_runningtime" + mmsi, EnsurePositive(runningTime).toFixed(1));
            setCookie("timing_hist_oil" + mmsi, EnsurePositive(oilTotal).toFixed(3));
            setCookie("timing_hist_oil_cost" + mmsi, EnsurePositive(oilCostTotal).toFixed(2));
        }

        function StartTiming() {
            $.messager.confirm('启动累计', (isTiming == true) ? '确认重新启动累计？' : '确认启动累计？', function (r) {
                if (r) {
                    if (EnsureMMSI() == false) return;
                    var mmsi = parent.cur_mmsi;

                    if (isTiming == true) {
                        SaveTiming2Hist();
                    }
                    isTiming = true;

                    //更新是否启动累计cookie
                    setCookie("isTiming" + mmsi, "true");

                    //设置起时间
                    timingEndTime = timingStartTime = GetCurDateTimeString();
                    setCookie("sTime" + mmsi, timingStartTime);

                    ResetRealTimeData();
                    RefreshRealTimeUI();
                }
            });
        }

        function StopTiming() {
            $.messager.confirm('关闭累计', '确认关闭启动累计？', function (r) {
                if (r) {
                    if (EnsureMMSI() == false) return;
                    var mmsi = parent.cur_mmsi;

                    if (isTiming == true) {
                        SaveTiming2Hist();
                    }
                    isTiming = false;
                    delCookie("isTiming" + mmsi);
                    ResetRealTimeUI();
                }
            });
        }

        function UpdateTiming() {
            if (isTiming == false) return;

            if (EnsureMMSI() == false) return;
            var mmsi = parent.cur_mmsi;

            var curTime = GetCurDateTimeString();
            if (IsValidValue(timingStartTime) == false) {
                timingEndTime = timingStartTime = curTime;
                return;
            }

            var deltaMonth = GetDateDiff(
            curTime,
            timingStartTime,
            "Month");

            if (deltaMonth >= 3) {
                $.messager.show({
                    title: '时间限制(三个月以内)',
                    msg: '时间查询范围在三个月以内',
                    showType: 'show'
                });
                return;
            }
            timingEndTime = curTime;

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi + "&btime=" + timingStartTime + "&etime=" + timingEndTime,
                url: "ajax/shipoil_ajax.aspx?oper=getOilTimingStatistics",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (json) {
                    var oilTimingStatInfo = json;
                    if (oilTimingStatInfo.hasOwnProperty('datetiming') == false) {
                        return;
                    }

                    if (ParseDateString(oilTimingStatInfo.datetiming).getTime() != 
                        ParseDateString(timingStartTime).getTime()) {
                        return;
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oil_accu')) {
                        if (oilTimingStatInfo.oil_accu != null) {
                            oilAccu = parseFloat(oilTimingStatInfo.oil_accu);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oilcost_accu')) {
                        if (oilTimingStatInfo.oilcost_accu != null) {
                            oilAccuCost = parseFloat(oilTimingStatInfo.oilcost_accu);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('mil_accu')) {
                        if (oilTimingStatInfo.mil_accu != null) {
                            milAccu = parseFloat(oilTimingStatInfo.mil_accu);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oil_ex')) {
                        if (oilTimingStatInfo.oil_ex != null) {
                            oilEx = parseFloat(oilTimingStatInfo.oil_ex);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oilcost_ex')) {
                        if (oilTimingStatInfo.oilcost_ex != null) {
                            oilExCost = parseFloat(oilTimingStatInfo.oilcost_ex);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oil_dyna')) {
                        if (oilTimingStatInfo.oil_dyna != null) {
                            oilDyna = parseFloat(oilTimingStatInfo.oil_dyna);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('oilcost_dyna')) {
                        if (oilTimingStatInfo.oilcost_dyna != null) {
                            oilDynaCost = parseFloat(oilTimingStatInfo.oilcost_dyna);
                        }
                    }


                    if (oilTimingStatInfo.hasOwnProperty('mil_dyna')) {
                        if (oilTimingStatInfo.mil_dyna != null) {
                            milDyna = parseFloat(oilTimingStatInfo.mil_dyna);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('sail_time')) {
                        if (oilTimingStatInfo.sail_time != null) {
                            sailTime = parseFloat(oilTimingStatInfo.sail_time);
                        }
                    }

                    if (oilTimingStatInfo.hasOwnProperty('running_time')) {
                        if (oilTimingStatInfo.running_time != null) {
                            runningTime = parseFloat(oilTimingStatInfo.running_time);
                        }
                    }

                    oilTotal = parseFloat(oilAccu) + parseFloat(oilDyna) + parseFloat(oilEx);
                    oilCostTotal = parseFloat(oilAccuCost) + parseFloat(oilDynaCost) + parseFloat(oilExCost);
                    milTotal = parseFloat(milAccu) + parseFloat(milDyna);
                    RefreshRealTimeUI();
                }
            });
        }

    
    </script>

</head>
<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO" style="margin-bottom: 2%;">
            <div class="ch_content2" style="margin-top: 0;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    float: left;">
                    历史计时</div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_blue" style="width: 155px;">
                        起始时刻</div>
                    <div class="content2C_no" id="hist_timing_start_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_blue" style="width: 155px;">
                        结束时刻</div>
                    <div class="content2C_no" id="hist_timing_end_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_yellow" style="width: 155px;">
                        计时里程（公里）</div>
                    <div class="content2C_no" id="hist_timing_mil">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_green" style="width: 155px;">
                        航行时间（小时）</div>
                    <div class="content2C_no" id="hist_timing_sail_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_green" style="width: 155px;">
                        运行时间（小时）</div>
                    <div class="content2C_no" id="hist_timing_running_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_blue" style="width: 155px;">
                        计时耗油（公斤）</div>
                    <div class="content2C_no" id="hist_timing_oil">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_yellow" style="width: 155px;">
                        参考油价（元）</div>
                    <div class="content2C_no" id="hist_timing_oil_cost">
                        0</div>
                </div>
            </div>
        </div>
        <div class="ch_IOO">
            <div class="ch_content2" style="margin-top: 0;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    float: left;">
                    当前计时</div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_blue" style="width: 155px;">
                        起始时刻</div>
                    <div class="content2C_no" id="timing_start_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_yellow" style="width: 155px;">
                        计时里程（公里）</div>
                    <div class="content2C_no" id="timing_mil">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_green" style="width: 155px;">
                        航行时间（小时）</div>
                    <div class="content2C_no" id="timing_sail_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_green" style="width: 155px;">
                        运行时间（小时）</div>
                    <div class="content2C_no" id="timing_running_time">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_blue" style="width: 155px;">
                        计时耗油（公斤）</div>
                    <div class="content2C_no" id="timing_oil">
                        0</div>
                </div>
                <div class="ch_content2C" style="margin-right: 20px; border-bottom: #4396cc dashed 1px;">
                    <div class="icon_txt_yellow" style="width: 155px;">
                        参考油价（元）</div>
                    <div class="content2C_no" id="timing_oil_cost">
                        0</div>
                </div>
            </div>
        </div>
        <div class="ch_IOO">
            <div class="ch_but">
                <a id="astop" class="easyui-linkbutton" href="javascript:void(0)" style="background-color: #60BB04;"
                    plain="true" onclick="StopTiming();">&nbsp;清零计时&nbsp;</a> <a id="astart"
                        class="easyui-linkbutton" href="javascript:void(0)" style="background-color: #FFC600;"
                        plain="true" onclick="StartTiming();">&nbsp;启动计时&nbsp;</a>
            </div>
        </div>
    </div>
</body>
</html>
