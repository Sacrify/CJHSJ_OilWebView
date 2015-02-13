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
