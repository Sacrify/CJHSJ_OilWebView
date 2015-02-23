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

        var preRowStr = null;

        function UpdateConfigUI() {
            $("#seasonSet").combobox({
                valueField: 'id',
                textField: 'text',
                data: [{
                    id: '夏季',
                    text: '夏季'
                }, {
                    id: '冬季',
                    text: '冬季'
                }],
                readonly: true,
                onChange: function (newValue, oldValue) {

                    if (oldValue == '夏季') {
                        parent.oil_density_summer = ensurePositive(document.getElementById("oil_density").value);
                    }
                    else if (oldValue == '冬季') {
                        parent.oil_density_winter = ensurePositive(document.getElementById("oil_density").value);
                    }

                    if (newValue == '夏季') {
                        document.getElementById("oil_density").value = parent.getOilDensitySummer();
                    }
                    else if (newValue == '冬季') {
                        document.getElementById("oil_density").value = parent.getOilDensityWinter();
                    }
                }
            });

            document.getElementById("llun_rps_warning").value = parent.getWarningLlun();
            document.getElementById("lmain_oil_gps_warning").value = parent.getWarningLoil();
            document.getElementById("speed_warning").value = parent.getWarningSpeed();
            document.getElementById("ljyh_warning").value = parent.getWarningMoil();
            document.getElementById("rlun_rps_warning").value = parent.getWarningRlun();
            document.getElementById("rmain_oil_gps_warning").value = parent.getWarningRoil();

            if (parent.getOilType() == 0) {
                document.getElementById("oilTypeSet").value = "0号柴油";
            }
            else if (parent.getOilType() == 1) {
                document.getElementById("oilTypeSet").value = "4号柴油";
            }

            if (parent.getOilSeason() == 0) {
                $('#seasonSet').combobox('select', '夏季');
                document.getElementById("oil_density").value = parent.getOilDensitySummer();
            }
            else if (parent.getOilSeason() == 1) {
                $('#seasonSet').combobox('select', '冬季');
                document.getElementById("oil_density").value = parent.getOilDensityWinter();
            }
        }

        function SaveConfig() {
            var mmsi = parent.cur_mmsi;
            if (IsValidValue(mmsi) == false) {
                $.messager.show({
                    title: '请选择船只',
                    msg: '请先选择需要计时的船只',
                    showType: 'show'
                });
                return;
            }

            parent.warning_llun = document.getElementById("llun_rps_warning").value;
            parent.warning_loil = document.getElementById("lmain_oil_gps_warning").value;
            parent.warning_speed = document.getElementById("speed_warning").value;
            parent.warning_moil = document.getElementById("ljyh_warning").value;
            parent.warning_rlun = document.getElementById("rlun_rps_warning").value;
            parent.warning_roil = document.getElementById("rmain_oil_gps_warning").value;

            if ($('#seasonSet').combo('getText') == '夏季') {
                parent.oil_season = 0;
            }
            else if ($('#seasonSet').combo('getText') == '冬季') {
                parent.oil_season = 1;
            }

            if (parent.oil_season == 0) {
                parent.oil_density_summer = ensurePositive(document.getElementById("oil_density").value);
            }
            else if (parent.oil_season == 1) {
                parent.oil_density_winter = ensurePositive(document.getElementById("oil_density").value);
            }

            parent.SetShipConfig();
        }


        function ShowOilPrices() {

            $('#price_oil_type').html(parent.getOilTypeString());

            if (preRowStr == null) {
                var rowStr;

                rowStr += "<tr>";
                rowStr += "<td colspan='2'>";
                rowStr += "加载中请稍等";
                rowStr += "</td>";
                rowStr += "</tr>";

                preRowStr = $(rowStr).insertAfter($("#price_table_head"));
            }

            $.ajax({
                type: "get",
                dataType: "json",
                data: "oilType=" + parent.getOilType(),
                url: "ajax/shipoil_ajax.aspx?oper=getOilPrices",
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                    Cxw.Loading.hide();
                    alert(XmlHttpRequest.responseText);

                    var rowStrError;
                    rowStrError += "<tr>";
                    rowStrError += "<td colspan='2'>";
                    rowStrError += "无法取得油价信息";
                    rowStrError += "</td>";
                    rowStrError += "</tr>";

                    if (preRowStr != null) {
                        preRowStr.remove();
                        preRowStr = null;
                    }
                    preRowStr = $(rowStrError).insertAfter($("#price_table_head"));
                },
                success: function (json) {
                    if (json) {

                        if ((json == undefined || json == null || json == '') ||
                        (json != undefined && json != null && json != '' && eval(json).length == 0)) {

                            var rowStrNone;

                            rowStrNone += "<tr>";
                            rowStrNone += "<td colspan='2'>";
                            rowStrNone += "暂无油价数据";
                            rowStrNone += "</td>";
                            rowStrNone += "</tr>";

                            if (preRowStr != null) {
                                preRowStr.remove();
                                preRowStr = null;
                            }

                            preRowStr = $(rowStrNone).insertAfter($("#price_table_head"));

                        }
                        else {

                            var jsondata = eval(json); // convert json data
                            var rowStrPrice;

                            for (var i = 0; i < jsondata.length; i++) {

                                rowStrPrice += "<tr>";

                                rowStrPrice += "<td>";
                                var btime = OilHelper_GetPriceBTimeString(jsondata[i]);
                                rowStrPrice += btime.substr(0, btime.lastIndexOf(' '));
                                rowStrPrice += "</td>";

                                rowStrPrice += "<td>";
                                rowStrPrice += OilHelper_GetOilPrice(jsondata[i]);
                                rowStrPrice += "</td>";

                                rowStrPrice += "</tr>";
                            }

                            if (preRowStr != null) {
                                preRowStr.remove();
                                preRowStr = null;
                            }

                            preRowStr = $(rowStrPrice).insertAfter($("#price_table_head"));
                        }
                    }
                }
            });

            $("#price_dlg").show();

            $("#price_dlg").dialog({
                width: 400,
                height: 300,
                modal: false,
                title: '参考油价',
                minimizable: false,
                maximizable: false
            });
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
