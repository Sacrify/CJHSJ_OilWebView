<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Shippos.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Shippos" %>

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

    <script type="text/javascript">
        $(function () {
            var mmsi = parent.cur_mmsi;
            var remoteUrl = '<%=remoteUrl%>';
            var remoteUrlNoMMSI = '<%=remoteUrlNoMMSI%>';
            if (mmsi != '' && mmsi != 'null' && mmsi != null) {
                var myUrl = remoteUrl + "?mmsi=" + mmsi;
                window.location.href = myUrl;
            } else {
                window.location.href = remoteUrlNoMMSI;
            }
        });
    </script>

</head>
<body>
    <span id="msg" style="color:red;display:none">请选择船只</span>
</body>
</html>
