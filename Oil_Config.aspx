<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Config.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Config" %>

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
        
        function UpdateConfigUI() {
            
        }

    </script>

</head>
<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO">
            <div class="ch_framework">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">左主机</li>
                    <li class="icon_txt_blue" style="width: 90%;">左主机转速报警值</li>
                    <li>
                        <input type="text" id="llun_rps_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （转/分）</li>
                    <li class="icon_txt_yellow" style="width: 90%;">左主机耗油报警值</li>
                    <li>
                        <input type="text" id="lmain_oil_gps_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （公斤）</li>
                </ul>
            </div>
            <div class="ch_framework" style="margin-left: 2%;">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">船舶航速/油耗</li>
                    <li class="icon_txt_green" style="width: 90%;">航速报警值</li>
                    <li>
                        <input type="text" id="speed_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （公里/小时）</li>
                    <li class="icon_txt_yellow" style="width: 90%;">月流量上限</li>
                    <li>
                        <input type="text" id="ljyh_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （公斤）</li>
                </ul>
            </div>
            <div class="ch_framework" style="float: right;">
                <ul class="ch_content1">
                    <li class="ch_content1_tit">右主机</li>
                    <li class="icon_txt_blue" style="width: 90%;">右主机转速报警值</li>
                    <li>
                        <input type="text" id="rlun_rps_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （转/分）</li>
                    <li class="icon_txt_yellow" style="width: 90%;">右主机耗油报警值</li>
                    <li>
                        <input type="text" id="rmain_oil_gps_warning" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （公斤）</li>
                </ul>
            </div>
        </div>
        <div class="ch_IOO">
            <div class="ch_content2">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    float: left;">
                    油设置</div>
                <div class="ch_content2a">
                    <div class="icon_txt_blue">
                        油品类型</div>
                    <div class="ch_xiala" style="margin-left: 10%; width: 90%">
                        <input id="oilTypeSet" class="ch_xiala_input2" style="width: 80px; height: 25px;"
                            onfocus="if(this.value=='')this.value='';else this.select();" />
                        <input id="seasonSet" class="ch_xiala_input2" style="width: 55px; height: 25px;"
                            onfocus="if(this.value=='')this.value='';else this.select();" />
                    </div>
                </div>
                <div class="ch_content2a" style="margin-left: 2%;">
                    <div class="icon_txt_yellow">
                        油品密度</div>
                    <div class="ch_xiala">
                        <input type="text" id="oil_density" class="ch_xiala_input" value="" onfocus="if(this.value=='')this.value='';else this.select();" />
                        （千克/立方米）</div>
                </div>
                <div class="ch_content2a" style="float: right;">
                    <div class="icon_txt_blue">
                        参考油价</div>
                    <div class="ch_xiala">
                        <input type="button" id="oil_price" class="ch_xiala_input" onclick="ShowOilPrices();"
                            value="点击详细" />
                        <!-- （元/千克） -->
                    </div>
                </div>
            </div>
            <div class="ch_IOO">
                <div class="ch_but">
                    <input class="ch_but1" type="button" onclick="SaveConfig();" value="确定" />
                    <input class="ch_but2" type="button" onclick="UpdateConfigUI();" value="取消" />
                </div>
            </div>
        </div>
        <div id="price_dlg" style="text-align: center; overflow: scroll; display: none;">
            <span id="price_oil_type" class="ch_content1_tit"></span>
            <br />
            <table id="price_table" width="100%" border="0" cellspacing="1" cellpadding="1">
                <tr id="price_table_head">
                    <td>
                        时间
                    </td>
                    <td>
                        油价（元）
                    </td>
                </tr>
            </table>
        </div>
    </div>
</body>
</html>
