<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Oil_Record.aspx.cs" Inherits="CJHSJ_OilWebView.Oil_Record" %>

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

</head>

<body>
    <div class="ch_all" style="color: #FFF;">
        <div class="ch_IOO2">
            <div class="ch_content2" style="margin-top: 0; margin-bottom: 20px;">
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    每月结存记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                1月（公斤）
                            </th>
                            <th scope="col">
                                2月（公斤）
                            </th>
                            <th scope="col">
                                3月（公斤）
                            </th>
                            <th scope="col">
                                4月（公斤）
                            </th>
                            <th scope="col">
                                5月（公斤）
                            </th>
                            <th scope="col">
                                6月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <input id="bal_jan" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_feb" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_mar" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_apr" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_may" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_jun" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                        </tr>
                    </table>
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                7月（公斤）
                            </th>
                            <th scope="col">
                                8月（公斤）
                            </th>
                            <th scope="col">
                                9月（公斤）
                            </th>
                            <th scope="col">
                                10月（公斤）
                            </th>
                            <th scope="col">
                                11月（公斤）
                            </th>
                            <th scope="col">
                                12月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <input id="bal_jul" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_aug" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_sep" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_oct" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_nov" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                            <td>
                                <input id="bal_dec" class="txtA" type="text" maxlength="20" name="" value="" />
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    本月加油记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1" id="oilFillRecordsTable">
                    </table>
                    <div style="width: 100%; float: left;">
                        <div class="ch_but0">
                            <input class="ch_but1" type="button" onclick="UpdateOilFillRecords();" value="编 辑" />
                        </div>
                    </div>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    每月加油记录</div>
                <div style="width: 100%; float: left; margin-bottom: 30px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                1月（公斤）
                            </th>
                            <th scope="col">
                                2月（公斤）
                            </th>
                            <th scope="col">
                                3月（公斤）
                            </th>
                            <th scope="col">
                                4月（公斤）
                            </th>
                            <th scope="col">
                                5月（公斤）
                            </th>
                            <th scope="col">
                                6月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="fill_jan">
                                空
                            </td>
                            <td id="fill_feb">
                                空
                            </td>
                            <td id="fill_mar">
                                空
                            </td>
                            <td id="fill_apr">
                                空
                            </td>
                            <td id="fill_may">
                                空
                            </td>
                            <td id="fill_jun">
                                空
                            </td>
                        </tr>
                    </table>
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                7月（公斤）
                            </th>
                            <th scope="col">
                                8月（公斤）
                            </th>
                            <th scope="col">
                                9月（公斤）
                            </th>
                            <th scope="col">
                                10月（公斤）
                            </th>
                            <th scope="col">
                                11月（公斤）
                            </th>
                            <th scope="col">
                                12月（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="fill_jul">
                                空
                            </td>
                            <td id="fill_aug">
                                空
                            </td>
                            <td id="fill_sep">
                                空
                            </td>
                            <td id="fill_oct">
                                空
                            </td>
                            <td id="fill_nov">
                                空
                            </td>
                            <td id="fill_dec">
                                空
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="ch_content1_tit" style="width: 100%; height: 25px; line-height: 25px;
                    line-height: 25px; float: left;">
                    本月结存（每月31日24时更新数据）</div>
                <div style="width: 100%; float: left; margin-bottom: 20px; margin-top: 5px;">
                    <table width="100%" border="0" cellspacing="1">
                        <tr>
                            <th scope="col">
                                上月结存（公斤）
                            </th>
                            <th scope="col">
                                本月加油（公斤）
                            </th>
                            <th scope="col">
                                本月油耗（公斤）
                            </th>
                            <th scope="col">
                                本月结存（公斤）
                            </th>
                        </tr>
                        <tr>
                            <td id="lastMonthSave">
                                空
                            </td>
                            <td id="thisMonthFill">
                                空
                            </td>
                            <td id="thisMonthConsume">
                                空
                            </td>
                            <td style="color: #0F0;" id="thisMonthSave">
                                空
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <div id="oil_fill_records_dlg" style="display: none; overflow: scroll;">
        <table id="oil_fill_records_table">
        </table>
    </div>
</body>
</html>
