<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="shipoil_list.aspx.cs" Inherits="CJHSJ_OilWebView.shipoil_list" %>

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

    <script type="text/javascript">
        var cur_mmsi = '<%=mmsiGot%>';
        var cur_shipname = '';
        var interval;
        var pageLoadTime;

        var warning_llun = 0;
        var warning_loil = 0;
        var warning_speed = 0;
        var warning_moil = 0;
        var warning_rlun = 0;
        var warning_roil = 0;

        var oil_type = 0;
        var oil_season = 0;
        var OIL_DEF_DENSITY_SUMMER = 0.84
        var OIL_DEF_DENSITY_WINTER = 0.86
        var oil_density_summer = OIL_DEF_DENSITY_SUMMER;
        var oil_density_winter = OIL_DEF_DENSITY_WINTER;

        $(function () {
            pageLoadTime = new Date();
            if (cur_mmsi != null && cur_mmsi != "") {
                $.ajax({
                    type: "get",
                    dataType: "json",
                    data: "mmsi=" + cur_mmsi,
                    url: "shipoil_ajax.aspx?oper=getShipInfo",
                    error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                    success: function (json) {
                        if (json.length > 0) {
                            var shipInfo = json[0];
                            cur_mmsi = shipInfo.mmsi;
                            cur_shipname = shipInfo.shipname;
                        }
                        else {
                            alert("未安装该项服务，如需安装请联系027-82767708");
                        }
                    }
                });
            }
        });

        function updateTitle(mmsi, shipname, x, y, time) {
            $("#divShipPos").panel({
                title: "当前船位 &nbsp;&nbsp;&nbsp;&nbsp;MMSI: " + mmsi + " &nbsp;&nbsp;&nbsp;&nbsp;船名: " + shipname + " &nbsp;&nbsp;&nbsp;&nbsp;经度: " + x + " &nbsp;&nbsp;&nbsp;&nbsp;纬度: " + y + " &nbsp;&nbsp;&nbsp;&nbsp;时间: " + time
            });
            $("#divRealTime").panel({
                title: '实时数显 (<font color="#C6E1F4">蓝色</font>为正常值, <font color="#E11017">红色</font>为异常值, <font color="#C0C0C0">灰色</font>为离线值)'
            });
        }

        function clearTitle() {
            $("#divShipPos").panel({
                title: "当前船位"
            });
            $("#divRealTime").panel({
                title: "实时数显"
            });
        }

        ///
        /// Config
        ///
        function GetShipConfig() {

            if (cur_mmsi == undefined ||
                cur_mmsi == null ||
                cur_mmsi == '') {

                resetConfigValues();
                return;
            }

            $.ajax({
                type: "get",
                dataType: "json",
                data: "mmsi=" + cur_mmsi,
                url: "ajax/shipoil_ajax.aspx?oper=getShipConfig",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (json) {
                    var shipConfig = json;
                    if ((shipConfig.hasOwnProperty('mmsi') == false) ||
                    (shipConfig.mmsi == undefined ||
                     shipConfig.mmsi == null ||
                     shipConfig.mmsi == '' ||
                     shipConfig.mmsi != cur_mmsi)) {
                        resetConfigValues();
                        return;
                    }

                    warning_llun = ensureValue(shipConfig.warning_llun);
                    warning_loil = ensureValue(shipConfig.warning_loil);
                    warning_speed = ensureValue(shipConfig.warning_speed);
                    warning_moil = ensureValue(shipConfig.warning_moil);
                    warning_rlun = ensureValue(shipConfig.warning_rlun);
                    warning_roil = ensureValue(shipConfig.warning_roil);
                    oil_type = ensureValue(shipConfig.oil_type);
                    oil_density_summer = ensureValueWithDef(shipConfig.oil_density_summer, OIL_DEF_DENSITY_SUMMER);
                    oil_density_winter = ensureValueWithDef(shipConfig.oil_density_winter, OIL_DEF_DENSITY_WINTER);

                    setConfigValues2UI();
                }
            });
        }

        function SetShipConfig() {
            $.ajax({
                type: "get",
                dataType: "json",
                data:
                "mmsi=" + cur_mmsi +
                "&llun=" + ensurePositive(warning_llun) +
                "&loil=" + ensurePositive(warning_loil) +
                "&speed=" + ensurePositive(warning_speed) +
                "&moil=" + ensurePositive(warning_moil) +
                "&rlun=" + ensurePositive(warning_rlun) +
                "&roil=" + ensurePositive(warning_roil) +
                "&oil_type=" + ensurePositive(oil_type) +
                "&oil_density_summer=" + ensurePositive(oil_density_summer) +
                "&oil_density_winter=" + ensurePositive(oil_density_winter),
                url: "ajax/shipoil_ajax.aspx?oper=setShipConfig",
                error: function (XmlHttpRequest, textStatus, errorThrown) { alert(XmlHttpRequest.responseText); },
                success: function (json) {
                    $.messager.show({
                        title: '保存成功',
                        msg: '配置信息保存成功',
                        showType: 'show'
                    });
                }
            });
        }
        

        function resetConfigValues() {
            warning_llun = 0;
            warning_loil = 0;
            warning_speed = 0;
            warning_moil = 0;
            warning_rlun = 0;
            warning_roil = 0;

            setConfigValues2UI();
        }

        function setConfigValues2UI() {
            var rightConfigWindow = document.getElementById("rightConfigFrame").contentWindow;
            if (rightConfigWindow) {
                rightConfigWindow.UpdateConfigUI();
            }
        }

        function getWarningLlun() {
            return ensurePositive(warning_llun);
        }

        function getWarningLoil() {
            return ensurePositive(warning_loil);
        }

        function getWarningSpeed() {
            return ensurePositive(warning_speed);
        }

        function getWarningMoil() {
            return ensurePositive(warning_moil);
        }

        function getWarningRlun() {
            return ensurePositive(warning_rlun);
        }

        function getWarningRoil() {
            return ensurePositive(warning_roil);
        }

        function getOilType() {
            return ensurePositive(oil_type);
        }

        function getOilTypeString() {
            var type = getOilType();

            if (type == 0) return "0号柴油";
            else if (type == 1) return "1号柴油";

            return "未知柴油";
        }

        function getOilSeason() {
            return ensurePositive(oil_season);
        }

        function getOilDensitySummer() {
            return ensurePositive(oil_density_summer);
        }

        function getOilDensityWinter() {
            return ensurePositive(oil_density_winter);
        }

    </script>

</head>
<body>
    <div id="tabs" class="easyui-tabs" fit="true" border="false">
        <div title="实时油耗">
            <div class="easyui-layout" style="width: 100%; height: 100%;">
                <div id="divShipPos" region="north" style="height: 290px" title="当前船位">
                    <iframe width="100%" height="100%" id="posFrame" scrolling="no" frameborder="0"
                        src="Oil_Shippos.aspx"></iframe>
                </div>
                <div id="divRealTime" region="center" style="height: auto; overflow:hidden" title="实时数显">
                    <iframe width="100%" height="100%" id="statFrame" scrolling="auto" frameborder="0"
                        src="Oil_RealTime.aspx"></iframe>
                </div>
            </div>
        </div>
        <div title="累计油耗">
<%--        <iframe width="100%" height="100%" id="rightHisStaFrame" scrolling="no" frameborder="0"
            src="Oil_rightHisSta.aspx"></iframe>--%>
        </div>
        <div title="历史报表">
<%--        <iframe width="100%" height="100%" id="rightHisFrame" scrolling="no" frameborder="0"
            src="Oil_rightHis.aspx"></iframe>--%>
        </div>
        <div title="报警设置" iconCls="icon-edit">
<%--        <iframe width="100%" height="100%" id="rightConfigFrame" scrolling="no" frameborder="0"
            src="Oil_rightConfig.aspx"></iframe>--%>
        </div>
        <div title="加油记录">
<%--            <iframe width="100%" height="100%" id="rightOilFillFrame" scrolling="auto" frameborder="0"
                src="Ship_OilFillRecords.aspx"></iframe>--%>
        </div>
    </div>
</body>
</html>
