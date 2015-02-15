<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_RealTime.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_RealTime" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="robots" content="noindex, nofollow" />
    <title></title>
    <script src="Scripts/jquery-easyui-1.4.1/jquery.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-easyui-1.4.1/jquery.easyui.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-easyui-1.4.1/easyloader.js" type="text/javascript"></script>
    <script src="Scripts/global.js" type="text/javascript"></script>
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/icon.css" />
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/default/easyui.css" />
    <link rel="Stylesheet" type="text/css" href="Styles/global.css" />

    <script type="text/javascript">
        var rpsMax = '3000';
        var rpmAlarm = '2000';
        var oilMax = '30'
        var isLoad = false;

        var lastUpdateTime;
        var timeUnchangedCount;
        var timeoutCount = 10;


        //累计参数变量
        var isTotal = false;
        var oilTotal = 0;
        var courseTotal = 0;
        var sTime;
        var eTime;

        var isOffline = false;

        function getRealtimeStat() {
            var mmsi = parent.cur_mmsi;
            var shipname = parent.cur_shipname;
            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + mmsi,
                url: "ajax/shipoil_ajax.aspx?oper=getRealtimeStat",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (json) {
                    if (json.length > 0) {
                        var shipDInfo = json[0];
                        var pclass = "ch_conent1_display";
                        var estime;

                        var recordSTime = ParseDateString(shipDInfo.stime);
                        if (lastUpdateTime == recordSTime) {
                            timeUnchangedCount++;
                            if (timeUnchangedCount >= timeoutCount) {
                                timeUnchangedCount = timeoutCount;
                            }
                        }
                        else {
                            lastUpdateTime = recordSTime;
                            timeUnchangedCount = 0;
                        }

                        isOffline = (timeUnchangedCount >= timeoutCount); //如果超过3分钟

                        if (isOffline) {
                            estime = shipDInfo.stime + "<font color='red'>(设备不在线)</font>";
                        } else {
                            estime = shipDInfo.stime;
                        }

                        //速度检测
                        shipDInfo.speed = ensurePositive(ensureNum(shipDInfo.speed));
                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningSpeed() != 0 && shipDInfo.speed > parent.getWarningSpeed()) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_display";
                        }
                        $("#speed").html(shipDInfo.speed.toFixed(0));
                        $("#speed").attr("class", pclass);

                        //左转速检测
                        shipDInfo.llunrps = ensurePositive(ensureNum(shipDInfo.llunrps));

                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningLlun() != 0 && shipDInfo.llunrps > parent.getWarningLlun()) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_display";
                        }

                        $("#llun_rps").html(shipDInfo.llunrps.toFixed(0));
                        $("#llun_rps").attr("class", pclass);

                        if (shipDInfo.lmpg_status == "" ||
                        shipDInfo.lmpg_status == undefined ||
                        shipDInfo.lmpg_status == null) {
                            shipDInfo.lmpg_status = 0;
                        }

                        //主机油耗仪开关 0 关  1 开
                        if (parseInt(shipDInfo.lmpg_status) == 0) {
                            $("#lmpg_status").html("■&nbsp;旁路");
                            $("#lmpg_status").attr("class", "state_danger");
                        }
                        else {
                            $("#lmpg_status").html("■&nbsp;正常");
                            $("#lmpg_status").attr("class", "state_normal");
                        }


                        //右转速检测
                        shipDInfo.rlunrps = ensurePositive(ensureNum(shipDInfo.rlunrps));

                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningRlun() != 0 && shipDInfo.rlunrps > parent.getWarningRlun()) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_display";
                        }
                        $("#rlun_rps").html(shipDInfo.rlunrps.toFixed(0));
                        $("#rlun_rps").attr("class", pclass);

                        //主机油耗仪开关 0 关  1 开
                        shipDInfo.rmpg_status = ensureValue(shipDInfo.rmpg_status);

                        if (parseInt(shipDInfo.rmpg_status) == 0) {
                            $("#rmpg_status").html("■&nbsp;旁路");
                            $("#rmpg_status").attr("class", "state_danger");
                        }
                        else {
                            $("#rmpg_status").html("■&nbsp;正常");
                            $("#rmpg_status").attr("class", "state_normal");
                        }


                        var lgps;
                        var rgps;
                        var lagps;
                        var ragps;

                        //左油耗检测
                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningLoil() != 0 && shipDInfo.lmain_gps > parent.getWarningLoil()) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_oil_display";
                        }
                        //判断是否为空
                        lgps = ensurePositive(ensureNum(shipDInfo.lmain_gps));
                        $("#lmain_oil_gps").html(lgps.toFixed(3));
                        $("#lmain_oil_gps").attr("class", pclass);

                        //右油耗检测
                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningRoil() != 0 && shipDInfo.rmain_gps > parent.getWarningRoil()) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_oil_display";
                        }
                        //判断是否为空
                        rgps = ensurePositive(ensureNum(shipDInfo.rmain_gps));
                        $("#rmain_oil_gps").html(rgps.toFixed(3));
                        $("#rmain_oil_gps").attr("class", pclass);

                        var allgps = ensurePositive(parseFloat(lgps) + parseFloat(rgps));

                        if (isOffline) {
                            pclass = "ch_content1_offline";
                        }
                        else if (parent.getWarningLoil() != 0 && parent.getWarningRoil() != 0 && allgps > (parent.getWarningLoil() + parent.getWarningRoil())) {
                            pclass = "ch_content1_alarm";
                        }
                        else {
                            pclass = "ch_conent1_oil_display";
                        }

                        $("#ljyh").html(allgps.toFixed(3));
                        $("#ljyh").attr("class", pclass);

                        $("#lrun_time").html(ensurePositive(ensureValue(shipDInfo.lrun_time)).toFixed(1));
                        $("#rrun_time").html(ensurePositive(ensureValue(shipDInfo.rrun_time)).toFixed(1));
                        $("#sail_time").html(ensurePositive(ensureValue(shipDInfo.sail_time)).toFixed(1));

                        //更新标题
                        parent.updateTitle(mmsi, shipname, shipDInfo.pos_x, shipDInfo.pos_y, estime);
                    }
                }
            });
        }

    </script>

</head>

<body style="color: #FFF;">
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO">
            <div class="ch_framework">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">左主机监控值&nbsp;&nbsp;<span id="lmpg_status" class="state_normal">■&nbsp;正常</span></li>
                    <li class="icon_txt_blue" style="width: 88%;">左主机转速</li>
                    <li style="height: 35px; line-height: 35px; text-align: center;">&nbsp;&nbsp;&nbsp;<span
                        id="llun_rps" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（转/分）</li>
                </ul>
                <div class="ch_A2">
                    <div class="icon_txt_yellow" style="margin-left: 2%; width: 88%;">
                        左主机油耗</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="lmain_oil_gps" class="ch_conent1_oil_display">0</span>&nbsp;&nbsp;&nbsp;（公斤）</div>
                </div>
                <div class="ch_A2" style="background-color: #145B89;">
                    <div class="icon_txt_green" style="width: 88%; margin-left: 2%;">
                        左主机运行时间</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="lrun_time" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（分钟）</div>
                </div>
            </div>
            <div class="ch_framework" style="margin-left: 2%;">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">船舶航速油耗</li>
                    <li class="icon_txt_blue" style="width: 88%;">航速</li>
                    <li style="height: 35px; line-height: 35px; text-align: center;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span
                        id="speed" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（公里/时）</li>
                </ul>
                <div class="ch_A2">
                    <div class="icon_txt_yellow" style="width: 88%; margin-left: 2%;">
                        实时油耗</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="ljyh" class="ch_conent1_oil_display">0</span>&nbsp;&nbsp;&nbsp;（公斤）</div>
                </div>
                <div class="ch_A2" style="background-color: #145B89;">
                    <div class="icon_txt_green" style="width: 88%; margin-left: 2%;">
                        航行时间</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="sail_time" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（分钟）</div>
                </div>
            </div>
            <div class="ch_framework" style="float: right;">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">右主机监控值&nbsp;&nbsp;<span id="rmpg_status" class="state_normal">■&nbsp;正常</span></li>
                    <li class="icon_txt_blue" style="width: 88%;">右主机转速</li>
                    <li style="height: 35px; line-height: 35px; text-align: center;">&nbsp;&nbsp;&nbsp;<span
                        id="rlun_rps" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（转/分）</li>
                </ul>
                <div class="ch_A2">
                    <div class="icon_txt_yellow" style="width: 88%; margin-left: 2%;">
                        右主机油耗</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="rmain_oil_gps" class="ch_conent1_oil_display">0</span>&nbsp;&nbsp;&nbsp;（公斤）</div>
                </div>
                <div class="ch_A2" style="background-color: #145B89;">
                    <div class="icon_txt_green" style="width: 88%; margin-left: 2%;">
                        右主机运行时间</div>
                    <div style="height: 35px; line-height: 35px; text-align: center;">
                        &nbsp;&nbsp;<span id="rrun_time" class="ch_conent1_display">0</span>&nbsp;&nbsp;&nbsp;（分钟）</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
