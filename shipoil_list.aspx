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


    </script>

</head>
<body>
    <div id="tabs" class="easyui-tabs" fit="true" border="false">
        <div title="实时油耗">
            <div class="easyui-layout" style="width: 100%; height: 100%;">
                <div region="north" style="height: 290px" title="当前船位">
                    <iframe width="100%" height="100%" id="posFrame" scrolling="no" frameborder="0"
                        src="Oil_Shippos.aspx"></iframe>
                </div>
                <div region="center" style="height:auto" title="实时数显">
                    <iframe width="100%" height="100%" id="statFrame" scrolling="no" frameborder="0"
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
