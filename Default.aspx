<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CJHSJ_OilWebView.DefaultPage" %>

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
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/default/easyui.css" />
    <link rel="Stylesheet" type="text/css" href="Scripts/jquery-easyui-1.4.1/themes/icon.css" />
</head>

<body>
    <div id="tabs" class="easyui-tabs" fit="true" border="false">
        <div title="实时油耗">
            <div class="easyui-layout" style="width: 100%; height: 100%;">
                <div data-options="region:'north'" style="height: 190px" title="North">
                </div>
                <div data-options="region:'center',iconCls:'icon-ok'" title="Center">
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
